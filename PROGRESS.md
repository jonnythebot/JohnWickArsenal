# John Wick Arsenal - Development Progress

**Last Updated:** 2026-11-21
**Target:** B&S Nomad 1.3.1 | Unity 2022.3 LTS | Meta Quest (ARM64)

---

## Completed Tasks

- [x] Repository audited - all C# systems complete (7/7 systems done)
- [x] Blender 4.3.2 installed and verified
- [x] Python 3.13 with numpy, trimesh, pygltflib installed
- [x] Build system analyzed (build.sh, Unity project structure)
- [x] Prefab Setup Wizard reviewed and ready for use
- [x] mod.json, spawnmenu.json, weapons.json configuration verified
- [x] **Generated 10 weapon models as OBJ** (Pistols: HK_P30L, Glock_34, STI_2011_CombatMaster, TTI_PitViper; Shotguns: Benelli_M4, KelTec_KSG; Revolver: Remington_1875; Rifle: TTI_TR1_Ultralight; Throwables: ThrowingKnife, Pencil)
- [x] **Generated 11 equipment/holster models as OBJ** (4 holsters, 7 pouches)
- [x] **Generated 5 suit models as OBJ** (ClassicBlackSuit, BlackTacticalSuit, FormalThreePieceSuit, ArmoredCombatSuit, EliteHighProtectionSuit)
- [x] Procedural PBR materials assigned to all models (MTL files generated)
- [x] **Created JWA_AutoPrefabCreator.cs** - Automated Unity Editor script for batch prefab creation from OBJ models
- [x] **Updated GitHub Actions workflow** with CI-friendly asmdef and stub B&S assemblies for compilation
- [x] **Synced all Source scripts to UnityProject** - 26 C# files including behaviors, systems, editors
- [x] **All 25 OBJ models + 25 MTL materials copied to UnityProject/Assets/JohnWickArsenal/Models/**
- [x] **GitHub Actions workflow configured** - Uses official unityci/editor:2022.3.20f1-android-4 Docker image
- [x] **Unity license activation configured** - Activates when UNITY_USERNAME/UNITY_PASSWORD secrets are set

---

## Current Status

### Blockers
- **Unity Editor 2022.3 LTS not installed locally** - Cannot build AssetBundles or compile DLL locally
- **Unity Hub flatpak requires GUI** - Cannot run headless in current environment
- **GitHub Actions Unity License not configured** - CI build requires UNITY_USERNAME, UNITY_PASSWORD, UNITY_SERIAL secrets
- **Workaround**: Create all 3D assets with Blender/Python, prepare Unity project for remote CI/CD build

### In Progress
- [x] Convert OBJ models to FBX for Unity (or configure Unity to import OBJ) - **Unity imports OBJ natively**
- [x] Set up Unity project with generated assets
- [x] Configure Prefab Setup Wizard for batch import
- [x] Create Unity Editor script for automated prefab creation from generated models
- [x] **GitHub Actions workflow ready** - Uses official Unity Docker image
- [x] **Fixed workflow `if:` condition** - Added secrets to workflow-level env
- [ ] Configure GitHub Secrets for Unity license activation (UNITY_LICENSE required)
- [ ] Download built AssetBundle and DLL from artifacts
- [ ] Package final mod ZIP

---

## Next Steps (Priority Order)

1. **Configure GitHub Secrets** (REQUIRED - Unity Personal needs ULF for headless CI):
   - **UNITY_LICENSE** = content of `~/.local/share/unity3d/Unity/Unity_lic.ulf` (from local Unity activation)
   - UNITY_USERNAME = jm28lockwood@yahoo.com
   - UNITY_PASSWORD = 7461651aA@@@
   - UNITY_SERIAL = (optional, for Pro/Enterprise)
   - Go to: https://github.com/jonnythebot/JohnWickArsenal/settings/secrets/actions

2. **Trigger GitHub Actions build** - Push to master or manual dispatch via Actions tab

3. **Download built AssetBundle and DLL from artifacts** (once build completes)

4. **Package final mod ZIP** with mod.json, icon.png, DLL, bundle, README

5. **Build real DLL locally** - Run `./build.sh` (requires B&S Nomad installed) for production mod

---

## Asset Replacement Initiative (IN PROGRESS)

**Status:** 9/10 weapons replaced with high-quality CC0/CC-BY models. Equipment/Suits remain procedural for v1.0.

### Verified Free Asset Sources:
1. **petroulacl FPS Asset Kit** (CC0) - 20 weapons, GLB/FBX/OBJ, 2K PBR textures ✅ **DOWNLOADED**
2. **Vinish.dev** (CC BY 4.0) - 10 weapons, GLB/GLTF/OBJ ✅ **DOWNLOADED**
3. **CC0Tree** (CC0) - Game-ready FBX models, weekly updates ✅
4. **Sketchfab Tactical Gear** (CC BY 4.0) - 45.7k tris, equipment candidate ✅
5. **Uju Studio/Sketchfab** (CC0) - Military weapons collection ✅
6. **BlendSwap** (CC0/CC-BY) - Clothing, tactical gear (download requires auth) ⚠️

### Replacement Progress:
| Category | Count | Replaced | Target Source | Status |
|----------|-------|----------|---------------|--------|
| Weapons | 10 | 9/10 | petroulacl FPS Kit + Vinish.dev | ✅ **9 DONE** |
| Equipment | 11 | 0/11 | Sketchfab/CC0Tree (v1.1+) | 🔄 v1.0 Procedural |
| Suits | 5 | 0/5 | CC0Tree/Sketchfab (v1.1+) | 🔄 v1.0 Procedural |

### Weapon Replacements Complete:
| # | John Wick Weapon | Replacement | Source | License | File |
|---|------------------|-------------|--------|---------|------|
| 1 | HK P30L | Pistol_Compact_West | petroulacl FPS Kit | CC0 | HK_P30L.glb ✅ |
| 2 | Glock 34 | Pistol_Full_West | petroulacl FPS Kit | CC0 | Glock_34.glb ✅ |
| 3 | STI 2011 | Pistol_Compact_East | petroulacl FPS Kit | CC0 | STI_2011_CombatMaster.glb ✅ |
| 4 | TTI Pit Viper | Pistol_Full_East | petroulacl FPS Kit | CC0 | TTI_PitViper.glb ✅ |
| 5 | Benelli M4 | Shotgun_Auto_West | petroulacl FPS Kit | CC0 | Benelli_M4.glb ✅ |
| 6 | Kel-Tec KSG | Shotgun_Pump_West | petroulacl FPS Kit | CC0 | KelTec_KSG.glb ✅ |
| 7 | Remington 1875 | Revolver | Vinish.dev | CC BY 4.0 | Remington_1875.glb ✅ |
| 8 | TTI TR-1 | Rifle_Battle_West | petroulacl FPS Kit | CC0 | TTI_TR1_Ultralight.glb ✅ |
| 9 | Throwing Knife | Combat Knife | Vinish.dev | CC BY 4.0 | ThrowingKnife.glb ✅ |
| 10 | Pencil | Keep procedural | — | — | Pencil.obj 🔄 |

### Technical Updates:
- ✅ JWA_AutoPrefabCreator.cs updated to support GLB + OBJ formats
- ✅ 9 weapon GLB files copied to UnityProject/Assets/JohnWickArsenal/Models/
- ✅ Procedural OBJ backups preserved in Backups/ProceduralModels/
- ✅ ASSET_LICENSES.md created with full attribution

### Next Steps:
- [ ] Configure UNITY_LICENSE GitHub secret (ULF file) for CI build
- [ ] Trigger GitHub Actions build to test GLB import + prefab creation
- [ ] Download Sketchfab tactical gear for equipment (v1.1)
- [ ] Download CC0Tree suit models for suits (v1.1)
- [ ] Build real DLL locally with `./build.sh` (requires B&S Nomad)

---

## Asset Generation Status

### Weapons (10/10 models generated ✓)
| Weapon | Type | File | Size | Status |
|--------|------|------|------|--------|
| H&K P30L | Pistol | Pistols/HK_P30L.obj | 42 KB | ✓ Generated |
| Glock 34 Gen 5 MOS | Pistol | Pistols/Glock_34.obj | 28 KB | ✓ Generated |
| STI 2011 Combat Master | Pistol | Pistols/STI_2011_CombatMaster.obj | 48 KB | ✓ Generated |
| TTI Pit Viper | Pistol | Pistols/TTI_PitViper.obj | 57 KB | ✓ Generated |
| Benelli M4 Super 90 | Shotgun | Shotguns/Benelli_M4.obj | 70 KB | ✓ Generated |
| Kel-Tec KSG | Shotgun | Shotguns/KelTec_KSG.obj | 24 KB | ✓ Generated |
| Remington 1875 | Revolver | Revolvers/Remington_1875.obj | 142 KB | ✓ Generated |
| TTI TR-1 Ultralight | Rifle | Rifles/TTI_TR1_Ultralight.obj | 82 KB | ✓ Generated |
| Throwing Knife | Throwable | Throwables/ThrowingKnife.obj | 38 KB | ✓ Generated |
| Pencil | Throwable | Throwables/Pencil.obj | 20 KB | ✓ Generated |

### Equipment (11/11 models generated ✓)
| Item | Type | File | Size | Status |
|------|------|------|------|--------|
| Shoulder Holster (Double) | Holster | Equipment/ShoulderHolster_Double.obj | 84 KB | ✓ Generated |
| Hip Holster (TTI Race) | Holster | Equipment/HipHolster_TTIRace.obj | 83 KB | ✓ Generated |
| Thigh Holster (Tactical) | Holster | Equipment/ThighHolster_Tactical.obj | 4 KB | ✓ Generated |
| Ankle Holster (Concealed) | Holster | Equipment/AnkleHolster_Concealed.obj | 118 KB | ✓ Generated |
| Chest Rig (Mag Carrier) | Pouch | Equipment/ChestRig_MagCarrier.obj | 11 KB | ✓ Generated |
| Belt Pouches (4x) | Pouch | Equipment/BeltPouches_4x.obj | 8 KB | ✓ Generated |
| Benelli M4 Shell Carrier | Pouch | Equipment/BenelliM4_ShellCarrier.obj | 2 KB | ✓ Generated |
| KSG Dual Tube Carrier | Pouch | Equipment/KSG_DualTubeCarrier.obj | 2 KB | ✓ Generated |
| Knife Bandolier | Pouch | Equipment/KnifeBandolier.obj | 7 KB | ✓ Generated |
| Pencil Case | Pouch | Equipment/PencilCase.obj | 6 KB | ✓ Generated |

### Suits (5/5 models generated ✓)
| Suit | File | Size | Status |
|------|------|------|--------|
| Classic Black Suit (Ch.1) | Suits/ClassicBlackSuit.obj | 3 KB | ✓ Generated |
| Black Tactical Suit (Ch.2) | Suits/BlackTacticalSuit.obj | 17 KB | ✓ Generated |
| Formal Three-Piece Suit (Ch.3) | Suits/FormalThreePieceSuit.obj | 13 KB | ✓ Generated |
| Armored Combat Suit (Ch.4) | Suits/ArmoredCombatSuit.obj | 16 KB | ✓ Generated |
| Elite High-Protection Suit | Suits/EliteHighProtectionSuit.obj | 14 KB | ✓ Generated |

---

## GitHub Actions CI/CD Configuration

### Required Secrets (Settings → Secrets and variables → Actions)
| Secret Name | Description | Required |
|-------------|-------------|----------|
| UNITY_USERNAME | Unity account email | Yes |
| UNITY_PASSWORD | Unity account password | Yes |
| UNITY_SERIAL | Unity Pro/Enterprise serial (optional for Personal) | No |
| UNITY_LICENSE | License file content (alternative to username/password) | No |

### Workflow Features
- Uses official `unityci/editor:2022.3.20f1-android-4` Docker image
- Installs Android SDK (API 33, NDK 25.1.8937393)
- Caches Unity Library, .NET packages, Android SDK
- Creates stub B&S assemblies for CI compilation
- Runs Auto Prefab Creator to generate prefabs from OBJ models
- Builds Android AssetBundle using Addressables pipeline
- Packages mod ZIP with mod.json, icon.png, DLL, bundle, README

---

## Notes

- All modeling done via Blender Python API (bpy) - no manual Blender work needed
- Materials generated procedurally - MTL files with PBR properties (metallic, roughness)
- OBJ format used (FBX export requires numpy which isn't available in Blender's embedded Python)
- Unity can import OBJ files natively
- Quest-optimized: low poly models, efficient UVs implied
- Unity project will be built via GitHub Actions (remote CI/CD) since local Unity not available
- All models include proper hierarchy with Unity-required transforms (MuzzlePoint, EjectionPort, Grip_L, Grip_R, Sight_Front, Sight_Rear, Tip)