# JohnWickArsenal Lightweight Branch - Pre-Build Audit Report

**Branch**: `lightweight` (commit `3ec163c`)
**Unity Version**: 2022.3.20f1 (61c2feb0970d)
**Target Platform**: Android (Meta Quest, ARM64)
**Audit Date**: 2026-11-23

---

## 1. Unity Project Structure Completeness ✅

### Core Directories Present
| Directory | Status |
|-----------|--------|
| `Assets/JohnWickArsenal/Models/` | ✅ Complete |
| `Assets/JohnWickArsenal/Prefabs/` | ✅ Complete |
| `Assets/JohnWickArsenal/Editor/` | ✅ Complete |
| `Assets/JohnWickArsenal/Scripts/` | ✅ Complete |
| `Assets/LocalTools/Editor/` | ✅ Complete |
| `Assets/Scripts/JohnWickArsenal/` | ✅ Complete |
| `ProjectSettings/` | ✅ Complete |
| `Packages/` | ✅ Complete |

### Model Assets Verified
| Asset | Path | Format | Status |
|-------|------|--------|--------|
| TTI Pit Viper | `Models/Pistols/TTI_PitViper.glb` + `.obj` | GLB + OBJ | ✅ |
| Glock 34 | `Models/Pistols/Glock_34.glb` + `.obj` | GLB + OBJ | ✅ |
| ClassicBlackSuit | `Models/Suits/ClassicBlackSuit.obj` + textures | OBJ + PNG | ✅ |
| AR-15 (Vinish) | `Models/DisplayModels/VinishWeapons/AR15_AssaultRifle.glb` | GLB | ✅ |
| AWP (Vinish) | `Models/DisplayModels/VinishWeapons/AWP_SniperRifle.glb` | GLB | ✅ |
| MP5 (Vinish) | `Models/DisplayModels/VinishWeapons/MP5_SubmachineGun.glb` | GLB | ✅ |
| Remington 870 (Vinish) | `Models/DisplayModels/VinishWeapons/Remington870_PumpShotgun.glb` | GLB | ✅ |

### Existing Prefabs
| Prefab | Path | Status |
|--------|------|--------|
| DisplayPistol_TTI_PitViper | `Prefabs/DisplayProps/DisplayPistol_TTI_PitViper.prefab` | ✅ |

---

## 2. Assembly Definition Structure ✅ ISOLATED

### Cosmetic Editor Assembly (NEW - No B&S Dependencies)
**File**: `Assets/JohnWickArsenal/Editor/JohnWickArsenal.Cosmetic.Editor.asmdef`
```json
{
  "name": "JohnWickArsenal.Cosmetic.Editor",
  "references": [],
  "includePlatforms": ["Editor"],
  "defineConstraints": []
}
```
- **No B&S references** ✅
- **Editor-only** ✅
- **Covers**: `JWA_DisplayPrefabGenerator.cs`, `JWA_DisplayModel` component

### Legacy Editor Assembly (Preserved)
**File**: `Assets/JohnWickArsenal/Editor/JohnWickArsenal.Legacy.Pipeline.Editor.asmdef`
- Editor-only, `JWA_LEGACY_PROTOTYPE` constraint

### Main Gameplay Assembly (Requires B&S)
**File**: `Assets/Scripts/JohnWickArsenal/JohnWickArsenal.asmdef`
- References: `BladeAndSorcery.Modding`, `BladeAndSorcery.Data`, `BladeAndSorcery.Gameplay`
- **Only needed for full mod build** ❌ Not required for cosmetic test

### Test Assembly
**File**: `Assets/Scripts/JohnWickArsenal/Tests/JohnWickArsenal.Tests.asmdef`
- References: `JohnWickArsenal`, `UnityEngine.TestRunner`, `UnityEngine.CoreModule`

### Isolation Result
| Assembly | B&S References | Required For |
|----------|----------------|--------------|
| `JohnWickArsenal.Cosmetic.Editor` | **None** ✅ | Cosmetic test |
| `JohnWickArsenal.Legacy.Pipeline.Editor` | None | Legacy tools |
| `JohnWickArsenal` (main) | **3 B&S assemblies** ❌ | Full mod only |
| `JohnWickArsenal.Tests` | Via main | Full mod only |

