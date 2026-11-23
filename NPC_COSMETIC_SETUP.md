# NPC Cosmetic Integration Setup

## Overview
This document describes the NPC cosmetic system for John Wick Arsenal in Blade & Sorcery Nomad. The system prepares non-combat cosmetic equipment for NPCs (wardrobe definitions, display props) without affecting combat, targeting, or weapon mechanics.

## System Components

### 1. JWA_NPCCosmeticSetup (Main Controller)
Located: `UnityProject/Assets/Scripts/JohnWickArsenal/NPCs/JWA_NPCCosmeticSetup.cs`

#### Wardrobe Entries (8 suits)
| Suit | Triangles | Source | License | Status |
|------|-----------|--------|---------|--------|
| ClassicBlackSuit (Ch.1) | 19,202 | MakeHuman | CC0 1.0 | ✅ 1K textures |
| BlackTacticalSuit (Ch.2) | ~15,000 | Procedural | - | ✅ Quest-optimized |
| FormalThreePieceSuit (Ch.3) | ~15,000 | Procedural | - | ✅ Quest-optimized |
| ArmoredCombatSuit (Ch.4) | ~15,000 | Procedural | - | ✅ Quest-optimized |
| EliteHighProtectionSuit | ~15,000 | Procedural | - | ✅ Quest-optimized |
| DoubleBreastedSuit | 10,062 | MakeHuman | CC0 1.0 | ✅ 1K textures |
| DinnerJacket | 15,300 | MakeHuman | CC0 1.0 | ✅ 1K textures |

**Note:** All suits are static meshes for wardrobe authoring. Applying to characters requires:
- Skin weights for the target character rig
- Fit checks and adjustments
- Wardrobe entries in B&S Nomad's character system

#### Display Props (6 props)
| Prop | Type | Priority | Animation State |
|------|------|----------|-----------------|
| TTI Pit Viper | Pistol | ✅ **PRIMARY** | HoldPistol |
| Remington 1875 | Revolver | No | HoldPistol |
| TTI TR-1 Ultralight | Rifle | No | HoldRifle |
| Briefcase | Briefcase | No | HoldBriefcase |
| Throwing Knife | Knife | No | HoldKnife |
| Pencil | Pencil | No | HoldPencil |

**Priority Rule:** Only ONE pistol-shaped display prop is prioritized (TTI Pit Viper). This satisfies the requirement to "prioritize one pistol-shaped display prop" if multiple configurations are too complicated.

### 2. JWA_NPCDisplayProp (Prop Component)
Attach to display prop prefabs for proper NPC hand attachment.

#### Hand Attachment Hierarchy
```
DisplayProp_Root
├── HandAttachPoint (aligns with NPC hand bone)
└── Model (actual prop geometry)
```

## Integration with B&S Nomad SDK

### Supported NPC Systems (Investigation Needed)
The following B&S Nomad systems should be investigated for cosmetic integration:

1. **Creature/Character Wardrobe System**
   - Check if Nomad supports custom wardrobe entries
   - Look for `CreatureData`, `CharacterData`, or `WardrobeEntry` classes
   - Determine if static meshes can be assigned as cosmetic overlays

2. **NPC Equipment/Prop System**
   - Check for `CreatureEquipment`, `NPCEquipment`, or similar
   - Look for hand attachment point definitions
   - Verify animation state compatibility

3. **Animation System**
   - Standard states: `Idle`, `Walk`, `Run`, `HoldPistol`, `HoldRifle`, `HoldKnife`
   - Custom states may be needed for briefcase, pencil

### Implementation Steps (When SDK Available)

#### Phase 1: Wardrobe Integration
```csharp
// Example - pseudo-code for B&S integration
var wardrobe = creatureData.GetOrCreateWardrobe();
foreach (var entry in npcCosmeticSetup.wardrobeEntries)
{
    if (entry.requiresRigging)
    {
        // Requires skinning to character rig - not automated
        Debug.LogWarning($"[JWA] {entry.suitName} requires manual rigging");
        continue;
    }
    
    wardrobe.AddEntry(new WardrobeEntry
    {
        name = entry.displayName,
        mesh = entry.staticMeshPrefab.GetComponent<MeshFilter>().sharedMesh,
        albedo = entry.albedoTexture,
        normal = entry.normalTexture
    });
}
```

