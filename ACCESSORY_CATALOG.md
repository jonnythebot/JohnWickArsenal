# Accessory Catalog - John Wick Arsenal

## Overview
Modular cosmetic accessory system for weapon customization. All accessories are **purely visual** - no gameplay mechanics, damage changes, or functional effects.

## Accessory Categories

### 1. Flashlights (4 variants)
| Variant | Description | Mount | Tri Target |
|---------|-------------|-------|------------|
| Tactical_Standard | Full-size tactical weapon light | Picatinny | <500 |
| Tactical_Compact | Compact light for pistols | Picatinny | <300 |
| Tactical_IR | IR illuminator for NVG | Picatinny | <500 |
| Pressure_Switch | Remote pressure switch (cosmetic) | Picatinny | <200 |

### 2. Laser Modules (4 variants)
| Variant | Description | Mount | Tri Target |
|---------|-------------|-------|------------|
| Visible_Red | Red laser diode housing | Picatinny | <300 |
| Visible_Green | Green laser diode housing | Picatinny | <300 |
| IR_PEQ | PEQ-style IR laser/illuminator | Picatinny | <500 |
| DBAL_Combo | Visible/IR combo housing | Picatinny | <600 |

### 3. Grips (5 variants)
| Variant | Description | Mount | Tri Target |
|---------|-------------|-------|------------|
| Vertical_Tactical | Vertical foregrip | Picatinny/M-LOK | <400 |
| Angled_Magpul | Angled foregrip (AFG style) | Picatinny/M-LOK | <400 |
| Hand_Stop | Barricade hand stop | Picatinny/M-LOK | <200 |
| Pistol_Stippled | Stippled pistol grip texture | Frame-integrated | <300 |
| Pistol_Rubberized | Rubberized pistol grip | Frame-integrated | <300 |

### 4. Sights (7 variants)
| Variant | Description | Mount | Tri Target |
|---------|-------------|-------|------------|
| RMR_Type2 | Trijicon RMR Type 2 red dot | Picatinny/RMR cut | <400 |
| RMRcc | Compact RMR for pistols | RMRcc cut | <300 |
| EOTech_EXPS3 | EOTech EXPS3 holographic | Picatinny | <500 |
| Aimpoint_T2 | Aimpoint Micro T-2 red dot | Picatinny | <400 |
| Magnifier_G33 | Flip-to-side magnifier | Picatinny | <500 |
| BUIS_Front | Backup front sight (folding) | Picatinny | <200 |
| BUIS_Rear | Backup rear sight (folding) | Picatinny | <200 |

### 5. Muzzle Devices (5 variants)
| Variant | Type | Description | Mount | Tri Target |
|---------|------|-------------|-------|------------|
| Pistol_Short | Suppressor | Short pistol suppressor tube | Threaded barrel | <400 |
| Rifle_Full | Suppressor | Full-size rifle suppressor tube | Threaded barrel | <600 |
| Compensator_3Port | MuzzleBrake | 3-port compensator | Threaded barrel | <300 |
| Compensator_5Port | MuzzleBrake | 5-port compensator (TTI style) | Threaded barrel | <400 |
| Flash_Hider_A2 | MuzzleBrake | A2 birdcage flash hider | Threaded barrel | <300 |

### 6. Rail Covers (4 variants)
| Variant | Description | Mount | Tri Target |
|---------|-------------|-------|------------|
| Picatinny_2Slot | 2-slot Picatinny cover | Picatinny | <100 |
| Picatinny_4Slot | 4-slot Picatinny cover | Picatinny | <150 |
| MLOK_3Slot | 3-slot M-LOK cover | M-LOK | <100 |
| MLOK_5Slot | 5-slot M-LOK cover | M-LOK | <150 |

### 7. Sling Mounts (3 variants)
| Variant | Description | Mount | Tri Target |
|---------|-------------|-------|------------|
| QD_Socket | Quick-detach sling socket | Picatinny/Plate | <200 |
| Loop_Standard | Standard sling loop | Stock/Receiver | <150 |
| ASH_Plate | ASH receiver plate mount | Receiver | <250 |

### 8. Magwell & Charging Handles (4 variants)
| Variant | Type | Description | Mount | Tri Target |
|---------|------|-------------|-------|------------|
| Flared_Standard | MagwellFlare | Standard flared magwell | Magwell | <300 |
| Flared_Extended | MagwellFlare | Extended flared magwell | Magwell | <400 |
| Extended_Latch | ChargingHandle | Extended charging handle latch | Receiver | <200 |
| Ambidextrous | ChargingHandle | Ambidextrous charging handle | Receiver | <300 |