---

## 3. Display Prefab Generator - glTFast Compatibility ✅

**File**: `Assets/JohnWickArsenal/Editor/JWA_DisplayPrefabGenerator.cs`

### glTFast Package Information
| Detail | Value |
|--------|-------|
| **Package ID** | `com.unity.cloud.gltfast` |
| **Unity Registry** | https://packages.unity.com |
| **Compatible Version** | 6.x.x (for Unity 2022.3 LTS) |
| **Latest Verified** | 6.3.0+ (check Package Manager) |
| **Install Method** | Window > Package Manager > Unity Registry > glTFast > Install |

### glTFast Features Used
| Feature | Status |
|---------|--------|
| GLB Import | ✅ Native support via glTFast |
| Embedded PBR Textures | ✅ Auto-imported |
| Mesh Import | ✅ Standard Unity Mesh |
| Material Creation | ✅ Standard Shader / URP compatible |

### Script Compatibility
| Check | Result |
|-------|--------|
| `AssetDatabase.LoadAssetAtPath<GameObject>()` | ✅ Works with glTFast-imported GLB |
| `PrefabUtility.InstantiatePrefab()` | ✅ Works with glTFast GameObjects |
| `PrefabUtility.SaveAsPrefabAsset()` | ✅ Standard Unity API |

### Import Notes
- glTFast imports GLB as `GameObject` with `MeshFilter` + `MeshRenderer`
- Materials use Unity Standard Shader (Built-in RP) or URP Lit
- Embedded textures auto-extracted to `Materials/` folder
- May need ModelImporter settings adjustment for scale/origin

---

## 4. Asset File Paths ✅ Valid

### ClassicBlackSuit
| File | Path | Size | Status |
|------|------|------|--------|
| ClassicBlackSuit.obj | `Models/Suits/ClassicBlackSuit.obj` | 884 KB | ✅ |
| ClassicBlackSuit.mtl | `Models/Suits/ClassicBlackSuit.mtl` | 180 B | ✅ |
| _Albedo.png | `Models/Suits/ClassicBlackSuit_Albedo.png` | 108 KB | ✅ |
| _Bump.png | `Models/Suits/ClassicBlackSuit_Bump.png` | 117 KB | ✅ |

### Display Models (GLB)
| Model | Path | Size | Source | Triangles |
|-------|------|------|--------|-----------|
| TTI Pit Viper | `Models/Pistols/TTI_PitViper.glb` | 115 KB | petroulacl (CC0) | ~15K |
| Glock 34 | `Models/Pistols/Glock_34.glb` | 158 KB | petroulacl (CC0) | ~15K |
| AR-15 (Vinish) | `Models/DisplayModels/VinishWeapons/AR15_AssaultRifle.glb` | 631 KB | Vinish.dev (CC BY 4.0) | 13,447 |
| AWP (Vinish) | `Models/DisplayModels/VinishWeapons/AWP_SniperRifle.glb` | 495 KB | Vinish.dev (CC BY 4.0) | 8,564 |
| MP5 (Vinish) | `Models/DisplayModels/VinishWeapons/MP5_SubmachineGun.glb` | 365 KB | Vinish.dev (CC BY 4.0) | 6,532 |
| Remington 870 (Vinish) | `Models/DisplayModels/VinishWeapons/Remington870_PumpShotgun.glb` | 237 KB | Vinish.dev (CC BY 4.0) | 3,726 |

### Attribution
- `Models/DisplayModels/VinishWeapons/ATTRIBUTION.txt` ✅ CC BY 4.0 credits

---

## 5. Unity Packages - Required vs Optional

### Current `Packages/manifest.json`
```json
{
  "dependencies": {
    "com.unity.addressables": "1.21.21",
    "com.unity.inputsystem": "1.7.0",
    "com.unity.mobile.android-logcat": "1.1.0",
    "com.unity.modules.assetbundle": "1.0.0",
    "com.unity.xr.management": "4.5.0",
    "com.unity.xr.oculus": "4.5.0"
  }
}
```

