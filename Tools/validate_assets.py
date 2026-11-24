#!/usr/bin/env python3
"""
Unity-free asset validation for JohnWickArsenal.
Validates GLB/OBJ models, textures, project structure, and manifest files.
Runs without Unity - suitable for GitHub Actions / Chromebook.
"""

import os
import sys
import json
from pathlib import Path
from typing import List, Dict, Any, Tuple

try:
    import pygltflib
    import trimesh
    PYGLTFLIB_AVAILABLE = True
except ImportError:
    PYGLTFLIB_AVAILABLE = False

PROJECT_ROOT = Path(__file__).resolve().parents[1]
UNITY_PROJECT = PROJECT_ROOT / "UnityProject"
ASSETS_ROOT = UNITY_PROJECT / "Assets" / "JohnWickArsenal"

# Expected assets for Tier 1 cosmetic test
REQUIRED_MODELS = {
    # Original John Wick weapons (petroulacl FPS Kit - CC0)
    "Models/Pistols/TTI_PitViper.glb": {"type": "glb", "source": "petroulacl", "license": "CC0", "max_tris": 20000, "has_textures": False},
    "Models/Pistols/Glock_34.glb": {"type": "glb", "source": "petroulacl", "license": "CC0", "max_tris": 20000, "has_textures": False},
    
    # Vinish.dev stand-ins (CC BY 4.0)
    "Models/DisplayModels/VinishWeapons/AR15_AssaultRifle.glb": {"type": "glb", "source": "vinish", "license": "CC BY 4.0", "max_tris": 20000, "has_textures": True},
    "Models/DisplayModels/VinishWeapons/AWP_SniperRifle.glb": {"type": "glb", "source": "vinish", "license": "CC BY 4.0", "max_tris": 20000, "has_textures": True},
    "Models/DisplayModels/VinishWeapons/MP5_SubmachineGun.glb": {"type": "glb", "source": "vinish", "license": "CC BY 4.0", "max_tris": 20000, "has_textures": True},
    "Models/DisplayModels/VinishWeapons/Remington870_PumpShotgun.glb": {"type": "glb", "source": "vinish", "license": "CC BY 4.0", "max_tris": 20000, "has_textures": True},
    
    # ClassicBlackSuit (OBJ + textures)
    "Models/Suits/ClassicBlackSuit.obj": {"type": "obj", "source": "makehuman", "license": "CC0", "max_tris": 25000, "has_textures": True},
    "Models/Suits/ClassicBlackSuit.mtl": {"type": "mtl", "source": "makehuman", "license": "CC0"},
    "Models/Suits/ClassicBlackSuit_Albedo.png": {"type": "png", "source": "makehuman", "license": "CC0"},
    "Models/Suits/ClassicBlackSuit_Bump.png": {"type": "png", "source": "makehuman", "license": "CC0"},
}

REQUIRED_PREFERBS = [
    "Prefabs/DisplayProps/DisplayPistol_TTI_PitViper.prefab",
]

REQUIRED_SCRIPTS = [
    "Assets/JohnWickArsenal/Editor/JWA_DisplayPrefabGenerator.cs",
    "Assets/JohnWickArsenal/Editor/JohnWickArsenal.Cosmetic.Editor.asmdef",
]

REQUIRED_CONFIGS = [
    "Packages/manifest.json",
    "ProjectSettings/ProjectVersion.txt",
    "ProjectSettings/ProjectSettings.asset",
]

class ValidationResult:
    def __init__(self):
        self.errors: List[str] = []
        self.warnings: List[str] = []
        self.info: List[str] = []
    
    def add_error(self, msg: str):
        self.errors.append(msg)
    
    def add_warning(self, msg: str):
        self.warnings.append(msg)
    
    def add_info(self, msg: str):
        self.info.append(msg)
    
    def has_errors(self) -> bool:
        return len(self.errors) > 0

def validate_file_exists(path: Path, description: str) -> bool:
    if not path.exists():
        return False
    if path.stat().st_size == 0:
        return False
    return True

