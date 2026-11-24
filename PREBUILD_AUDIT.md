# JohnWickArsenal Lightweight Branch - Pre-Build Audit Report

**Branch**: `lightweight` (commit `2c3e20e`)
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

## 2. Display Prefab Generator - Compilation Review ✅

**File**: `Assets/JohnWickArsenal/Editor/JWA_DisplayPrefabGenerator.cs`

### Code Analysis
| Check | Result | Notes |
|-------|--------|-------|
| Namespace | ✅ | `JohnWickArsenal.Editor` |
| Dependencies | ✅ | Uses `UnityEditor`, `UnityEngine`, `System.IO` |
| Menu Items | ✅ | `Tools/John Wick Arsenal/Generate Display Prefabs` and `Validate Display Models` |
| API Usage | ✅ | Uses `AssetDatabase`, `PrefabUtility`, `EditorUtility` correctly |
| Error Handling | ✅ | Checks for null assets, existing prefabs |
| Component | ✅ | `JWA_DisplayModel` MonoBehaviour with `handAttachPoint` |

### Potential Issues (Unity Editor Only)
| Issue | Severity | Resolution |
|-------|----------|------------|
| GLB Import | ⚠️ Medium | Requires **glTFast** package (`com.unity.cloud.gltfast`) for GLB import in Unity 2022.3 |
| Model Scale | ⚠️ Low | GLB models may import with different scales; may need `ModelImporter` settings |
| Material Import | ⚠️ Low | Embedded PBR textures in GLB should auto-import via glTFast |

---

## 3. ClassicBlackSuit & Display Assets - File Paths ✅

| Asset | Path | Dependencies | Status |
|-------|------|--------------|--------|
| ClassicBlackSuit.obj | `Models/Suits/ClassicBlackSuit.obj` | .mtl, _Albedo.png, _Bump.png | ✅ All present |
| ClassicBlackSuit.mtl | `Models/Suits/ClassicBlackSuit.mtl` | References textures | ✅ |
| _Albedo.png | `Models/Suits/ClassicBlackSuit_Albedo.png` | 108 KB | ✅ |
| _Bump.png | `Models/Suits/ClassicBlackSuit_Bump.png` | 117 KB | ✅ |
| TTI Pit Viper | `Models/Pistols/TTI_PitViper.glb` | Self-contained GLB | ✅ |
| Glock 34 | `Models/Pistols/Glock_34.glb` | Self-contained GLB | ✅ |
| Vinish Weapons (4) | `Models/DisplayModels/VinishWeapons/*.glb` | Self-contained GLB | ✅ |

---

## 4. Unity Packages - Review ⚠️

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

### Package Assessment
| Package | Status | Notes |
|---------|--------|-------|
| `com.unity.addressables` 1.21.21 | ✅ Required | For AssetBundle building |
| `com.unity.inputsystem` 1.7.0 | ✅ Required | For input handling |
| `com.unity.mobile.android-logcat` 1.1.0 | ✅ Optional | Debugging aid |
| `com.unity.modules.assetbundle` 1.0.0 | ✅ Required | Built-in module for AssetBundles |
| `com.unity.xr.management` 4.5.0 | ✅ Required | XR core |
| `com.unity.xr.oculus` 4.5.0 | ✅ Required | Meta Quest support |

### Missing Packages (Required for GLB Import)
| Package | Required For | Status |
|---------|--------------|--------|
| `com.unity.cloud.gltfast` | GLB/glTF import | ❌ **MISSING** - Required for `.glb` import |

### Unnecessary Packages (Not Present - Good)
- `com.unity.urp` - Not included (using Built-in RP) ✅
- `com.unity.2d.*` - Not included ✅
- `com.unity.ai.*` - Not included ✅

---

## 5. Missing B&S Nomad SDK References ❌

### Required Assembly References (from `JohnWickArsenal.asmdef`)
```json
"references": [
  "BladeAndSorcery.Modding",
  "BladeAndSorcery.Data",
  "BladeAndSorcery.Gameplay"
]
```

### Status
| Assembly | Source | Status |
|----------|--------|--------|
| `BladeAndSorcery.Modding` | B&S Nomad install | ❌ **MISSING** |
| `BladeAndSorcery.Data` | B&S Nomad install | ❌ **MISSING** |
| `BladeAndSorcery.Gameplay` | B&S Nomad install | ❌ **MISSING** |

### Impact
- **Main mod DLL cannot compile** without these assemblies
- Required for: `JohnWickPistolBehavior`, `JohnWickRifleBehavior`, `JWA_EquipmentSystem`, `JWA_NPCSystem`, etc.
- Only available by installing Blade & Sorcery Nomad and copying from `BladeAndSorcery_Data/Managed/`

### Workaround for Cosmetic-Only Build
The `JWA_LEGACY_PROTOTYPE` define constraint allows conditional compilation, but the main asmdef still references these assemblies.