#### Phase 2: Display Prop Integration
```csharp
// Example - pseudo-code for B&S integration
var priorityProp = npcCosmeticSetup.GetPriorityDisplayProp();
if (priorityProp != null && priorityProp.propPrefab != null)
{
    // Spawn prop and attach to NPC hand
    var propInstance = Instantiate(priorityProp.propPrefab);
    var displayProp = propInstance.GetComponent<JWA_NPCDisplayProp>();
    displayProp.AttachToHand(npcRightHandBone);
    
    // Set animation state
    npcAnimator.Play(priorityProp.animationState);
}
```

## Quest Optimization Guidelines

### Triangle Budgets
- **Suits**: 15,000-20,000 triangles (static mesh, not skinned)
- **Display Props**: < 2,000 triangles each
- **Accessories**: < 500 triangles each

### Texture Limits
- Maximum 1024x1024 (1K) for all textures
- Android compression: ASTC 6x6
- Normal maps: sRGB = false

### Draw Call Optimization
- Share materials across suit variants
- Use texture atlases where possible
- Combine meshes for static display props

## Animation Compatibility

### Required Animation States
| State | Description | Props Compatible |
|-------|-------------|------------------|
| `Idle` | Standing idle | All |
| `Walk` | Walking | All |
| `HoldPistol` | One-handed pistol grip | Pistol, Revolver, Pencil |
| `HoldRifle` | Two-handed rifle grip | Rifle, Shotgun |
| `HoldKnife` | Reverse/forward knife grip | Knife |
| `HoldBriefcase` | Carrying briefcase | Briefcase |

### Validation Checklist
- [ ] Idle pose doesn't clip with suits
- [ ] Walk cycle maintains suit deformation
- [ ] HoldPistol aligns with pistol display prop
- [ ] HoldRifle aligns with rifle display prop
- [ ] Custom states for briefcase/pencil if needed

## File Locations

### Models
```
UnityProject/Assets/JohnWickArsenal/Models/
├── Suits/
│   ├── ClassicBlackSuit.obj + .mtl + _Albedo.png + _Bump.png
│   ├── BlackTacticalSuit.obj + .mtl
│   ├── FormalThreePieceSuit.obj + .mtl
│   ├── ArmoredCombatSuit.obj + .mtl
│   └── EliteHighProtectionSuit.obj + .mtl
└── Cosmetics/
    ├── Garments/
    │   ├── DoubleBreastedSuit/DoubleBreastedSuit.obj + .mtl + _Albedo.png + _Normal.png
    │   └── DinnerJacket/DinnerJacket.obj + .mtl + _Albedo.png + _Normal.png
    └── Props/
        └── Briefcase/Briefcase.fbx + Briefcase.fbm/_Albedo.png + _Normal.png
```

### Scripts
```
UnityProject/Assets/Scripts/JohnWickArsenal/NPCs/
├── JWA_NPCCosmeticSetup.cs
└── JWA_NPCDisplayProp.cs (nested class)
```

### Asset Credits
`UnityProject/Assets/JohnWickArsenal/Models/Cosmetics/ASSET_CREDITS.txt`

## Usage in Unity Editor

1. Add `JWA_NPCCosmeticSetup` component to a GameObject in your scene
2. Click "Initialize Default Wardrobe" in context menu
3. Click "Initialize Default Display Props" in context menu
4. Assign prefabs and textures in Inspector
5. Click "Validate Wardrobe" to check for missing assets
6. Use "Create Display Prop Template" to make new prop prefabs

## Next Steps

1. **Investigate B&S Nomad SDK NPC systems** - Check available APIs for wardrobe/equipment
2. **Create skinned suit variants** - If SDK supports it, rig suits to character skeleton
3. **Implement prop spawning** - Integrate with NPC spawn/behavior system
4. **Add animation overrides** - Custom animation states for unique props
5. **Test on Quest** - Validate performance with multiple NPCs

## License Compliance
All cosmetic assets are CC0 1.0 (public domain) from MakeHuman Community and OpenGameArt. See `ASSET_CREDITS.txt` for full attribution.

---

*This system is cosmetic only - it does not modify combat, targeting, damage, or any gameplay mechanics.*