def validate_glb(path: Path, spec: Dict) -> Tuple[List[str], List[str]]:
    """Validate GLB model using pygltflib and trimesh."""
    errors = []
    warnings = []
    
    if not PYGLTFLIB_AVAILABLE:
        warnings.append(f"pygltflib/trimesh not available, skipping GLB validation for {path}")
        return errors, warnings
    
    try:
        gltf = pygltflib.GLTF2().load_binary(str(path))
        
        # Basic structure checks
        if len(gltf.meshes) == 0:
            errors.append(f"GLB has no meshes: {path}")
        
        if len(gltf.materials) == 0:
            warnings.append(f"GLB has no materials: {path}")
        
        # Check embedded textures
        has_embedded = len(gltf.images) > 0
        if spec.get("has_textures", False) and not has_embedded:
            warnings.append(f"GLB spec expects embedded textures but none found: {path}")
        elif not spec.get("has_textures", False) and has_embedded:
            warnings.append(f"GLB has embedded textures but spec says none expected: {path}")
        
        # Triangle count via trimesh
        scene = trimesh.load(str(path), force='scene')
        total_tris = sum(len(g.faces) for g in scene.geometry.values() if hasattr(g, 'faces'))
        max_tris = spec.get("max_tris", 20000)
        if total_tris > max_tris:
            errors.append(f"Triangle count {total_tris} exceeds limit {max_tris}: {path}")
        elif total_tris > max_tris * 0.8:
            warnings.append(f"Triangle count {total_tris} near limit {max_tris}: {path}")
        
        # Check for reasonable mesh count
        if len(gltf.meshes) > 10:
            warnings.append(f"High mesh count ({len(gltf.meshes)}): {path}")
        
    except Exception as e:
        errors.append(f"Failed to parse GLB {path}: {e}")
    
    return errors, warnings

def validate_obj(path: Path, spec: Dict) -> Tuple[List[str], List[str]]:
    """Validate OBJ model and associated MTL/textures."""
    errors = []
    warnings = []
    
    try:
        with open(path, 'r') as f:
            lines = f.readlines()
        
        v_count = sum(1 for l in lines if l.startswith('v '))
        f_count = sum(1 for l in lines if l.startswith('f '))
        
        if v_count == 0:
            errors.append(f"OBJ has no vertices: {path}")
        if f_count == 0:
            errors.append(f"OBJ has no faces: {path}")
        
        tris = f_count  # assuming triangulated
        max_tris = spec.get("max_tris", 25000)
        if tris > max_tris:
            errors.append(f"Triangle count {tris} exceeds limit {max_tris}: {path}")
        
        # Check MTL exists
        mtl_path = path.with_suffix('.mtl')
        if not mtl_path.exists():
            warnings.append(f"MTL file missing for OBJ: {path}")
        
    except Exception as e:
        errors.append(f"Failed to parse OBJ {path}: {e}")
    
    return errors, warnings

def validate_texture(path: Path) -> Tuple[List[str], List[str]]:
    errors = []
    warnings = []
    
    if not path.exists():
        errors.append(f"Texture missing: {path}")
        return errors, warnings
    
    if path.stat().st_size == 0:
        errors.append(f"Texture empty: {path}")
    
    # Check file extension
    if path.suffix.lower() not in ['.png', '.jpg', '.jpeg', '.tga', '.bmp']:
        warnings.append(f"Unusual texture format: {path}")
    
    return errors, warnings

def validate_manifest(path: Path) -> Tuple[List[str], List[str]]:
    errors = []
    warnings = []
    
    try:
        with open(path, 'r') as f:
            manifest = json.load(f)
        
        deps = manifest.get("dependencies", {})
        required = ["com.unity.modules.assetbundle", "com.unity.xr.oculus", "com.unity.xr.management"]
        for req in required:
            if req not in deps:
                warnings.append(f"Manifest missing recommended dependency: {req}")
        
        # Check for glTFast (required for GLB import)
        if "com.unity.gltfast" not in deps:
            warnings.append("Manifest missing glTFast (com.unity.gltfast) - required for GLB import")
        
    except json.JSONDecodeError as e:
        errors.append(f"Invalid JSON in manifest: {e}")
    except Exception as e:
        errors.append(f"Failed to read manifest: {e}")
    
    return errors, warnings

