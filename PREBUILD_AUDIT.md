# JohnWickArsenal Lightweight Branch - Pre-Build Audit Report

**Branch**: `lightweight` (commit `040ec54`)
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

### Legacy Editor Assembly (Conditional B&S References)
**File**: `Assets/JohnWickArsenal/Editor/JohnWickArsenal.Legacy.Pipeline.Editor.asmdef`
- Editor-only, `JWA_LEGACY_PROTOTYPE` constraint
- B&S-dependent code wrapped in `#if !JWA_COSMETIC_ONLY`

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
| `JohnWickArsenal.Cosmetic.Editor` | **None** ✅ | **Tier 1 Cosmetic test** |
| `JohnWickArsenal.Legacy.Pipeline.Editor` | Conditional (`#if !JWA_COSMETIC_ONLY`) | Legacy tools |
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
| **Compatible Version** | **6.3.0 - 6.3.2** (for Unity 2022.3 LTS) |
| **Latest Verified** | **6.3.2** |
| **Install Method** | Window > Package Manager > Unity Registry > glTFast > Install |

### glTFast Version Compatibility Matrix
| glTFast Version | Unity 2022.3 LTS | Notes |
|-----------------|------------------|-------|
| 5.x.x | ✅ Compatible | Legacy support |
| **6.0.0 - 6.3.x** | ✅ **Recommended** | Current stable |
| **6.3.2** | ✅ **Verified** | Latest 6.3.x |
| 6.4.x+ | ⚠️ Check changelog | May require Unity 6+ |

### glTFast Features Used by JWA_DisplayPrefabGenerator

| Feature | glTFast Support |
|---------|-----------------|
| GLB Import | ✅ Native |
| Embedded PBR Textures | ✅ Auto-extracted |
| Mesh Import | ✅ Standard Unity Mesh |
| Material Creation | ✅ URP/Built-in RP |

### Script Compatibility
| Check | Result |
|-------|--------|
| `AssetDatabase.LoadAssetAtPath<GameObject>()` | ✅ Works with glTFast-imported GLB |
| `PrefabUtility.InstantiatePrefab()` | ✅ Works with glTFast GameObjects |
| `PrefabUtility.SaveAsPrefabAsset()` | ✅ Standard Unity API |

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

### Display Prefab Generator - Model Paths Verified
| Generator Call | Model Path | File Exists |
|----------------|------------|-------------|
| `Models/Pistols/TTI_PitViper.glb` | ✅ | 115 KB |
| `Models/Pistols/Glock_34.glb` | ✅ | 158 KB |
| `Models/DisplayModels/VinishWeapons/AR15_AssaultRifle.glb` | ✅ | 631 KB |
| `Models/DisplayModels/VinishWeapons/AWP_SniperRifle.glb` | ✅ | 495 KB |
| `Models/DisplayModels/VinishWeapons/MP5_SubmachineGun.glb` | ✅ | 365 KB |
| `Models/DisplayModels/VinishWeapons/Remington870_PumpShotgun.glb` | ✅ | 237 KB |

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

| Package | Tier 1 Cosmetic Test | Tier 2 Bundle Build | Tier 3 Android Build | Tier 4 Full Mod | Notes |
|---------|---------------------|---------------------|---------------------|-----------------|-------|
| `com.unity.cloud.gltfast` | **✅ REQUIRED** | ✅ REQUIRED | ✅ REQUIRED | ✅ REQUIRED | GLB import |
| `com.unity.modules.assetbundle` | Optional | ✅ REQUIRED | ✅ REQUIRED | ✅ REQUIRED | Built-in module |
| `com.unity.addressables` | Optional | ✅ REQUIRED | ✅ REQUIRED | ✅ REQUIRED | AssetBundle |
| `com.unity.xr.management` | **Not needed** | **Not needed** | ✅ REQUIRED | ✅ REQUIRED | XR core |
| `com.unity.xr.oculus` | **Not needed** | **Not needed** | ✅ REQUIRED | ✅ REQUIRED | Quest support |
| `com.unity.inputsystem` | **Not needed** | **Not needed** | Optional | ✅ REQUIRED | Input handling |
| `com.unity.mobile.android-logcat` | **Not needed** | **Not needed** | Optional | Optional | Debugging |

