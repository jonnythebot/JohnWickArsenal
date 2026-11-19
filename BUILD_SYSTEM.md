# John Wick Arsenal - Automated Build System

Complete automated build pipeline for Blade & Sorcery Nomad mod development.
Designed for **Linux/ChromeOS** with **remote build support** for low-spec machines.

## 🎯 Quick Start

### 1. Prerequisites (Local Build)
```bash
# Ubuntu/Debian/ChromeOS Crostini
sudo apt update && sudo apt install -y \
    dotnet-sdk-8.0 \
    zip \
    git \
    python3 \
    wget \
    curl

# Install Unity 2022.3 LTS via Unity Hub (recommended)
# Or via Flatpak: flatpak install flathub com.unity.UnityHub
```

### 2. Configure Environment
```bash
cd /home/wolfwolfwolf8586/JohnWickArsenal
cp .env.example .env
# Edit .env with your Unity path
```

### 3. Run Build
```bash
# Full automated build
./build.sh

# Individual steps
./build.sh deps      # Check dependencies only
./build.sh setup     # Initialize Unity project
./build.sh dll       # Build DLL only
./build.sh bundle    # Build AssetBundle only
./build.sh package   # Package mod
./build.sh verify    # Verify build
./build.sh clean     # Clean all outputs
```

---

## 🏗️ Build Pipeline Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    BUILD PIPELINE                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. DEPENDENCY CHECK                                            │
│     ├── Unity 2022.3 LTS (auto-detect: Hub, Flatpak, Snap)     │
│     ├── .NET SDK 8.0+                                           │
│     ├── Android SDK (optional - Unity has built-in)            │
│     ├── Git, zip, python3, adb                                 │
│     └── ChromeOS detection & storage warnings                   │
│                                                                  │
│  2. UNITY PROJECT SETUP                                         │
│     ├── Sync C# source to UnityProject/Assets/Scripts/         │
│     ├── Create packages-lock.json (XR, Input, Addressables)    │
│     ├── Configure Player Settings (Android/Quest)              │
│     ├── Configure Graphics (URP, OpenGL ES 3.2)                │
│     ├── Create folder structure (Models, Resources, etc.)      │
│     └── Initialize Addressables                                 │
│                                                                  │
│  3. BUILD MANAGED DLL (dotnet)                                  │
│     ├── dotnet restore                                          │
│     ├── dotnet build -c Release                                 │
│     └── Output: Assemblies/JohnWickArsenal.dll                 │
│                                                                  │
│  4. BUILD ASSETBUNDLE (Unity Batch Mode)                        │
│     ├── Collect prefabs from Resources/                         │
│     ├── Validate each prefab                                    │
│     ├── Build via Addressables or Legacy pipeline              │
│     ├── LZ4 compression for Android                             │
│     └── Output: AssetBundles/Android/johnwickarsenal           │
│                                                                  │
│  5. PACKAGE MOD                                                 │
│     ├── Copy: mod.json, icon.png, DLL, bundle, README         │
│     └── Create distribution ZIP                                 │
│                                                                  │
│  6. VERIFY BUILD                                                │
│     ├── Check all required files                                │
│     ├── Validate mod.json JSON                                  │
│     ├── Verify DLL loadable                                     │
│     └── Validate bundle contents                                │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🖥️ Unity Editor Automation Tools

Access via **Menu: John Wick Arsenal**

| Tool | Purpose |
|------|---------|
| **Project Bootstrap** | One-click project setup: packages, settings, folders, Addressables |
| **Prefab Setup Wizard** | Create weapon prefabs from models with auto-components |
| **Build AssetBundle** | Build Android AssetBundle with validation |
| **Build Validator** | Comprehensive validation (settings, prefabs, bundle, package) |
| **Dependency Checker** | Detect missing tools with platform-specific install guides |

### Project Bootstrap
```
Menu: John Wick Arsenal → Project Bootstrap
```
Automates:
- ✅ Installs required packages (XR, Oculus, Input System, Addressables, URP)
- ✅ Configures Player Settings for Quest (IL2CPP, ARM64, API 29+)
- ✅ Configures Graphics (URP, Linear color space, Mobile optimization)
- ✅ Creates folder structure
- ✅ Initializes Addressables groups

