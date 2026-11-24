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
| Prop | Type | Priority | Animation State | Prefab Location |
|------|------|----------|-----------------|-----------------|
| TTI Pit Viper | Pistol | ✅ **PRIMARY** | HoldPistol | `Prefabs/DisplayProps/DisplayPistol_TTI_PitViper.prefab` |
| Remington 1875 | Revolver | No | HoldPistol | `Prefabs/DisplayProps/DisplayRevolver_Remington1875.prefab` |
| TTI TR-1 Ultralight | Rifle | No | HoldRifle | `Prefabs/DisplayProps/DisplayRifle_TTI_TR1.prefab` |
| Briefcase | Briefcase | No | HoldBriefcase | `Prefabs/DisplayProps/DisplayBriefcase.prefab` |
| Throwing Knife | Knife | No | HoldKnife | `Prefabs/DisplayProps/DisplayKnife_ThrowingKnife.prefab` |
| Pencil | Pencil | No | HoldPencil | `Prefabs/DisplayProps/DisplayPencil.prefab` |

**Priority Rule:** Only ONE pistol-shaped display prop is prioritized (TTI Pit Viper). This satisfies the requirement to "prioritize one pistol-shaped display prop" if multiple configurations are too complicated.

### 2. JWA_NPCDisplayProp (Prop Component)
Attach to display prop prefabs for proper NPC hand attachment.

#### Hand Attachment Hierarchy
```
DisplayProp_Root
├── HandAttachPoint (aligns with NPC hand bone)
└── Model (actual prop geometry)
```

#### Component Properties
- `handAttachPoint`: Transform that aligns with NPC hand bone (RightHand for B&S)
- `propType`: Enum (Pistol, Revolver, Rifle, Shotgun, Knife, Briefcase, Pencil, Custom)
- `isCosmeticOnly`: True - ensures no combat interaction
- `castShadows` / `receiveShadows`: Visual settings
- `renderQueue`: 2000 (standard geometry)

#### Attachment Method
```csharp
public void AttachToHand(Transform npcHandBone)
{
    // Match the hand bone's position/rotation
    transform.SetParent(npcHandBone, false);
    transform.position = npcHandBone.position;
    transform.rotation = npcHandBone.rotation;
    
    // Apply handAttachPoint offset
    transform.localPosition = -handAttachPoint.localPosition;
    transform.localRotation = Quaternion.Inverse(handAttachPoint.localRotation);
}
```

## B&S Nomad SDK NPC Cosmetic Systems - Research Findings

### Supported NPC Systems (Investigated)

Based on analysis of the B&S Nomad SDK (BladeAndSorcery.Gameplay, BladeAndSorcery.Data assemblies) and existing project code:

#### 1. Creature/Character System
- **Core Class**: `Creature` (from `BladeAndSorcery.Gameplay`)
- **Data Class**: `CreatureData` (from `BladeAndSorcery.Data`) - defines creature templates
- **Spawning**: `Creature.Instantiate(CreatureData, position, rotation)` - used in `JWA_NPCSystem.SpawnCreature()`
- **Factions**: `Creature.Faction` enum (Player, Enemy, Neutral, etc.)
- **Components**: 
  - `CreatureHealth` - health/damage handling
  - `BrainModule` - AI behavior
  - `Ragdoll` - physics body with bone access via `GetHand(Side)`, `GetBoneTransform(HumanBodyBones)`

#### 2. Equipment/Item System (Used for Combat Gear)
- **ItemData/WeaponData**: Define weapons and items in `Catalog`
- **Item/Weapon**: Runtime instances with `Grab()`, `Ungrab()` methods
- **Hand Attachment**: `Hand.Grab(Item)` - attaches items to creature hands
- **Catalog System**: `Catalog.GetCatalog<T>()` / `Catalog.RegisterData()` for registration
- **Equipment Slots**: No dedicated wardrobe system found in current SDK version

#### 3. Animation System
- **Animator**: Standard Unity Animator on Creature GameObject
- **Bone Access**: `Animator.GetBoneTransform(HumanBodyBones.*)` - used in `JWA_EquipmentSystem.GetSlotTransform()`
- **Known Animation States** (from B&S base game):
  - `Idle`, `Walk`, `Run`
  - `HoldPistol` (one-handed)
  - `HoldRifle` / `HoldTwoHanded` (two-handed)
  - `HoldKnife` / `HoldMelee`
  - Custom states vary by creature type

#### 4. Wardrobe/Cosmetic System - **NOT DIRECTLY SUPPORTED**
**Finding**: The current B&S Nomad SDK (as of v1.3.x) does **not** expose a public wardrobe/cosmetic overlay system for NPCs via the Modding API.