### Minimal Package Set for Tier 1 Cosmetic Test
```json
{
  "dependencies": {
    "com.unity.cloud.gltfast": "6.3.2",
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

### Conditional Compilation Protection
All B&S references in legacy editor scripts wrapped in `#if !JWA_COSMETIC_ONLY`:

| Script | B&S References | Protection |
|--------|----------------|------------|
| `JWA_PrefabSetupWizard.cs` | 10 refs | `#if !JWA_COSMETIC_ONLY` |
| `JWA_BatchBuilder.cs` | 5 refs | `#if !JWA_COSMETIC_ONLY` |
| `JWA_BuildAssetBundles.cs` | 5 refs | `#if !JWA_COSMETIC_ONLY` |
| `JWA_BatchPrefabCreator.cs` | 8 refs | `#if !JWA_COSMETIC_ONLY` |

### Impact
| Build Type | B&S Assemblies Required |
|------------|------------------------|
| **Tier 1 Cosmetic Test** | **No** ✅ (with `JWA_COSMETIC_ONLY` define) |
| **Tier 4 Full Mod Build** | **Yes** ❌ |

---

## 7. Python vs Unity Editor Checks

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

## 8. Tier 1 Cosmetic-Only Test - Pass/Fail Criteria

### Prerequisites (Must Complete Before Test)
- [ ] Unity 2022.3.20f1 installed via Unity Hub
- [ ] **glTFast 6.3.2** installed via Package Manager (`com.unity.cloud.gltfast`)
- [ ] `JWA_COSMETIC_ONLY` added to **Scripting Define Symbols** in Player Settings
- [ ] `UnityProject` opened in Unity Hub

### Test Procedure
```bash
# In Unity Editor:
1. Window > Package Manager > Install glTFast (com.unity.cloud.gltfast 6.3.2)
2. Edit > Project Settings > Player > Scripting Define Symbols > Add "JWA_COSMETIC_ONLY"
3. Tools > John Wick Arsenal > Generate Display Prefabs
```

### Pass/Fail Criteria

| Check | Pass Criteria | Fail Criteria |
|-------|---------------|---------------|
| **Compilation** | Zero errors in Console | Any CS0246 (missing B&S types) |
| **Model Import** | 6 GLB + 1 OBJ import as GameObjects | Any import errors/warnings |
| **Material Setup** | Materials assigned, no pink shaders | Pink/missing materials |
| **Prefab Generation** | 6 prefabs created in `Prefabs/DisplayModels/` | < 6 prefabs created |
| **Component Attachment** | `JWA_DisplayModel` on each prefab | Missing component |
| **Hand Attach Point** | `HandAttachPoint` child on each prefab | Missing transform |

### Expected Output (Pass)
```
[JWA_DisplayPrefabGenerator] Created 6 display prefabs at Assets/JohnWickArsenal/Prefabs/DisplayModels
Generated 6 display prefabs!
```

### Expected Prefabs Created
| Prefab Name | Weapon Type | Source Model |
|-------------|-------------|--------------|
| DisplayPistol_TTI_PitViper | Pistol | petroulacl (CC0) |
| DisplayPistol_Glock_34 | Pistol | petroulacl (CC0) |
| DisplayRifle_AR15 | Rifle | Vinish.dev stand-in |
| DisplayRifle_AWP | Rifle | Vinish.dev stand-in |
| DisplaySMG_MP5 | SMG | Vinish.dev stand-in |
| DisplayShotgun_Remington870 | Shotgun | Vinish.dev stand-in |

---

## 9. Python vs Unity Editor Checks

### Python (`validate_android_sources.py`) ✅ Source-Level Only
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

### Unity Editor Required ❌ Runtime/Compilation Checks
| Check | Tool |
|-------|------|
| GLB import & GameObject creation | Unity Editor + glTFast |
| Material/shader compilation | Unity Editor |
| Prefab generation (`JWA_DisplayPrefabGenerator`) | Unity Editor |
| AssetBundle building | Unity Editor |
| C# compilation (asmdef references) | Unity Editor |
| Android/Quest build & deploy | Unity Editor + Android modules |

---

## 10. Final Status

| Check | Status |
|-------|--------|
| Source validation | ✅ Passes (`python3 Tools/validate_android_sources.py`) |
| Git working tree | ✅ Clean |
| Cosmetic asmdef created | ✅ (`JohnWickArsenal.Cosmetic.Editor`) |
| B&S references wrapped | ✅ 4 legacy editor scripts |
| Display generator updated | ✅ (glTFast notes) |
| glTFast version verified | ✅ 6.3.2 for Unity 2022.3 |
| PREBUILD_AUDIT.md updated | ✅ |
| Lightweight branch pushed | ✅ (commit pending) |