def validate_project_version(path: Path) -> Tuple[List[str], List[str]]:
    errors = []
    warnings = []
    
    try:
        content = path.read_text()
        if "2022.3.20f1" not in content:
            errors.append(f"ProjectVersion.txt does not pin 2022.3.20f1: {content.strip()}")
        if "61c2feb0970d" not in content:
            warnings.append(f"ProjectVersion.txt missing revision 61c2feb0970d")
    except Exception as e:
        errors.append(f"Failed to read ProjectVersion.txt: {e}")
    
    return errors, warnings

def validate_asmdef(path: Path) -> Tuple[List[str], List[str]]:
    errors = []
    warnings = []
    
    try:
        content = json.loads(path.read_text())
        
        # Check for required fields
        if "name" not in content:
            errors.append(f"asmdef missing 'name': {path}")
        if "includePlatforms" not in content:
            warnings.append(f"asmdef missing 'includePlatforms': {path}")
        
        # Check for B&S references in cosmetic asmdef
        if "Cosmetic.Editor" in content.get("name", ""):
            refs = content.get("references", [])
            if refs:
                warnings.append(f"Cosmetic asmdef has references (should be empty): {refs}")
    
    except json.JSONDecodeError as e:
        errors.append(f"Invalid JSON in asmdef {path}: {e}")
    except Exception as e:
        errors.append(f"Failed to read asmdef {path}: {e}")
    
    return errors, warnings

def validate_csproj_syntax(path: Path) -> Tuple[List[str], List[str]]:
    """Basic C# syntax check - just verify file is readable and has basic structure."""
    errors = []
    warnings = []
    
    try:
        content = path.read_text()
        if not content.strip():
            warnings.append(f"Empty C# file: {path}")
        # Check for basic C# structure
        if "using " not in content and "namespace " not in content and "class " not in content:
            warnings.append(f"C# file may be empty or malformed: {path}")
    except Exception as e:
        errors.append(f"Failed to read C# file {path}: {e}")
    
    return errors, warnings