**Evidence from code analysis**:
- `JWA_EquipmentSystem` applies suits/equipment by spawning prefabs and parenting to bone transforms (EquipmentSlot mapping)
- No `WardrobeEntry`, `CharacterData.wardrobe`, or similar API found in `BladeAndSorcery.Data`
- `CreatureData` does not contain wardrobe fields in the public API
- Suits in JWA are implemented as `EquipmentItem` with `EquipmentType.Suit` and `EquipmentSlot.FullBody`, spawned and parented to creature root

**Workaround Used in Project**:
- Suits applied via `JWA_EquipmentSystem.EquipOnCreature()` → spawns suit prefab → parents to `creature.transform` (FullBody slot)
- Armor bonus applied via `CreatureHealth.damageMultiplier` modification
- This is **equipment attachment**, not true wardrobe/skinning

### Integration Approach for Nomad

#### Current Implementation (Equipment-Based)
The project uses an **equipment attachment system** rather than true wardrobe skinning:

```csharp
// From JWA_EquipmentSystem.SpawnEquipment()
var instance = Instantiate(prefab);
var slotTransform = GetSlotTransform(creature, item.slot); // Maps to HumanBodyBones
instance.transform.SetParent(slotTransform);
instance.transform.localPosition = item.localOffset;
instance.transform.localRotation = Quaternion.Euler(item.localRotation);
```

**Suits**: Parented to `creature.transform` (FullBody slot) - renders as overlay mesh
**Holsters/Pouches**: Parented to specific bones (Chest, Hip, Leg, etc.)

#### Limitations
1. **No Skin Weights**: Static suit meshes don't deform with character animation
2. **Clipping**: Suits may clip during complex animations
3. **No Material Override**: Can't swap character body materials
4. **Performance**: Each suit = extra draw call per NPC

#### True Wardrobe Integration (Requires SDK Support)
If B&S adds wardrobe API in future:
- Would need `CreatureData.wardrobeEntries` or similar
- Skinned mesh renderer with shared skeleton
- Material property blocks for texture variants

## Implementation Steps (Current SDK)

### Phase 1: Display Prop Integration (Ready Now)
```csharp
// In NPC spawn or cosmetic setup
var priorityProp = npcCosmeticSetup.GetPriorityDisplayProp();
if (priorityProp != null && priorityProp.propPrefab != null)
{
    var propInstance = Instantiate(priorityProp.propPrefab);
    var displayProp = propInstance.GetComponent<JWA_NPCDisplayProp>();
    
    // Get NPC right hand (B&S uses Side.Right for primary hand)
    var rightHand = npcCreature.ragdoll.GetHand(Side.Right);
    if (rightHand != null)
    {
        displayProp.AttachToHand(rightHand.transform);
    }
    
    // Trigger HoldPistol animation
    var animator = npcCreature.GetComponent<Animator>();
    animator?.CrossFade(priorityProp.animationState, 0.1f);
}
```

### Phase 2: Suit Application (Current Method)
```csharp
// Via JWA_EquipmentSystem - already implemented
equipSystem.EquipOnCreature(npcCreature, "jwa_suit_chapter1");
// Applies suit prefab as child of creature root
```

