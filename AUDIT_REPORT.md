# JohnWickArsenal - Audit Report

## Critical Issues Found

### 1. Missing UnityProject/Packages/manifest.json
- **File**: `UnityProject/Packages/manifest.json` - **MISSING**
- Only `packages-lock.json` exists
- Unity requires manifest.json to resolve package dependencies

### 2. Incorrect/Unverified Blade & Sorcery API References
**Source/JohnWickArsenal/JohnWickArsenal.asmdef** references:
- `BladeAndSorcery.Data` - Unverified for Nomad 1.3.1
- `BladeAndSorcery.Gameplay` - Unverified for Nomad 1.3.1  
- `BladeAndSorcery.Controls` - Likely doesn't exist in Nomad
- `BladeAndSorcery.Nomad` - Unverified assembly name

**C# scripts use unverified types:**
- `Mod` base class - namespace unclear
- `WeaponData`, `ItemData` - from which assembly?
- `WeaponBehavior`, `ItemBehavior` - from which assembly?
- `Catalog` - static class for registration
- `RagdollPart`, `Creature`, `DamageType` - may not exist in Nomad
- `WeaponData.Type`, `WeaponData.GunType`, `WeaponData.ReloadType` - enum structure unverified
- `ItemData.Type`, `ItemData.ThrowableType` - enum structure unverified

### 3. Invalid Assembly Definition References
**Source/JohnWickArsenal/JohnWickArsenal.asmdef** incorrectly references UnityEngine modules:
```json
"references": [
  "UnityEngine",           // Not a valid asmdef reference
  "UnityEngine.CoreModule", // These are Unity internal modules, not asmdefs
  "UnityEngine.PhysicsModule",
  "UnityEngine.AudioModule",
  "UnityEngine.AnimationModule",
  "UnityEngine.InputLegacyModule",
  "UnityEngine.UI"
]
```
Unity asmdefs should reference **other asmdef names**, not module names. UnityEngine types are available by default.

### 4. csproj Uses Unresolvable Environment Variables
```xml
<HintPath>$(UNITY_PATH)/Data/Managed/UnityEngine/UnityEngine.CoreModule.dll</HintPath>
<HintPath>$(BLADE_SORCERY_PATH)/BladeAndSorcery.Data.dll</HintPath>
```
- `$(UNITY_PATH)` not set in CI
- `$(BLADE_SORCERY_PATH)` requires proprietary game files
- Cannot compile in CI without actual game assemblies

### 5. GitHub Actions Workflow Issues
- **Broken Unity download URLs** using version hashes that don't exist publicly
- **No mod.json trigger** - workflow won't run when mod manifest changes
- **No Unity license handling** - batch mode requires valid license
- **DLL build job will always fail** without B&S game assemblies in CI
- **AssetBundle build job downloads Unity again** (duplicate, different URL)

### 6. UnityProject asmdef Missing B&S References
```json
"references": [
  "Unity.InputSystem",
  "Unity.Addressables", 
  "Unity.XR.Management",
  "Unity.XR.Oculus"
]
```
Missing references to B&S assemblies that the scripts actually use.

### 7. API Types That May Not Exist in Nomad 1.3.1
Based on B&S Nomad modding community knowledge:
- `BladeAndSorcery.Controls` - PC only, not in Nomad
- `BladeAndSorcery.Nomad` - Not a standard assembly name
- `RagdollPart` - May be `RagdollPart` or different
- `Creature.Damage()` signature - Unverified
- `WeaponData.ReloadType` enum values - Unverified
- `ItemData.ThrowableType.Improvised` - May not exist

## Required Fixes

1. ✅ Create `UnityProject/Packages/manifest.json`
2. ✅ Fix Source asmdef - remove UnityEngine module references, keep only B&S
3. ✅ Fix UnityProject asmdef - add B&S references
4. ✅ Update csproj - use conditional references, document local-only build
5. ✅ Fix GitHub Actions workflow - correct Unity URL, add mod.json trigger, add license
6. ✅ Separate CI jobs: DLL build (local only) vs AssetBundle build (CI compatible)
7. ✅ Verify and fix B&S API usage based on community documentation
8. ✅ Add proper mod.json workflow trigger

## Missing Requirements for True CI Build
- **Unity License**: Required for batch mode (set as `UNITY_LICENSE` secret)
- **B&S Game Assemblies**: Cannot be distributed; must be provided by developer
- **Quest Device**: For testing the final mod
- **Android Signing Keys**: For distribution (optional for dev builds)