def main():
    result = ValidationResult()
    
    print("=" * 60)
    print("JohnWickArsenal Asset Validation (Unity-free)")
    print("=" * 60)
    
    # 1. Validate required model files
    print("\n[1/7] Validating model files...")
    for rel_path, spec in REQUIRED_MODELS.items():
        path = ASSETS_ROOT / rel_path
        if not path.exists():
            result.add_error(f"Missing required model: {rel_path}")
            continue
        
        if path.stat().st_size == 0:
            result.add_error(f"Empty model file: {rel_path}")
            continue
        
        if spec["type"] == "glb":
            errors, warnings = validate_glb(ASSETS_ROOT / rel_path, spec)
        elif spec["type"] == "obj":
            errors, warnings = validate_obj(ASSETS_ROOT / rel_path, spec)
        elif spec["type"] in ["png", "mtl"]:
            errors, warnings = validate_texture(ASSETS_ROOT / rel_path)
        else:
            result.add_warning(f"Unknown model type {spec['type']} for {rel_path}")
            continue
        
        result.errors.extend(errors)
        result.warnings.extend(warnings)
        
        if not errors:
            result.add_info(f"OK: {rel_path}")
    
    # 2. Validate texture files referenced by OBJ
    print("\n[2/7] Validating texture files...")
    texture_files = [
        "Models/Suits/ClassicBlackSuit_Albedo.png",
        "Models/Suits/ClassicBlackSuit_Bump.png",
    ]
    for tex in texture_files:
        path = ASSETS_ROOT / tex
        errors, warnings = validate_texture(ASSETS_ROOT / tex)
        result.errors.extend(errors)
        result.warnings.extend(warnings)
    
    # 3. Validate prefab files (existence only)
    print("\n[3/7] Validating prefab files...")
    for rel_path in REQUIRED_PREFERBS:
        path = ASSETS_ROOT / rel_path
        if not path.exists():
            result.add_error(f"Missing prefab: {rel_path}")
        elif path.stat().st_size == 0:
            result.add_warning(f"Empty prefab: {rel_path}")
        else:
            result.add_info(f"OK: {rel_path}")
    
    # 4. Validate scripts
    print("\n[4/7] Validating C# scripts...")
    for rel_path in REQUIRED_SCRIPTS:
        path = UNITY_PROJECT / rel_path
        if not path.exists():
            result.add_error(f"Missing script: {rel_path}")
        else:
            errors, warnings = validate_csproj_syntax(UNITY_PROJECT / rel_path)
            result.errors.extend(errors)
            result.warnings.extend(warnings)
            if not errors:
                result.add_info(f"OK: {rel_path}")
    
    # 4b. Check all Editor asmdefs
    print("\n[4b/7] Validating Editor asmdefs...")
    asmdefs = list((UNITY_PROJECT / "Assets" / "JohnWickArsenal" / "Editor").glob("*.asmdef"))
    for asmdef in asmdefs:
        errors, warnings = validate_asmdef(asmdef)
        result.errors.extend(errors)
        result.warnings.extend(warnings)
    
    # 4c. Check Scripts asmdefs
    print("\n[4c/7] Validating Scripts asmdefs...")
    asmdefs = list((UNITY_PROJECT / "Assets" / "Scripts" / "JohnWickArsenal").glob("*.asmdef"))
    for asmdef in asmdefs:
        errors, warnings = validate_asmdef(asmdef)
        result.errors.extend(errors)
        result.warnings.extend(warnings)
    
    # 5. Validate configs
    print("\n[5/7] Validating config files...")
    for rel_path in REQUIRED_CONFIGS:
        path = UNITY_PROJECT / rel_path
        if not path.exists():
            result.add_error(f"Missing config: {rel_path}")
            continue
        
        if rel_path == "Packages/manifest.json":
            errors, warnings = validate_manifest(path)
        elif rel_path == "ProjectSettings/ProjectVersion.txt":
            errors, warnings = validate_project_version(path)
        else:
            errors, warnings = [], []
        
        result.errors.extend(errors)
        result.warnings.extend(warnings)
    
    # 6. Validate asmdefs
    print("\n[6/7] Validating asmdef files...")
    asmdef_files = list(UNITY_PROJECT.glob("**/*.asmdef"))
    for asmdef in asmdef_files:
        errors, warnings = validate_asmdef(asmdef)
        result.errors.extend(errors)
        result.warnings.extend(warnings)
    
    # 7. Check project structure
    print("\n[7/7] Validating project structure...")
    required_dirs = [
        "Assets/JohnWickArsenal/Models",
        "Assets/JohnWickArsenal/Prefabs",
        "Assets/JohnWickArsenal/Editor",
        
        "Assets/LocalTools/Editor",
        "Assets/Scripts/JohnWickArsenal",
    ]
    for dir_path in required_dirs:
        path = UNITY_PROJECT / dir_path
        if not path.exists():
            result.add_error(f"Missing directory: {dir_path}")
        else:
            result.add_info(f"OK: {dir_path}")
    
    # Summary
    print("\n" + "=" * 60)
    print("VALIDATION SUMMARY")
    print("=" * 60)
    print(f"Errors:   {len(result.errors)}")
    print(f"Warnings: {len(result.warnings)}")
    print(f"Info:     {len(result.info)}")
    
    if result.errors:
        print("\n❌ ERRORS:")
        for err in result.errors:
            print(f"  - {err}")
    
    if result.warnings:
        print("\n⚠️  WARNINGS:")
        for warn in result.warnings:
            print(f"  - {warn}")
    
    if result.info:
        print("\n✅ PASSED:")
        for info in result.info[-10:]:  # Show last 10
            print(f"  - {info}")
        if len(result.info) > 10:
            print(f"  ... and {len(result.info) - 10} more")
    
    print("\n" + "=" * 60)
    
    if result.has_errors():
        print("❌ VALIDATION FAILED")
        return 1
    else:
        print("✅ VALIDATION PASSED")
        return 0

if __name__ == "__main__":
    sys.exit(main())