### Phase 3: Animation Compatibility Validation
```csharp
// Validate in editor
[ContextMenu("Validate Animation Compatibility")]
public void ValidateAnimations()
{
    var animator = GetComponent<Animator>();
    var requiredStates = new[] { "Idle", "Walk", "HoldPistol", "HoldRifle", "HoldKnife" };
    
    foreach (var state in requiredStates)
    {
        if (!HasAnimationState(animator, state))
            Debug.LogWarning($"[JWA Cosmetic] Missing animation state: {state}");
    }
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
- **Current prefabs**: Each display prop = 1 draw call (Model only)

### Display Prop Triangle Counts (Target)
| Prop | Target Tris | Notes |
|------|-------------|-------|
| TTI Pit Viper | ~1,500 | From existing weapon model (LOD0) |
| Remington 1875 | ~1,200 | From existing weapon model |
| TTI TR-1 Ultralight | ~1,800 | From existing weapon model |
| Briefcase | 712 | Already optimized (Cosmetics/Props/Briefcase.fbx) |
| Throwing Knife | ~300 | From existing weapon model |
| Pencil | ~100 | From existing weapon model |

## Animation Compatibility

### Required Animation States
| State | Description | Props Compatible | B&S Availability |
|-------|-------------|------------------|------------------|
| `Idle` | Standing idle | All | ✅ Standard |
| `Walk` | Walking | All | ✅ Standard |
| `Run` | Running | All | ✅ Standard |
| `HoldPistol` | One-handed pistol grip | Pistol, Revolver, Pencil | ✅ Standard |
| `HoldRifle` | Two-handed rifle grip | Rifle, Shotgun | ✅ Standard |
| `HoldKnife` | Reverse/forward knife grip | Knife | ✅ Standard |
| `HoldBriefcase` | Carrying briefcase | Briefcase | ❌ Custom needed |
| `HoldPencil` | Writing/holding pose | Pencil | ❌ Custom needed |

### Hand Bone Alignment (Critical for Display Props)
B&S Nomad uses standard Unity Humanoid rig:
- **Right Hand**: `HumanBodyBones.RightHand` (primary weapon hand)
- **Left Hand**: `HumanBodyBones.LeftHand` (off-hand)
- **HandAttachPoint** in prefabs positioned at local origin (0,0,0) - aligns to hand bone
- **Model offset** tuned per-prop for natural grip

#### Prefab Model Offsets (from prefab files)
| Prop | Model Local Position | Notes |
|------|---------------------|-------|
| TTI Pit Viper | (0.015, -0.045, -0.02) | Grip aligned to hand center |
| Remington 1875 | (0.015, -0.05, -0.015) | Revolver frame offset |
| TTI TR-1 | (0.05, -0.08, -0.03) | Two-handed, forward grip |
| Briefcase | (0.05, -0.12, 0.0) | Handle at hand center |
| Throwing Knife | (0.01, -0.03, -0.01) | Blade forward |
| Pencil | (0.005, -0.02, -0.005) | Tip forward |

### Validation Checklist
- [ ] Idle pose doesn't clip with suits (test all 7 suits)
- [ ] Walk cycle maintains suit position (no sliding)
- [ ] HoldPistol aligns with pistol display prop (TTI Pit Viper primary)
- [ ] HoldRifle aligns with rifle display prop (TTI TR-1)
- [ ] HoldKnife aligns with knife display prop
- [ ] Custom states for briefcase/pencil if needed (use HoldPistol as fallback)
- [ ] Test on Quest with 5+ NPCs simultaneously

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

### Display Prop Prefabs (NEW)
```
UnityProject/Assets/JohnWickArsenal/Prefabs/DisplayProps/
├── DisplayPistol_TTI_PitViper.prefab       # PRIORITY
├── DisplayRevolver_Remington1875.prefab
├── DisplayRifle_TTI_TR1.prefab
├── DisplayBriefcase.prefab
├── DisplayKnife_ThrowingKnife.prefab
└── DisplayPencil.prefab
```

### Scripts
```
UnityProject/Assets/Scripts/JohnWickArsenal/NPCs/
├── JWA_NPCCosmeticSetup.cs
└── JWA_NPCDisplayProp.cs (nested class in JWA_NPCCosmeticSetup.cs)

UnityProject/Assets/Scripts/JohnWickArsenal/Equipment/
└── JWA_EquipmentSystem.cs  # Handles suit/equipment attachment to NPCs

UnityProject/Assets/Scripts/JohnWickArsenal/NPCs/
└── JWA_NPCSystem.cs        # NPC spawning with loadouts
```

### Asset Credits
`UnityProject/Assets/JohnWickArsenal/Models/Cosmetics/ASSET_CREDITS.txt`

## Usage in Unity Editor

1. Add `JWA_NPCCosmeticSetup` component to a GameObject in your scene
2. Click "Initialize Default Wardrobe" in context menu
3. Click "Initialize Default Display Props" in context menu
4. Assign prefabs and textures in Inspector:
   - Wardrobe: Assign suit prefabs from `Models/Suits/` and `Models/Cosmetics/Garments/`
   - Display Props: Assign prefabs from `Prefabs/DisplayProps/`
5. Click "Validate Wardrobe" to check for missing assets
6. Use "Create Display Prop Template" to make new prop prefabs

### Assigning Display Prop Prefabs
In Inspector for `JWA_NPCCosmeticSetup`:
- Expand `Display Props` array (6 entries after initialization)
- For each entry, drag the corresponding prefab from `Prefabs/DisplayProps/` to `Prop Prefab` field
- Verify `Hand Attach Point` is assigned (child "HandAttachPoint" of prefab root)
- Ensure `Is Priority Prop` is checked ONLY for TTI Pit Viper (index 0)

## Next Steps

1. **Assign prefabs in Inspector** - Link all 6 display prop prefabs to JWA_NPCCosmeticSetup
2. **Assign suit prefabs** - Create prefabs from suit models and assign to wardrobe entries
3. **Test attachment** - Spawn NPC, call `AttachToHand()` on priority prop, verify HoldPistol animation
4. **Validate on Quest** - Build and test with multiple NPCs for performance
5. **Monitor B&S SDK updates** - Check for wardrobe API in future Nomad versions
6. **Consider skinned variants** - If SDK adds wardrobe, create skinned suit meshes

## License Compliance
All cosmetic assets are CC0 1.0 (public domain) from MakeHuman Community and OpenGameArt. See `ASSET_CREDITS.txt` for full attribution.

---

*This system is cosmetic only - it does not modify combat, targeting, damage, or any gameplay mechanics.*