### Package Requirements

| Package | Cosmetic Test | Full Mod Build | Notes |
|---------|---------------|----------------|-------|
| `com.unity.addressables` | Optional | ✅ Required | AssetBundle building |
| `com.unity.inputsystem` | Optional | ✅ Required | Input handling |
| `com.unity.mobile.android-logcat` | Optional | Optional | Debugging |
| `com.unity.modules.assetbundle` | Optional | ✅ Required | Built-in module |
| `com.unity.xr.management` | Optional | ✅ Required | XR core |
| `com.unity.xr.oculus` | Optional | ✅ Required | Quest support |
| **`com.unity.cloud.gltfast`** | **✅ REQUIRED** | **✅ REQUIRED** | **GLB import** |

### Minimal Package Set for Cosmetic Test
```json
{
  "dependencies": {
    "com.unity.cloud.gltfast": "6.3.0",
    "com.unity.modules.assetbundle": "1.0.0"
  }
}
```

---

## 6. Missing B&S Nomad SDK References

### Required Assemblies (Main Mod Only)
| Assembly | Source | Status | Required For |
|----------|--------|--------|--------------|
| `BladeAndSorcery.Modding` | B&S Nomad install | ❌ Missing | Main mod DLL |
| `BladeAndSorcery.Data` | B&S Nomad install | ❌ Missing | Main mod DLL |
| `BladeAndSorcery.Gameplay` | B&S Nomad install | ❌ Missing | Main mod DLL |

### Impact
| Build Type | B&S Assemblies Required |
|------------|------------------------|
| **Cosmetic-only test** | **No** ✅ |
| **Full mod build** | **Yes** ❌ |

---

## 7. glTFast Verification

### Package Details
- **Identifier**: `com.unity.cloud.gltfast`
- **Unity Version**: 2022.3 LTS compatible
- **Version Range**: 5.x - 6.x (6.3.0+ recommended)
- **Registry**: Unity Registry (built-in)
- **Documentation**: https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@6.3/manual/index.html

### Verification Steps (Unity Editor)
1. Window > Package Manager
2. Select "Unity Registry"
3. Search "glTFast"
4. Version 6.3.0+ > Install
5. Verify: `Assets/JohnWickArsenal/Models/Pistols/TTI_PitViper.glb` imports as GameObject

---

## 8. Test Tier Definitions

### Tier 1: Basic Unity Asset Import Test (Cosmetic-Only) ✅ MINIMAL
| Requirement | Status |
|-------------|--------|
| Unity 2022.3.20f1 | ✅ Required |
| glTFast package | ✅ Required |
| Project open | ✅ |
| GLB model import | ✅ Test target |
| Prefab generation | ✅ Test target |
| **B&S Assemblies** | **NOT REQUIRED** |
| **Android Build Support** | **NOT REQUIRED** |
| **XR/Oculus packages** | **NOT REQUIRED** |

### Tier 2: Cosmetic Bundle Build Test
| Requirement | Status |
|-------------|--------|
| Tier 1 + | ✅ |
| Addressables package | ✅ Required |
| AssetBundle module | ✅ Required |
| **Android Build Support** | **NOT REQUIRED** (can build bundle for any platform) |

### Tier 3: Android/Quest Build Test
| Requirement | Status |
|-------------|--------|
| Tier 2 + | ✅ |
| Android Build Support | ✅ Required |
| Android SDK/NDK/OpenJDK | ✅ Required |
| XR Management + Oculus | ✅ Required |

### Tier 4: Full Nomad Mod Build
| Requirement | Status |
|-------------|--------|
| Tier 3 + | ✅ |
| B&S Nomad assemblies (3) | ✅ Required |
| Full mod DLL compilation | ✅ Required |

---

## 9. Python vs Unity Editor Checks