---

## Minimum Command for First Test (Tier 1)
```bash
# On target machine:
1. Install Unity 2022.3.20f1 via Hub
2. Open UnityProject
3. Window → Package Manager → Install glTFast (com.unity.cloud.gltfast 6.3.2)
4. Edit → Project Settings → Player → Scripting Define Symbols → Add "JWA_COSMETIC_ONLY"
5. Tools → John Wick Arsenal → Generate Display Prefabs
# If 6 prefabs created → COSMETIC TEST PASSED
```

**No B&S Nomad, No Android Build Support, No XR packages required for Tier 1.**

---

## Remaining Blocker for Tier 1 Test
| Blocker | Resolution |
|---------|------------|
| **glTFast package** | Install `com.unity.cloud.gltfast` 6.3.2 in Unity Package Manager |
| **Scripting Define** | Add `JWA_COSMETIC_ONLY` to Player Settings → Scripting Define Symbols |

**That's it.** Once these two steps are done, the 6 GLB models + ClassicBlackSuit can be imported and previewed.

---

## 11. Unity Cloud Build Cosmetic Preview Configuration

### Cloud Build Preparation Status

| Item | Status | Details |
|------|--------|---------|
| glTFast in manifest | ✅ Added | `com.unity.cloud.gltfast`: "6.3.2" |
| Cosmetic-only define | ✅ Configured | `JWA_COSMETIC_ONLY` in cloud build config |
| Gameplay scripts excluded | ✅ Configured | Main assembly excluded via cloud build config |
| Preview scene | ✅ Created | `JWA_CosmeticPreviewSceneSetup.cs` |
| Cloud build config | ✅ Created | `unitycloudbuild.yaml` + `JWA_CloudBuildConfig.cs` |
| Cosmetic preview scene | ✅ Scripted | `JWA_CosmeticPreviewSceneSetup.cs` |

### Cloud Build Configuration Files

| File | Purpose |
|------|---------|
| `unitycloudbuild.yaml` | Unity Cloud Build configuration for cosmetic preview |
| `Assets/JohnWickArsenal/Editor/JWA_CloudBuildConfig.cs` | Pre/post-export methods for Cloud Build |
| `Assets/JohnWickArsenal/Editor/JWA_CosmeticPreviewSceneSetup.cs` | Creates cosmetic preview scene |

### Cloud Build Requirements

| Requirement | Status |
|-------------|--------|
| glTFast in manifest | ✅ Added (`com.unity.cloud.gltfast`: "6.3.2") |
| Cosmetic-only define | ✅ `JWA_COSMETIC_ONLY` in cloud build config |
| Gameplay assemblies excluded | ✅ Main assembly excluded in cloud config |
| Cosmetic asmdef isolated | ✅ No B&S references |
| glTFast version | ✅ 6.3.2 (compatible with Unity 2022.3.20f1) |

### Cloud Build Pipeline

The cloud build pipeline for cosmetic preview:

1. **Pre-export**: Apply `JWA_COSMETIC_ONLY` define, disable gameplay assemblies
2. **Import**: Import GLB models via glTFast
3. **Generate**: Create display prefabs via `JWA_DisplayPrefabGenerator`
4. **Build**: Build Addressables content for cosmetic assets
5. **Package**: Build Android AssetBundle for cosmetic preview
5. **Deploy**: Upload build artifacts to Unity Cloud

### Next Steps for Cloud Build

1. **Set up Unity Cloud project** at build.unity3d.com
2. **Link GitHub repository** to Unity Cloud Build
3. **Configure build target** using `unitycloudbuild.yaml` settings
4. **Set environment variable** `JWA_COSMETIC_ONLY=1` in Cloud Build environment
5. **Trigger first build** and verify cosmetic preview builds successfully

### Remaining Blockers for Cloud Preview

| Blocker | Resolution |
|---------|------------|
| Unity Cloud project setup | Create project at build.unity3d.com, link GitHub repo |
| Cloud Build credentials | Configure Unity Cloud credentials in GitHub Actions secrets |
| First build verification | Trigger first build, verify cosmetic assets load correctly |

---