## Total: 36 Accessory Variants

## Prefab Structure

Each accessory prefab follows this hierarchy:
```
JWA_Accessory_<Type>_<Variant>
├── JWA_AccessoryBase (component)
├── MountPoint (Transform - REQUIRED)
│   └── (alignment reference for attachment)
├── OffsetPoint (Transform - OPTIONAL)
│   └── (fine positioning offset)
└── Model (GameObject)
    ├── MeshFilter + MeshRenderer
    └── (shared materials)
```

## JWA_AccessoryBase Component

Key properties:
- `type`: AccessoryType enum (Flashlight, LaserModule, Grip, etc.)
- `displayName`: Human-readable name
- `variant`: Variant identifier
- `mountPoint`: Primary attachment transform (REQUIRED)
- `offsetPoint`: Optional fine-tuning transform
- `compatibleMounts`: Array of mount types ("Picatinny", "M-LOK")
- `requiresGunsmith`: If true, only attachable at workbench
- `isCosmeticOnly`: Always true
- `maxPolygonCount`: Quest optimization target (default 500)

## JWA_AccessoryCatalog (ScriptableObject)

Central catalog asset at: `Assets/Resources/JWA_AccessoryCatalog.asset`

Contains all accessory entries with:
- Prefab references
- Descriptions
- Sort order for UI

## JWA_AccessoryManager (Runtime)

Attach to weapon prefabs to manage cosmetic attachments:
- `AttachAccessory(prefab, mountPoint)` - Add accessory
- `RemoveAccessory(type, mountPoint)` - Remove by type
- `RemoveAllAccessories()` - Clear all
- `GetAttachedAccessories()` - List current
- `ValidateAll()` - Check all setups

## Mount Point Standards

### Picatinny Rail
- Forward: +Z
- Up: +Y
- Right: +X
- Standard slot spacing: 0.206" (5.23mm)

### M-LOK
- Forward: +Z
- Up: +Y
- Right: +X
- Slot width: 0.330" (8.38mm)

### Threaded Barrel (Muzzle)
- Forward: +Z (toward muzzle)
- Center: Barrel bore axis

### RMR Cut
- Forward: +Z
- Up: +Y
- Mounting holes: 0.625" spacing

## Quest Optimization Rules

1. **Triangle Budget**: ≤500 per accessory (≤200 for rail covers/small items)
2. **Texture**: 1K max, ASTC 6x6 on Android
3. **Materials**: Share where possible (metal, polymer, glass)
4. **Draw Calls**: Combine meshes when multiple on same weapon
5. **LODs**: Not needed for accessories (always close to camera)

## Material Guidelines

| Material Type | Albedo | Normal | Metallic | Smoothness |
|---------------|--------|--------|----------|------------|
| Anodized Aluminum | Dark gray | Machining marks | 0.9 | 0.3 |
| Polymer (Black) | Near black | Mold texture | 0.0 | 0.1 |
| Glass (Lens) | Transparent | None | 0.0 | 1.0 |
| Steel (Muzzle) | Dark metal | Machining | 0.8 | 0.2 |
| Rubber | Dark gray | Texture | 0.0 | 0.05 |

## Creating New Accessories

1. Model in Blender (target <500 tris)
2. Export as FBX/OBJ with proper origin at MountPoint
3. Import to Unity, set up materials
4. Create prefab with hierarchy above
5. Add JWA_AccessoryBase component, configure
6. Add entry to JWA_AccessoryCatalog asset
7. Test attachment in scene

## File Locations

```
UnityProject/Assets/
├── Scripts/JohnWickArsenal/Accessories/
│   ├── JWA_AccessoryBase.cs
│   ├── JWA_AccessoryCatalog.cs
│   └── JWA_AccessoryManager.cs
├── Resources/
│   └── JWA_AccessoryCatalog.asset (create via menu)
└── JohnWickArsenal/Prefabs/Accessories/
    ├── Flashlights/
    ├── LaserModules/
    ├── Grips/
    ├── Sights/
    ├── MuzzleDevices/
    ├── RailCovers/
    ├── SlingMounts/
    └── MagwellCharging/
```

## License
All accessories are original procedural models created for this project. No external assets used.
