# Chromebook-Compatible Development Workflow

## Overview
This document explains what can be verified from a Chromebook (Linux environment) without installing Unity Editor, and what requires the Unity Editor.

---

## ✅ What CAN Be Verified on Chromebook

### 1. Source Code Validation (Python)
| Check | Tool | Status |
|-------|------|--------|
| C# syntax (basic) | Python | ✅ |
| asmdef JSON validity | Python | ✅ |
| manifest.json syntax | Python | ✅ |
| ProjectVersion.txt format | Python | ✅ |
| mod.json / ModConfig/*.json | Python | ✅ |

### 2. Asset File Validation (Python + pygltflib + trimesh)
| Check | Tool | Status |
|-------|------|--------|
| GLB structure (meshes, materials, textures) | pygltflib | ✅ |
| Triangle counts | trimesh | ✅ |
| Embedded texture detection | pygltflib | ✅ |
| OBJ vertex/face counts | Python | ✅ |
| Texture file existence | Python | ✅ |
| File size / empty checks | Python | ✅ |

### 3. Project Structure (Bash / Python)
| Check | Tool | Status |
|-------|------|--------|
| Required directories exist | Bash/Python | ✅ |
| Required files exist | Bash/Python | ✅ |
| File sizes > 0 | Bash/Python | ✅ |
| JSON/YAML syntax | Python | ✅ |

### 4. Automated CI (GitHub Actions)
| Workflow | Status |
|----------|--------|
| Asset validation (Python) | ✅ Configured |
| Project structure check (Bash) | ✅ Configured |
| Manifest/version validation | ✅ Configured |
| GLB model validation (pygltflib) | ✅ Configured |

---

## ❌ What REQUIRES Unity Editor

| Task | Reason |
|------|--------|
| **GLB Import** | Requires glTFast package + Unity's ModelImporter |
| **Material/Shader Compilation** | Requires Unity's Shader/Material system |
| **Prefab Generation** | Requires `PrefabUtility`, `AssetDatabase` APIs |
| **AssetBundle Building** | Requires `BuildPipeline`, Addressables |
| **C# Compilation (asmdef refs)** | Requires Unity's Roslyn compiler + B&S assemblies |
| **Android/Quest Build** | Requires Android Build Support, SDK, NDK, OpenJDK |
| **Quest Deployment/Testing** | Requires Android device + ADB |

---

## 📋 What Can Be Verified Now (Chromebook)

### ✅ Currently Verified (No Unity)
| Asset | Verified |
|-------|----------|
| All 6 GLB models exist & valid | ✅ |
| Triangle counts within Quest limits (<20K) | ✅ |
| Embedded textures detected (Vinish models) | ✅ |
| ClassicBlackSuit OBJ + MTL + textures | ✅ |
| ClassicBlackSuit: 9,601 triangles | ✅ |
| ProjectVersion.txt: 2022.3.20f1 (61c2feb0970d) | ✅ |
| manifest.json valid JSON | ✅ |
| asmdef JSON syntax valid | ✅ |
| glTFast missing from manifest (warning) | ✅ |
| asmdef JSON syntax valid | ✅ |
| Required directories exist | ✅ |
| Required files exist | ✅ |

### ⚠️ Known Issues (Chromebook-detectable)
| Issue | Severity |
|-------|----------|
| Manifest missing `com.unity.cloud.gltfast` | Warning |
| petroulacl models lack embedded textures | Info (expected) |
| `.mtl` file flagged as unusual texture format | Warning (false positive) |

---

## 🚫 What Still Requires Unity Editor

| Task | Blocker |
|------|---------|
| Import GLB models into Unity | Requires glTFast + Unity ModelImporter |
| Verify materials/shaders render correctly | Requires Unity's Material system |
| Generate display prefabs | Requires `PrefabUtility`, `AssetDatabase` |
| Build cosmetic AssetBundle | Requires `BuildPipeline` |
| Compile C# scripts | Requires Unity's compiler + B&S assemblies |
| Build Android bundle | Requires Android Build Support |
| Test on Quest device | Requires Android device + ADB |

---

## 🤖 GitHub Actions CI (Configured)

The following workflows run automatically on push/PR:

### 1. Asset Validation (Python)
- Validates GLB/OBJ structure with pygltflib/trimesh
- Checks triangle counts, embedded textures
- Validates OBJ/MTL/PNG files
- Runs in ~30 seconds

### 2. Project Structure Check (Bash)
- Verifies all required directories/files exist
- Fast, no dependencies
- Runs in ~5 seconds

### 3. Manifest & Version Check (Bash/Python)
- Validates manifest.json syntax & dependencies
- Checks ProjectVersion.txt pins 2022.3.20f1 (61c2feb0970d)
- Validates asmdef JSON syntax

### 4. GLB Model Validation (Python + pygltflib)
- Deep GLB inspection: triangles, meshes, materials, embedded textures
- Validates Quest triangle budget (<20K)
- Checks embedded texture presence

---

## 🔐 Unity Personal Licensing for Cloud Builds

### Official Position (Unity Support)
> **Unity Personal cannot be activated in headless CI/CD environments.**
> - Requires Unity Hub GUI for activation
> - License is machine-bound (machine ID)
> - No headless activation support for Personal tier

### Legitimate Options for Cloud Builds

| Option | Description | Cost |
|--------|-------------|------|
| **Unity Pro/Enterprise** | Supports headless CI activation via license file | $2,040+/yr per seat |
| **Unity Build Server** | Dedicated CI/CD service (discontinued) | N/A |
| **Self-hosted Runner (Mac/Windows)** | Run Unity on self-hosted GitHub runner | Hardware cost |
| **Local Build + Artifact Upload** | Build locally, upload artifacts to GitHub | Free (your time) |

### Recommended Approach for This Project
1. **Local Build**: Build on a capable Windows/Linux/macOS machine with Unity Hub
2. **GitHub Actions**: Only for source validation (what we configured)
3. **Artifact Storage**: Upload built bundles as GitHub release assets

---

## 🎯 Minimum Unity Test Setup (When Available)

```bash
# On capable machine (Windows/Linux/macOS):
1. Install Unity 2022.3.20f1 via Unity Hub
2. Add Android Build Support + SDK/NDK/OpenJDK modules
3. Open UnityProject in Hub
4. Window → Package Manager → Install glTFast (com.unity.cloud.gltfast 6.3.2)
5. Edit → Project Settings → Player → Scripting Define Symbols → Add "JWA_COSMETIC_ONLY"
6. Tools → John Wick Arsenal → Generate Display Prefabs
# If 6 prefabs created → TIER 1 TEST PASSED
```

---

## 📊 Current Validation Status

| Category | Status | Details |
|----------|--------|---------|
| **Python Validation** | ✅ Passing | 0 errors, 3 warnings |
| **Project Structure** | ✅ Complete | All dirs/files present |
| **GLB Models** | ✅ Valid | 6/6 models, tris OK |
| **OBJ/Textures** | ✅ Complete | ClassicBlackSuit + textures |
| **Prefab** | ✅ Exists | DisplayPistol_TTI_PitViper |
| **Scripts** | ✅ Syntax OK | 2 key scripts valid |
| **asmdefs** | ✅ Valid JSON | 4 files valid |
| **manifest.json** | ✅ Valid JSON | Missing glTFast (warning) |
| **ProjectVersion** | ✅ Correct | 2022.3.20f1 (61c2feb0970d) |

---

## 📋 Next Steps

### From Chromebook (Now)
- [x] Run `python3 Tools/validate_assets.py` ✅
- [x] Run GitHub Actions workflows ✅
- [x] Commit validation improvements ✅

### From Capable Machine (When Available)
1. Install Unity 2022.3.20f1 + Android modules
2. Install glTFast 6.3.2
3. Add `JWA_COSMETIC_ONLY` scripting define
4. Run `Tools → John Wick Arsenal → Generate Display Prefabs`
5. Verify 6 display prefabs created
6. Build cosmetic AssetBundle
7. Test on Quest

---

## 🔗 GitHub Actions Workflows

| File | Purpose |
|------|---------|
| `.github/workflows/asset-validation.yml` | Complete CI pipeline |

All workflows run on `ubuntu-latest` with Python 3.12, no Unity required.

---

## 📝 Summary

| Question | Answer |
|----------|--------|
| **Can I validate assets on Chromebook?** | ✅ Yes - GLB/OBJ structure, triangle counts, textures, project structure |
| **Can I run GitHub Actions CI?** | ✅ Yes - 4 workflows configured, no Unity needed |
| **Can I import GLB models?** | ❌ No - requires glTFast + Unity Editor |
| **Can I build AssetBundles?** | ❌ No - requires Unity Editor + Android Build Support |
| **Can I test on Quest?** | ❌ No - requires Android build + Quest device |
| **Can I use Unity Personal in CI?** | ❌ No - Unity Personal requires GUI activation |
| **What's the minimum Unity test?** | Tier 1: Unity + glTFast + JWA_COSMETIC_ONLY define |

---

*Last Updated: 2026-11-23*
*Branch: lightweight*
*Unity Target: 2022.3.20f1 (61c2feb0970d)*
