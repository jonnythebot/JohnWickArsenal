# John Wick Arsenal - Unity Build Instructions

## Prerequisites

### Software Requirements
- **Unity 2022.3 LTS** (required for Nomad compatibility)
- **Android Build Support** module installed
- **Blade & Sorcery Nomad** installed on Quest for testing
- **BSMG Mod Loader** or **Official Mod.io** for sideloading

### Unity Package Requirements
```
com.unity.xr.management@4.5.0
com.unity.xr.oculus@4.5.0
com.unity.inputsystem@1.7.0
com.unity.addressables@1.21.0
```

## Project Setup

### 1. Create Unity Project
```bash
# Create new 3D URP project
Unity Hub → New Project → 3D (URP) → Name: "JohnWickArsenal_Build"
```

### 2. Import Blade & Sorcery Nomad Modding SDK
- Download from BSMG GitHub: `BSMG/BladeAndSorcery.Modding.SDK`
- Import as custom package or clone into `Packages/`

### 3. Copy Source Code
```
Copy /Source/JohnWickArsenal/ → UnityProject/Assets/Scripts/JohnWickArsenal/
```

### 4. Assembly Definitions
The `JohnWickArsenal.asmdef` is already configured. Verify in Unity:
- Inspector → Assembly Definition → References should include:
  - BladeAndSorcery.Data
  - BladeAndSorcery.Gameplay
  - BladeAndSorcery.Controls
  - BladeAndSorcery.Nomad

## AssetBundle Creation

### 1. Prepare Assets
Organize your Assets folder:
```
Assets/
├── JohnWickArsenal/
│   ├── Models/
│   │   ├── Pistols/
│   │   │   ├── HK_P30L/
│   │   │   ├── Glock_34/
│   │   │   ├── STI_2011_CombatMaster/
│   │   │   └── TTI_PitViper/
│   │   ├── Shotguns/
│   │   │   ├── Benelli_M4/
│   │   │   └── KelTec_KSG/
│   │   ├── Revolvers/
│   │   │   └── Remington_1875/
│   │   ├── Rifles/
│   │   │   └── TTI_TR1_Ultralight/
│   │   └── Throwables/
│   │       ├── ThrowingKnife/
│   │       └── Pencil/
│   ├── Animations/
│   ├── Audio/
│   ├── Materials/
│   ├── Prefabs/
│   └── Resources/
```

### 2. Prefab Setup for Each Weapon

#### Required Components (Guns):
```
Weapon Prefab
├── Weapon (Script)
├── Gun (Script)
├── Magazine (Script)
├── Rigidbody
├── Collider(s)
├── JohnWickPistolBehavior/ShotgunBehavior/etc.
├── AudioSource
├── ParticleSystem (muzzle flash)
└── Child Objects:
    ├── Model (MeshRenderer)
    ├── Magazine_Model
    ├── Slide/Bolt
    ├── MuzzlePoint (Transform)
    ├── EjectionPort (Transform)
    ├── Grip_L (Transform)
    ├── Grip_R (Transform)
    └── Sight_Front / Sight_Rear (Transforms)
```

#### Required Components (Throwables):
```
Throwable Prefab
├── Item (Script)
├── Throwable (Script)
├── Rigidbody
├── Collider (Convex)
├── ThrowingKnifeBehavior / PencilBehavior
├── AudioSource
├── TrailRenderer (optional)
└── Child Objects:
    ├── Model
    └── Tip (Transform for penetration)
```

### 3. Animation Clips
Create/import these animation clips for each weapon:
- `Draw` - Pull from holster
- `Idle` - Holding animation
- `Fire` - Single shot recoil
- `Fire_Auto` - Full auto (rifles)
- `Reload_Tactical` - Reload with round in chamber
- `Reload_Empty` - Reload from empty
- `ChargingHandle` / `SlideRack` / `Pump` - Manual cycling
- `Inspect` - Weapon inspection
- `Holster` - Return to holster

### 4. Build AssetBundle

#### Editor Script (Assets/Editor/BuildAssetBundles.cs):
```csharp
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Pipeline;

public class BuildAssetBundles
{
    [MenuItem("John Wick Arsenal/Build AssetBundle")]
    public static void Build()
    {
        string outputPath = "AssetBundles/Android";
        System.IO.Directory.CreateDirectory(outputPath);

        var buildParams = new AssetBundleBuildParams
        {
            outputPath = outputPath,
            targetPlatform = BuildTarget.Android,
            compression = BuildCompression.LZ4,
            bundleName = "johnwickarsenal"
        };

        // Collect all prefabs in JohnWickArsenal/Resources
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/JohnWickArsenal/Resources" });
        var builds = new List<AssetBundleBuild>();

        var bundle = new AssetBundleBuild();
        bundle.assetBundleName = "johnwickarsenal";
        bundle.assetNames = Array.ConvertAll(prefabGuids, guid => AssetDatabase.GUIDToAssetPath(guid));
        builds.Add(bundle);

        BuildPipeline.BuildAssetBundles(outputPath, builds.ToArray(), BuildAssetBundleOptions.None, BuildTarget.Android);
        
        Debug.Log($"AssetBundle built to: {outputPath}/johnwickarsenal");
    }

    [MenuItem("John Wick Arsenal/Copy to Mod Folder")]
    public static void CopyToModFolder()
    {
        string source = "AssetBundles/Android/johnwickarsenal";
        string dest = Application.dataPath.Replace("/Assets", "") + "/../JohnWickArsenal/AssetBundles/johnwickarsenal.bundle";
        
        if (System.IO.File.Exists(source))
        {
            System.IO.File.Copy(source, dest, true);
            Debug.Log($"Copied to mod folder: {dest}");
        }
    }
}
```