---

## 6. Python vs Unity Editor Checks

### Performed by Python (`validate_android_sources.py`) ✅
| Check | Status |
|-------|--------|
| ProjectVersion.txt pins 2022.3.20f1 | ✅ |
| Package manifest valid JSON | ✅ |
| packages-lock.json valid JSON | ✅ |
| mod.json valid JSON | ✅ |
| ModConfig/*.json valid JSON | ✅ |
| All .asmdef files valid JSON | ✅ |
| build_local.py syntax valid | ✅ |
| JwaLocalAndroidBuild.cs exists | ✅ |

### Require Unity Editor ❌
| Check | Requires |
|-------|----------|
| GLB model import & validation | Unity Editor + glTFast |
| Model scale/origin verification | Unity Editor |
| Material/shader compilation | Unity Editor |
| Prefab generation (`JWA_DisplayPrefabGenerator`) | Unity Editor |
| AssetBundle building | Unity Editor + Android Build Support |
| C# compilation (asmdef references) | Unity Editor + B&S assemblies |
| Android build & APK generation | Unity Editor + Android Build Support |
| Quest deployment & testing | Android device + ADB |

---

## 7. Import & Test Checklist for Target Machine

### Prerequisites
- [ ] **OS**: Windows 10/11, Ubuntu 22.04+, or macOS 13+
- [ ] **RAM**: 16 GB+ (32 GB recommended)
- [ ] **Storage**: 50 GB+ free (SSD/NVMe)
- [ ] **Unity Hub**: Installed and signed in
- [ ] **Unity 2022.3.20f1**: Installed via Hub with modules:
  - [ ] Android Build Support
  - [ ] Android SDK & NDK Tools
  - [ ] OpenJDK
- [ ] **glTFast Package**: Install via Package Manager (`com.unity.cloud.gltfast`)
- [ ] **Blade & Sorcery Nomad**: Installed (for assembly refs)

### Assembly Setup
- [ ] Copy B&S assemblies to `Assemblies/` folder:
  - `BladeAndSorcery.Modding.dll`
  - `BladeAndSorcery.Data.dll`
  - `BladeAndSorcery.Gameplay.dll`
  - (From `BladeAndSorcery_Data/Managed/` in B&S install)

### Unity Project Setup
- [ ] Open `UnityProject` in Unity Hub
- [ ] Wait for Package Manager to resolve dependencies
- [ ] Install missing `com.unity.cloud.gltfast` via Package Manager
- [ ] Verify `File > Build Settings > Android` → Switch Platform
- [ ] Verify `Project Settings > XR Plug-in Management > Oculus` ✅ enabled

### Asset Import & Validation
- [ ] Verify GLB models import correctly (check scale, materials, textures)
- [ ] Run `Tools > John Wick Arsenal > Validate Display Models`
- [ ] Run `Tools > John Wick Arsenal > Generate Display Prefabs`
- [ ] Verify 6 display prefabs created in `Assets/JohnWickArsenal/Prefabs/DisplayModels/`

### Build & Test
- [ ] Run `python3 Tools/build_local.py --check`
- [ ] Run `python3 Tools/build_local.py --output BuildOutput`
- [ ] Verify `johnwickarsenal-cosmetics.bundle` created
- [ ] Deploy bundle to Quest for visual review
- [ ] For full mod: Run `./build.sh` (requires B&S assemblies)

---

## 8. Remaining Blockers Before First Unity Test

| Blocker | Category | Resolution Required |
|---------|----------|---------------------|
| **Missing glTFast package** | Package | Install `com.unity.cloud.gltfast` in Unity |
| **Missing B&S assemblies** | SDK | Install B&S Nomad, copy 3 DLLs to `Assemblies/` |
| **Missing Android Build Support** | Unity Module | Install via Unity Hub |
| **Missing OpenJDK** | Unity Module | Install via Unity Hub |
| **GLB import unverified** | Import | Must test in Unity with glTFast |
| **ClassicBlackSuit material setup** | Import | May need manual material assignment for OBJ |
| **Sketchfab models unverified** | Legal | Must verify 11 licenses before use |

---

## Summary

| Area | Status |
|------|--------|
| Project Structure | ✅ Complete |
| Display Prefab Generator | ✅ Ready (needs glTFast) |
| Asset File Paths | ✅ Valid |
| Unity Packages | ⚠️ Missing glTFast |
| B&S SDK References | ❌ Missing (3 assemblies) |
| Source Validation | ✅ Passes |
| Ready for Unity Test | ❌ **No - 4 blockers** |

### Minimum Viable Setup for First Test
1. Install Unity 2022.3.20f1 + Android modules
2. Install glTFast package
3. Copy B&S assemblies to `Assemblies/`
4. Open project, import GLB models
5. Generate display prefabs
6. Build cosmetic bundle