### Prefab Setup Wizard
```
Menu: John Wick Arsenal → Prefab Setup Wizard
```
Features:
- Drag & drop FBX/OBJ model
- Select weapon type (Pistol/Shotgun/Revolver/Rifle/Throwable)
- Auto-detects weapon from model name
- Creates proper hierarchy (Model, MuzzlePoint, EjectionPort, Grips, Sights, Tip)
- Adds all required components (Weapon/Gun/Item, Magazine, Rigidbody, Colliders, AudioSource)
- Adds custom behaviors (JohnWickPistolBehavior, etc.)
- Saves to Resources folder for Addressables
- Validates after creation

### Build Validator
```
Menu: John Wick Arsenal → Validate Build
```
Checks:
- Project Settings (Android, IL2CPP, API Level, Architecture, XR)
- Prefabs & Assets (components, transforms, colliders, tris, materials)
- Components & Scripts (behaviors, asmdef)
- Android/Quest Config (Graphics API, Packages)
- AssetBundle Output (size, contents, assets)
- Mod Package (required files, mod.json validity)

### Dependency Checker
```
Menu: John Wick Arsenal → Dependency Checker
```
Detects:
- Unity Editor (version, path)
- .NET SDK (version, path)
- Android SDK (path, ADB)
- Git, zip, ADB
- ChromeOS/Crostini (storage warnings)

Provides platform-specific installation guides.

---

## 📦 Remote Build (GitHub Actions / GitLab CI)

For **Chromebook/low-spec machines**, delegate builds to cloud CI.

### GitHub Actions Setup

1. **Add Secrets** in GitHub repo settings:
   ```
   UNITY_LICENSE          # Your Unity license (base64 encoded)
   ANDROID_KEYSTORE_BASE64 # Optional: keystore for signed builds
   ```

2. **Trigger Build**:
   - Go to Actions → Build Android Mod → Run workflow
   - Or push to main branch
   - Or create a tag (auto-creates release)

3. **Download Artifact**:
   - Actions → Build run → Artifacts → Download `JohnWickArsenal-mod-package`

### GitLab CI Setup

1. **Add CI/CD Variables** in GitLab:
   ```
   UNITY_LICENSE          # Your Unity license
   ```

2. **Pipeline runs on**:
   - Push to main
   - Tag creation (auto-deploys to Package Registry)

### Unity Cloud Build
Configure at: https://build.cloud.unity3d.com/
- Connect repo
- Set build target: Android
- Build method: `JohnWickArsenal.Editor.JWA_BuildAssetBundles.BuildBundle`

---

## 🔧 Configuration

### Environment Variables (.env)
```bash
# Core
MOD_NAME=JohnWickArsenal
UNITY_EXECUTABLE=/path/to/Unity          # Auto-detected if empty
UNITY_PROJECT_PATH=./UnityProject
BUILD_TARGET=Android
CONFIGURATION=Release

# Output
ASSEMBLY_OUTPUT=./Assemblies
BUNDLE_OUTPUT=./AssetBundles
MOD_OUTPUT=./BuildOutput

# ChromeOS Optimization
USE_TEMP_DIR=true
TEMP_BASE=/tmp/jwa-build
CLEAN_AFTER_BUILD=true

# Remote Build
REMOTE_BUILD=false
REMOTE_BUILD_PROVIDER=github
```

### Unity Version
Default: **2022.3.20f1** (LTS)
Override: `UNITY_VERSION=2022.3.25f1` or set in `.env`

---

## 📁 Project Structure

```
JohnWickArsenal/
├── .github/workflows/build-android.yml   # GitHub Actions pipeline
├── .gitlab-ci.yml                        # GitLab CI pipeline
├── .env.example                          # Configuration template
├── build.sh                              # Main build script (Linux/macOS)
├── mod.json                              # Mod manifest
├── icon.png                              # Mod icon
├── README.md                             # Mod documentation
├── BUILD_SYSTEM.md                       # This file
│
├── Source/JohnWickArsenal/               # C# source (shared)
│   ├── JohnWickArsenal.cs                # Main mod entry
│   ├── Behaviors/                        # Custom weapon behaviors
│   ├── Data/                             # Config loader
│   ├── Utils/                            # Registry
│   ├── Editor/                           # Editor scripts (source)
│   └── Tests/                            # Unit tests
│
├── UnityProject/                         # Unity project (generated)
│   ├── Assets/
│   │   ├── Scripts/JohnWickArsenal/      # Linked from Source/
│   │   ├── JohnWickArsenal/              # Mod assets
│   │   │   ├── Models/                   # Weapon models (FBX/OBJ)
│   │   │   ├── Resources/                # Prefabs (auto-generated)
│   │   │   ├── Animations/
│   │   │   ├── Audio/
│   │   │   ├── Materials/
│   │   │   ├── Textures/
│   │   │   └── Editor/                   # Editor tools
│   │   └── AddressableAssets/
│   ├── Packages/
│   │   └── packages-lock.json            # Package versions
│   └── ProjectSettings/
│
├── ModConfig/                            # JSON configuration
│   ├── weapons.json                      # Weapon stats, animations, audio
│   └── spawnmenu.json                    # Spawn menu layout
│
├── Assemblies/                           # Build output: DLL
├── AssetBundles/                         # Build output: bundles
└── BuildOutput/                          # Final mod package
```