### 5. Build Steps
1. Open Unity project
2. Place all prefabs in `Assets/JohnWickArsenal/Resources/` (matching prefab paths in code)
3. Run `John Wick Arsenal → Build AssetBundle`
4. Run `John Wick Arsenal → Copy to Mod Folder`
4. Open `JohnWickArsenal` folder in terminal
5. Run: `dotnet build` (if using .NET for DLL compilation) OR compile Assembly-CSharp.dll via Unity

## DLL Compilation

### Option A: Unity Compilation (Recommended)
1. In Unity: `Build → Build Settings → Android`
2. `Build → Compile Assembly-CSharp.dll`
3. Copy `Library/ScriptAssemblies/JohnWickArsenal.dll` to mod folder

### Option B: Command Line (dotnet)
```bash
cd /home/wolfwolfwolf8586/JohnWickArsenal
dotnet build Source/JohnWickArsenal/JohnWickArsenal.csproj \
  -r android-arm64 \
  -c Release \
  -o ./Assemblies
```

## Final Mod Package Structure

```
JohnWickArsenal/
├── mod.json
├── icon.png (512x512)
├── JohnWickArsenal.dll
├── johnwickarsenal.bundle
└── README.md
```

## Installation on Quest

### Via BSMG Mod Loader (Recommended)
1. Enable Developer Mode on Quest
2. Install BSMG Mod Loader via SideQuest
3. Copy mod folder to: `/sdcard/Mods/JohnWickArsenal/`
4. Launch Blade & Sorcery Nomad
5. Enable in Mods menu

### Via Mod.io (Official)
1. Package as `.zip` with manifest
2. Upload to Mod.io
3. Subscribe in-game

## Testing Checklist

### Per Weapon:
- [ ] Spawns correctly in menu
- [ ] Animations play (draw, idle, fire, reload, inspect)
- [ ] Sounds play correctly
- [ ] Particles emit (muzzle flash, ejection, gas)
- [ ] Damage values feel right
- [ ] Recoil/spread matches config
- [ ] Magazine inserts/ejects properly
- [ ] Custom behavior works (compensator, dual tube, etc.)

### Throwables:
- [ ] Throw arc feels natural
- [ ] Stick/retrieve works
- [ ] Damage applied correctly
- [ ] Headshot multipliers work
- [ ] Pencil instant-kill + break after 3

### Nomad Specific:
- [ ] Performance: < 2ms GPU per weapon
- [ ] No pink shaders (missing materials)
- [ ] No missing script errors in log
- [ ] Works in multiplayer (if applicable)

## Troubleshooting

### Common Issues

**Weapon invisible / pink material**
- Check material shaders are URP/Lit compatible
- Ensure textures are in AssetBundle

**Animations don't play**
- Verify AnimationClips are in AssetBundle
- Check Animator Controller references

**Script errors in log**
- Ensure DLL is compiled against correct B&S version
- Check assembly references in asmdef

**Crash on spawn**
- Check prefab has all required components
- Verify prefab path matches `PrefabPaths` class

**Performance issues**
- Reduce polygon count (< 15k tris for pistols, < 30k for rifles)
- Use texture atlases
- LODs for distance

## Asset Requirements Summary

| Weapon | Tris Target | Textures | Anims | Sounds |
|--------|------------|----------|-------|--------|
| P30L | 12k | 4 (Albedo, Normal, ORM, Emissive) | 7 | 7 |
| Glock 34 | 10k | 4 | 7 | 7 |
| STI 2011 | 15k | 5 (Gold accents) | 7 | 7 |
| TTI Pit Viper | 18k | 5 (Gold TiN) | 7 | 7 |
| Benelli M4 | 20k | 4 | 8 | 5 |
| Kel-Tec KSG | 18k | 4 | 9 | 5 |
| Remington 1875 | 15k | 4 (Case-hardened) | 10 | 8 |
| TTI TR-1 | 25k | 5 (Carbon fiber) | 9 | 8 |
| Throwing Knife | 3k | 2 | 5 | 4 |
| Pencil | 1k | 2 | 6 | 5 |

## Resources

- **BSMG Discord**: discord.gg/bladesorcery
- **Nomad Modding Wiki**: github.com/BSMG/BladeAndSorcery.Modding.Wiki
- **Unity XR Interaction Toolkit**: docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit
- **Taran Tactical Reference**: tarantactical.com (for accurate modeling)

---

*Good luck, Baba Yaga. 🖋️*