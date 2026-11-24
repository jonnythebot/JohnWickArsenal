# Local Unity Build Guide - John Wick Arsenal

## Overview
This guide explains how to build the Android cosmetic review bundle locally using Unity Personal. Per Unity Support, Unity Personal **cannot be activated in headless CI/CD** - builds must run on a machine with Unity Hub and an activated license.

## Prerequisites

### 1. Unity Installation
- **Unity Hub**: Install from https://unity.com/download
- **Unity Editor**: 2022.3.20f1 (changeset 61c2feb0970d)
  - In Hub: Installs → Add → 2022.3.20f1
- **Android Build Support**: Required modules
  - Android SDK & NDK Tools
  - OpenJDK
  - (Installed via Hub when adding modules to 2022.3.20f1)

### 2. License Activation
1. Open Unity Hub
2. Sign in with Unity ID
3. License → Manage → Activate Personal License
4. Verify: Hub shows "Personal" license for 2022.3.20f1

### 3. Repository Setup
```bash
git clone https://github.com/jonnythebot/JohnWickArsenal.git
cd JohnWickArsenal
git checkout develop/unity-cosmetics-final
```

## Validation (No Unity Required)

Run source validation before building:
```bash
python3 Tools/validate_android_sources.py
```

Checks:
- ProjectVersion.txt pins 2022.3.20f1 (61c2feb0970d)
- Package manifest.json valid with correct dependencies
- packages-lock.json valid
- mod.json and ModConfig/*.json valid
- All .asmdef files valid JSON
- build_local.py and JwaLocalAndroidBuild.cs exist

Expected output: `Source checks passed for Unity 2022.3.20f1. Run the local build to compile and export.`

## Check Unity Installation

```bash
python3 Tools/build_local.py --check
```

Verifies:
- Unity 2022.3.20f1 found in Hub locations
- Android Build Support modules present (SDK, NDK, OpenJDK)

## Build Cosmetic Review Bundle

```bash
python3 Tools/build_local.py --output BuildOutput
```

### What This Does
1. Launches Unity 2022.3.20f1 in batch mode
2. Opens `UnityProject` 
3. Switches to Android build target
4. Runs `JwaLocalAndroidBuild.ExportBatch()`
5. Exports cosmetic models to AssetBundle: `johnwickarsenal-cosmetics.bundle`
6. Outputs to `BuildOutput/cosmetics-android-<timestamp>/`

### Output Files
```
BuildOutput/
└── cosmetics-android-<timestamp>/
    ├── johnwickarsenal-cosmetics.bundle  # Android AssetBundle
    └── unity-android-<timestamp>.log     # Unity build log
```

### Bundle Contents
The bundle includes all models under these paths:
- `Assets/JohnWickArsenal/Models/Suits/`
- `Assets/JohnWickArsenal/Cosmetics/`
- `Assets/JohnWickArsenal/Models/Cosmetics/`

Models included:
- 5 suits (ClassicBlackSuit, BlackTacticalSuit, FormalThreePieceSuit, ArmoredCombatSuit, EliteHighProtectionSuit)
- 2 cosmetic garments (DoubleBreastedSuit, DinnerJacket)
- 1 prop (Briefcase)

## Build Full Mod (Requires B&S Nomad)

For a complete mod build with DLL and weapon AssetBundle:

### Prerequisites
- Blade & Sorcery Nomad installed (for assembly references)
- Unity 2022.3.20f1 with Android Build Support

### Using build.sh (Legacy - Comprehensive)
```bash
./build.sh
```

Options:
```bash
./build.sh deps      # Check dependencies only
./build.sh setup     # Initialize Unity project
./build.sh dll       # Build DLL only
./build.sh bundle    # Build AssetBundle only
./build.sh package   # Package mod ZIP
./build.sh verify    # Verify build outputs
./build.sh clean     # Clean all outputs
```

### Manual Steps
```bash
# 1. Build DLL (requires B&S assemblies)
cd Source/JohnWickArsenal
dotnet build -c Release

# 2. Build AssetBundle (requires Unity)
# Open UnityProject in Unity 2022.3.20f1
# Menu: John Wick Arsenal → Build AssetBundle

# 3. Package
# Copy to BuildOutput:
# - mod.json
# - icon.png
# - Assemblies/JohnWickArsenal.dll
# - AssetBundles/Android/johnwickarsenal.bundle
# - README.md
# Create ZIP
```

## Troubleshooting

### "Unity executable not found"
- Install Unity 2022.3.20f1 via Hub
- Or set `UNITY_EXECUTABLE` environment variable

### "Android Build Support not installed"
- In Hub: Installs → 2022.3.20f1 → ⋮ → Add Modules
- Check: Android SDK & NDK Tools, OpenJDK

### "Unity license not activated"
- Open Unity Hub → License → Activate Personal
- Must be done in GUI, not headless

### "ProjectVersion.txt mismatch"
- Open UnityProject in correct Unity version
- Let Unity upgrade/migrate project
- Check ProjectSettings/ProjectVersion.txt

### "Package manifest errors"
- Run `python3 Tools/validate_android_sources.py`
- Check UnityProject/Packages/manifest.json syntax
- Delete Packages/packages-lock.json and let Unity regenerate

### "asmdef compilation errors"
- Open in Unity to see actual errors
- Check define constraints (JWA_LEGACY_PROTOTYPE)
- Verify references exist

### Build fails silently
- Check the log file: `BuildOutput/cosmetics-android-*/unity-android-*.log`
- Look for: "Error:", "Exception:", "Failed"

## GitHub Actions (CI Checks Only)

The GitHub Actions workflow (`.github/workflows/build-android.yml`) runs:
- Source validation (validate_android_sources.py)
- Code style checks
- **Does NOT build Unity project** (requires license)

This is by design - Unity Personal cannot run in headless CI.

## File Reference

### Scripts
| File | Purpose |
|------|---------|
| `Tools/build_local.py` | Main build orchestrator |
| `Tools/validate_android_sources.py` | Pre-build validation |
| `UnityProject/Assets/LocalTools/Editor/JwaLocalAndroidBuild.cs` | Unity batch export |
| `UnityProject/Assets/LocalTools/Editor/JwaCosmeticTextureImport.cs` | Texture import settings |

### Configuration
| File | Purpose |
|------|---------|
| `UnityProject/ProjectSettings/ProjectVersion.txt` | Unity version pin |
| `UnityProject/Packages/manifest.json` | Package dependencies |
| `.env.example` | Environment variables template |

## Next Steps After Build

1. **Verify Bundle**: Open in Unity AssetBundle Browser or load at runtime
2. **Test on Quest**: Deploy bundle to Quest for visual review
3. **Iterate**: Fix materials, scale, UV issues
4. **Full Mod Build**: When ready, build DLL + weapon bundle via build.sh

## Support

- Unity version issues: https://forum.unity.com/
- B&S Nomad modding: BSMG Discord #mod-development
- This project: GitHub Issues