---

## 🎮 Adding Weapons

### 1. Import Model
```
Assets/JohnWickArsenal/Models/
├── Pistols/
│   ├── HK_P30L.fbx
│   ├── Glock_34.fbx
│   └── ...
├── Shotguns/
│   ├── Benelli_M4.fbx
│   └── KSG.fbx
└── ...
```

### 2. Create Prefab
```
Menu: John Wick Arsenal → Prefab Setup Wizard
1. Select model (FBX/OBJ)
2. Choose weapon type
3. Select weapon definition
4. Click "Create Prefab"
5. Repeat for all 10 weapons
```

### 3. Batch Create (if models follow naming)
```
In Prefab Wizard → "Create All Weapon Prefabs (Batch)"
```

### 4. Configure Animations/Audio
Edit `ModConfig/weapons.json`:
```json
{
  "animations": {
    "draw": "P30L_Draw",
    "fire": "P30L_Fire",
    "reload_tactical": "P30L_Reload_Tactical"
  },
  "audio": {
    "fire": "P30L_Fire",
    "reload_mag_out": "P30L_MagOut"
  }
}
```

---

## ✅ Validation Checklist

Before releasing, run **Build Validator** and verify:

- [ ] All 10 weapon prefabs created and valid
- [ ] No missing scripts on any prefab
- [ ] All required transforms present (MuzzlePoint, EjectionPort, Grips, etc.)
- [ ] Triangle counts < 25k per weapon (Quest limit)
- [ ] Materials use URP-compatible shaders
- [ ] Android Player Settings: IL2CPP, ARM64, API 29+
- [ ] Graphics: URP, OpenGL ES 3.2, Linear color space
- [ ] Packages: XR Management, Oculus, Input System, Addressables, URP
- [ ] AssetBundle builds without errors
- [ ] Mod package contains all 5 required files
- [ ] mod.json is valid JSON with correct ID/version

---

## 🐛 Troubleshooting

### "Unity not found"
```bash
# Check Unity Hub install
ls ~/.local/share/unity3d/Hub/Editor/

# Or set manually
export UNITY_EXECUTABLE=/home/user/.local/share/unity3d/Hub/Editor/2022.3.20f1/Editor/Unity
```

### ".NET SDK not found"
```bash
# Ubuntu/Debian/ChromeOS
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update && sudo apt install -y dotnet-sdk-8.0

# Verify
dotnet --version
```

### "AssetBundle build fails"
1. Open Unity Editor
2. Menu: John Wick Arsenal → Validate Build
3. Fix all errors
4. Check `unity-build.log` for details

### "Out of disk space (ChromeOS)"
```bash
# Enable temp dir in .env
USE_TEMP_DIR=true
TEMP_BASE=/tmp/jwa-build

# Clean after build
CLEAN_AFTER_BUILD=true

# Or use remote build
REMOTE_BUILD=true
```

### "Missing Oculus XR package"
```bash
# In Unity Editor:
Menu: John Wick Arsenal → Project Bootstrap → Install Packages
```

---

## 📚 References

- [Blade & Sorcery Modding Wiki](https://github.com/BSMG/BladeAndSorcery.Modding.Wiki)
- [Unity 2022.3 LTS Documentation](https://docs.unity3d.com/2022.3/Documentation/Manual/index.html)
- [Quest Development Guide](https://developer.oculus.com/documentation/unity/unity-quest-support/)
- [Addressables Documentation](https://docs.unity3d.com/Packages/com.unity.addressables@1.21/manual/index.html)
- [GitHub Actions for Unity](https://github.com/game-ci/unity-builder)

---

*Build system designed for automated, reproducible mod development on any platform.*