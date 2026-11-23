# John Wick Arsenal - Project Transfer Guide

## Overview
This guide explains how to transfer the John Wick Arsenal project from the current development environment (Chromebook Linux) to a more capable computer for Unity Android builds.

**Current Status**: All source code, models, textures, scripts, and documentation are committed to GitHub on branch `develop/unity-cosmetics-final` (PR #2). No local-only changes exist.

---

## Prerequisites for Target Machine

### Hardware Requirements
| Component | Minimum | Recommended |
|-----------|---------|-------------|
| **RAM** | 16 GB | 32+ GB |
| **Storage** | 50 GB free (SSD) | 100+ GB free (NVMe SSD) |
| **CPU** | 4+ cores, x86_64 | 8+ cores, modern architecture |
| **GPU** | Any (headless build supported) | NVIDIA/AMD for faster shader compilation |
| **OS** | Windows 10/11, Ubuntu 20.04+, macOS 12+ | Windows 11 / Ubuntu 22.04 / macOS 14+ |

### Software Requirements
| Software | Version | Notes |
|----------|---------|-------|
| **Unity Hub** | Latest | https://unity.com/download |
| **Unity Editor** | **2022.3.20f1** (changeset 61c2feb0970d) | **Exact version required** |
| **Android Build Support** | SDK 33 (API 33), NDK 25.1.8937393, OpenJDK | Installed via Unity Hub modules |
| **Git** | 2.30+ | For cloning |
| **Python** | 3.8+ | For build scripts |
| **Blade & Sorcery Nomad** | 1.3.1 (Quest) | Required for assembly references (DLL build) |

---

## Unity Editor Installation (Exact Steps)

### 1. Install Unity Hub
```bash
# Windows: Download from unity.com/download
# Ubuntu: snap install unityhub --classic
# macOS: brew install --cask unity-hub
```

### 2. Sign In & Activate License
1. Open Unity Hub → Sign in with Unity ID
2. **License → Manage License → Activate Personal License**
3. Verify: Hub shows "Personal" for your account

### 3. Install Unity 2022.3.20f1
1. **Installs → Add → Official Releases → 2022.3.20f1**
2. **Wait for download/install** (~2-3 GB)

### 4. Add Android Build Support Modules
1. **Installs → 2022.3.20f1 → ⋮ (three dots) → Add Modules**
2. Check **all** of these:
   - ✅ **Android Build Support**
   - ✅ **Android SDK & NDK Tools** (installs SDK 33, NDK 25.1.8937393)
   - ✅ **OpenJDK**
3. **Apply** → Wait for download (~5-8 GB total)

### 5. Verify Installation
```bash
# Check Unity executable exists
# Windows: C:\Program Files\Unity\Hub\Editor\2022.3.20f1\Editor\Unity.exe
# Linux: ~/Unity/Hub/Editor/2022.3.20f1/Editor/Unity
# macOS: /Applications/Unity/Hub/Editor/2022.3.20f1/Unity.app/Contents/MacOS/Unity

# Check Android Player engine
# Should exist at:
# Windows: .../Editor/Data/PlaybackEngines/AndroidPlayer/
# Linux: .../Editor/Data/PlaybackEngines/AndroidPlayer/
# macOS: .../PlaybackEngines/AndroidPlayer/

# Verify subdirectories: SDK/, NDK/, OpenJDK/
```

---

## Blade & Sorcery Nomad Installation (For DLL Compilation)

### Required for Main Mod DLL
The main mod assembly (`JohnWickArsenal.dll`) references:
- `BladeAndSorcery.Modding`
- `BladeAndSorcery.Data`
- `BladeAndSorcery.Gameplay`

These assemblies are **only available** with B&S Nomad installed.

### Installation Options
1. **Meta Quest (via SideQuest/BSMG)**: Install B&S Nomad on Quest, then copy assemblies from device
2. **PCVR Version**: Install Blade & Sorcery on PC (Steam/Oculus), copy assemblies
3. **Assembly Location**: Typically at:
   - Quest: `/sdcard/Mods/...` (requires ADB pull)
   - PC: `BladeAndSorcery_Data/Managed/`

### Assembly Setup for Project
```bash
# Create Assemblies folder in project root
mkdir -p /path/to/JohnWickArsenal/Assemblies

# Copy these files from B&S installation:
# BladeAndSorcery.Modding.dll
# BladeAndSorcery.Data.dll
# BladeAndSorcery.Gameplay.dll
# UnityEngine.CoreModule.dll (from Unity Editor)
# UnityEngine.PhysicsModule.dll
# etc. (see build.sh for full list)
```

---

## Project Transfer Steps

### 1. Clone Repository
```bash
git clone https://github.com/jonnythebot/JohnWickArsenal.git
cd JohnWickArsenal
git checkout develop/unity-cosmetics-final
```

### 2. Verify Source Integrity
```bash
python3 Tools/validate_android_sources.py
# Expected: "Source checks passed for Unity 2022.3.20f1. Run the local build to compile and export."
```

### 3. Open in Unity Hub
1. Unity Hub → **Add** → Select `UnityProject` folder
2. Wait for Package Manager to resolve dependencies
3. Check **Console** for red errors (should be none)

### 4. Verify Project Settings
- **File → Build Settings** → **Android** → **Switch Platform**
- **Project Settings → Player** → Verify:
  - Company Name: `JohnWickArsenal`
  - Product Name: `JohnWickArsenal`
  - Scripting Backend: **IL2CPP**
  - Target Architectures: **ARM64** (ARMv7 optional)
  - Minimum API Level: **29** (Android 10)
  - Target API Level: **33** (Android 13)
- **Project Settings → XR Plug-in Management** → **Oculus** ✅ enabled

### 5. Verify Assets Imported
In Project Browser, check:
```
Assets/JohnWickArsenal/
├── Models/
│   ├── Suits/ (5 suits + ClassicBlackSuit with textures)
│   ├── Cosmetics/
│   │   ├── Garments/DoubleBreastedSuit/ (+ Albedo/Normal)
│   │   ├── Garments/DinnerJacket/ (+ Albedo/Normal)
│   │   └── Props/Briefcase/ (.fbx + textures)
│   ├── Pistols/ (4 weapons - OBJ + GLB)
│   ├── Shotguns/ (2 weapons - OBJ + GLB)
│   ├── Revolvers/ (1 weapon - OBJ + GLB)
│   ├── Rifles/ (1 weapon - OBJ + GLB)
│   └── Throwables/ (2 weapons - OBJ + GLB)
├── Prefabs/
│   ├── Accessories/ (empty - generate via Tools menu)
│   └── DisplayProps/ (6 display prop prefabs)
├── Editor/ (LocalTools, Build scripts)
└── Scripts/ (all C# systems)
```

### 6. Generate Accessory Prefabs (Optional)
In Unity Editor:
```
Tools → John Wick Arsenal → Generate All (Prefabs + Catalog)
```
This creates 36 accessory prefabs in `Assets/JohnWickArsenal/Prefabs/Accessories/`

### 7. Run Validation Build (Cosmetic Bundle Only)
```bash
python3 Tools/build_local.py --check
# Expected: "Found editor and Android modules at ... Confirm license activation in Unity Hub."

python3 Tools/build_local.py --output BuildOutput
# Creates: BuildOutput/cosmetics-android-<timestamp>/johnwickarsenal-cosmetics.bundle
```

---

## Build Pipeline Overview

### Cosmetic Review Bundle (No B&S Required)
```bash
python3 Tools/build_local.py --output BuildOutput
```
- Exports: Suits, garments, briefcase, display props
- Output: `johnwickarsenal-cosmetics.bundle` (Android AssetBundle)
- Use for: Visual review on Quest, material/UV verification

### Full Mod Build (Requires B&S Nomad)
```bash
./build.sh
# Or individual steps:
./build.sh deps      # Check dependencies
./build.sh setup     # Initialize Unity project
./build.sh dll       # Compile JohnWickArsenal.dll
./build.sh bundle    # Build weapon AssetBundle
./build.sh package   # Create distribution ZIP
./build.sh verify    # Validate outputs
```

---

## Key Files Reference

| File | Purpose |
|------|---------|
| `Tools/validate_android_sources.py` | Pre-build validation (no Unity needed) |
| `Tools/build_local.py` | Local Unity build orchestrator |
| `UnityProject/Assets/LocalTools/Editor/JwaLocalAndroidBuild.cs` | Unity batch export method |
| `UnityProject/Assets/LocalTools/Editor/JwaCosmeticTextureImport.cs` | Auto texture settings (1K, ASTC_6x6) |
| `UnityProject/Packages/manifest.json` | Package dependencies |
| `UnityProject/ProjectSettings/ProjectVersion.txt` | Unity version pin (2022.3.20f1) |
| `mod.json` | Mod metadata for B&S Nomad |
| `ASSET_LICENSES.md` | All third-party asset licenses |

---

## Third-Party Asset Licenses

All cosmetic assets have verified redistribution licenses:

| Asset | Source | License | Status |
|-------|--------|---------|--------|
| ClassicBlackSuit | MakeHuman Community | CC0 1.0 | ✅ Verified |
| DoubleBreastedSuit | MakeHuman Community | CC0 1.0 | ✅ Verified |
| DinnerJacket | MakeHuman Community | CC0 1.0 | ✅ Verified |
| Briefcase | OpenGameArt (loafbrr_1) | CC0 1.0 | ✅ Verified |
| Weapon GLB models (9/10) | petroulacl FPS Kit | CC0 | ✅ Verified |
| Remington 1875 | Vinish.dev | CC BY 4.0 | ✅ Verified |
| Pencil | Procedural | Original | ✅ Original |

**Full attribution**: `UnityProject/Assets/JohnWickArsenal/Models/Cosmetics/ASSET_CREDITS.txt`

---

## Troubleshooting Checklist

### "Unity executable not found"
- Verify Unity 2022.3.20f1 installed via Hub
- Check exact path matches OS convention
- Set `UNITY_EXECUTABLE` env var if non-standard location

### "Android Build Support not installed"
- Unity Hub → Installs → 2022.3.20f1 → ⋮ → Add Modules
- Check: Android SDK & NDK Tools, OpenJDK
- Verify `PlaybackEngines/AndroidPlayer/SDK` exists

### "License not activated"
- Unity Hub → License → Activate Personal
- Must be done in GUI (not headless)
- License is machine-bound

### "Assembly references missing"
- Install B&S Nomad
- Copy assemblies to `Assemblies/` folder
- Check `build.sh` for required assembly list

### "Out of memory during build"
- Increase swap (Linux): `sudo fallocate -l 8G /swapfile && sudo mkswap /swapfile && sudo swapon /swapfile`
- Close other applications
- Use `-batchmode -nographics` (already in build script)

### "Gradle build failed"
- Check `BuildOutput/*/unity-android-*.log`
- Verify JDK version matches Unity requirement
- Delete `Library/`, `Temp/`, `obj/`, `bin/` and retry

---

## What Remains Unfinished (Requires Target Machine)

| Task | Blocked By | Status |
|------|------------|--------|
| **Real Unity compilation** | Unity Editor + Android modules | ❌ Not tested |
| **Android AssetBundle build** | Unity Editor + Android modules | ❌ Not tested |
| **Full mod DLL compilation** | B&S Nomad assemblies | ❌ Not tested |
| **Meta Quest deployment/test** | Android build + Quest device | ❌ Not tested |
| **Accessory prefab generation** | Unity Editor (menu command) | ❌ Not tested |
| **NPC display prop validation** | Unity Editor + B&S runtime | ❌ Not tested |
| **In-game weapon testing** | Full mod build + Quest | ❌ Not tested |

---

## Verification Steps After Transfer

1. ✅ `python3 Tools/validate_android_sources.py` passes
2. ✅ `python3 Tools/build_local.py --check` passes
3. ✅ Unity opens project without red console errors
4. ✅ `Tools → John Wick Arsenal → Generate All (Prefabs + Catalog)` works
4. ✅ `python3 Tools/build_local.py --output BuildOutput` creates valid bundle
5. ✅ Bundle loads on Quest for visual review
6. ✅ Full mod build via `./build.sh` completes
7. ✅ Mod installs and works on Quest (BSMG/Mod.io)

---

## Support Resources

- **Repository**: https://github.com/jonnythebot/JohnWickArsenal
- **PR #2**: https://github.com/jonnythebot/JohnWickArsenal/pull/2
- **B&S Modding Discord**: BSMG #mod-development
- **Unity Documentation**: docs.unity3d.com/2022.3
- **Quest Development**: developer.oculus.com

---

*Last Updated: 2026-11-23*
*Branch: develop/unity-cosmetics-final*
*Unity Version: 2022.3.20f1 (61c2feb0970d)*