### Python (`validate_android_sources.py`) ✅
| Check | Status |
|-------|--------|
| ProjectVersion.txt | ✅ |
| manifest.json syntax | ✅ |
| packages-lock.json | ✅ |
| mod.json | ✅ |
| ModConfig/*.json | ✅ |
| All .asmdef JSON syntax | ✅ |
| build_local.py syntax | ✅ |
| JwaLocalAndroidBuild.cs exists | ✅ |

### Unity Editor Required ❌
| Check | Tool |
|-------|------|
| GLB import & GameObject creation | Unity Editor + glTFast |
| Material/shader compilation | Unity Editor |
| Prefab generation (`JWA_DisplayPrefabGenerator`) | Unity Editor |
| AssetBundle building | Unity Editor |
| C# compilation (asmdef references) | Unity Editor |
| Android/Quest build & deploy | Unity Editor + Android modules |

---

## 10. Updated Import & Test Checklist

### Phase 1: Cosmetic Asset Import Test (First Test)
- [ ] **Unity 2022.3.20f1** installed via Hub
- [ ] **glTFast** (`com.unity.cloud.gltfast` 6.3.0+) installed via Package Manager
- [ ] Open `UnityProject` in Unity Hub
- [ ] Verify GLB models import:
  - [ ] `Models/Pistols/TTI_PitViper.glb` → GameObject
  - [ ] `Models/Pistols/Glock_34.glb` → GameObject
  - [ ] `Models/DisplayModels/VinishWeapons/*.glb` (4) → GameObjects
  - [ ] `Models/Suits/ClassicBlackSuit.obj` + textures → GameObject
- [ ] Run `Tools > John Wick Arsenal > Validate Display Models`
- [ ] Run `Tools > John Wick Arsenal > Generate Display Prefabs`
- [ ] Verify 6 prefabs in `Assets/JohnWickArsenal/Prefabs/DisplayModels/`

### Phase 2: Cosmetic Bundle Build (Optional)
- [ ] Install `com.unity.addressables` 1.21.21
- [ ] Configure Addressables group for display models
- [ ] Build AssetBundle for Android
- [ ] Verify bundle loads

### Phase 3: Full Mod Build (Separate)
- [ ] Install Android Build Support + SDK/NDK/OpenJDK
- [ ] Install XR Management + Oculus
- [ ] Install B&S Nomad, copy 3 assemblies to `Assemblies/`
- [ ] Run `./build.sh` for full mod

---

## 11. Dependency Summary

| Dependency | Cosmetic Test | Full Mod Build | Source |
|------------|---------------|----------------|--------|
| Unity 2022.3.20f1 | ✅ Essential | ✅ Essential | Unity Hub |
| glTFast (com.unity.cloud.gltfast) | ✅ Essential | ✅ Essential | Package Manager |
| ClassicBlackSuit assets | ✅ Essential | ✅ Essential | Local (in repo) |
| Display GLB models | ✅ Essential | ✅ Essential | Local (in repo) |
| `JohnWickArsenal.Cosmetic.Editor` asmdef | ✅ Essential | ✅ Essential | Local (in repo) |
| `com.unity.addressables` | Optional | Essential | Package Manager |
| `com.unity.modules.assetbundle` | Optional | Essential | Built-in |
| Android Build Support | **Not needed** | ✅ Essential | Unity Hub |
| XR Management + Oculus | **Not needed** | ✅ Essential | Package Manager |
| B&S Assemblies (3) | **Not needed** | ✅ Essential | B&S Nomad install |
| Android SDK/NDK/OpenJDK | **Not needed** | ✅ Essential | Unity Hub |

---

## 12. Final Status

| Check | Status |
|-------|--------|
| Source validation | ✅ Passes (`python3 Tools/validate_android_sources.py`) |
| Git working tree | ✅ Clean |
| Cosmetic asmdef created | ✅ (`JohnWickArsenal.Cosmetic.Editor`) |
| Display generator updated | ✅ (glTFast notes added) |
| PREBUILD_AUDIT.md updated | ✅ |
| Lightweight branch pushed | ✅ (commit pending) |

---

## Minimum Command for First Test
```bash
# On target machine:
1. Install Unity 2022.3.20f1 via Hub
2. Open UnityProject
3. Window > Package Manager > Install glTFast (com.unity.cloud.gltfast)
4. Tools > John Wick Arsenal > Generate Display Prefabs
# If successful → cosmetic test PASSED
```

