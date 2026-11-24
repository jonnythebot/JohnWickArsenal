# John Wick Arsenal - Lightweight Build Configuration

## Overview
This branch (`lightweight`) contains a minimal cosmetic configuration for Blade & Sorcery Nomad on Meta Quest. It preserves the core mod functionality while reducing asset count for faster iteration and smaller build size.

## Branch Information
- **Base**: `develop/unity-cosmetics-final` (commit `d2735ae`)
- **Created**: 2026-11-23
- **Purpose**: Minimal cosmetic build for testing core pipeline

## Included Assets

### Suits (1/5)
| Suit | Status | Files |
|------|--------|-------|
| ClassicBlackSuit | ✅ **Included** | `.obj`, `.mtl`, `_Albedo.png`, `_Bump.png` |
| BlackTacticalSuit | ✅ Preserved (procedural) | `.obj`, `.mtl` |
| FormalThreePieceSuit | ✅ Preserved (procedural) | `.obj`, `.mtl` |
| ArmoredCombatSuit | ✅ Preserved (procedural) | `.obj`, `.mtl` |
| EliteHighProtectionSuit | ✅ Preserved (procedural) | `.obj`, `.mtl` |

### Display Props (1/6)
| Prop | Type | Status | Prefab |
|------|------|--------|--------|
| TTI Pit Viper | Pistol | ✅ **Included** | `Prefabs/DisplayProps/DisplayPistol_TTI_PitViper.prefab` |
| Remington 1875 | Revolver | ⏳ Preserved for later | Model exists |
| TTI TR-1 Ultralight | Rifle | ⏳ Preserved for later | Model exists |
| Briefcase | Prop | ❌ Removed | — |
| Throwing Knife | Knife | ⏳ Preserved for later | Model exists |
| Pencil | Improvised | ⏳ Preserved for later | Model exists |

### Equipment (11/11)
All procedural equipment models preserved (holsters, pouches, carriers).

### Weapons (10/10)
All weapon models preserved (OBJ + GLB where available).

## Removed from Lightweight Build
| Asset | Reason |
|-------|--------|
| DoubleBreastedSuit | Extra cosmetic garment (CC0 MakeHuman) |
| DinnerJacket | Extra cosmetic garment (CC0 MakeHuman) |
| Briefcase | Extra prop (CC0 OpenGameArt) |
| 5 Display Props | Reduced to single priority pistol |
| 36 Accessory Variants | Procedural generator removed |
| NPC Cosmetic System | Not needed for lightweight test |
| Accessory Scripts | `JWA_AccessoryBase`, `JWA_AccessoryCatalog`, `JWA_AccessoryManager` |
| Accessory Prefab Generator | `JWA_AccessoryPrefabGenerator.cs` |

## Planned Future Display Models (8 Total)

The lightweight build is designed to eventually include these 8 nonfunctional cosmetic display models:

| # | Model | Source Weapon | Asset Status | Notes |
|---|-------|---------------|--------------|-------|
| 1 | **TTI Pit Viper** | TTI Pit Viper (9mm Major) | ✅ **Exists** | GLB + OBJ in `Models/Pistols/` |
| 2 | **Glock** | Glock 34 Gen 5 MOS | ✅ **Exists** | GLB + OBJ in `Models/Pistols/` |
| 3 | **AR-15** | — | ❌ Missing | No AR-15 model in project |
| 4 | **SKS** | — | ❌ Missing | No SKS model in project |
| 5 | **AWP Sniper Rifle** | — | ❌ Missing | No AWP model in project |
| 6 | **MP5** | — | ❌ Missing | No MP5 model in project |
| 7 | **Remington 870** | Benelli M4 (similar) | ⚠️ Similar exists | Benelli M4 GLB in `Models/Shotguns/` |
| 8 | **SR-47** | — | ❌ Missing | No SR-47 model in project |

**Verified Existing (2/8):**
- TTI Pit Viper → `Models/Pistols/TTI_PitViper.glb/.obj`
- Glock → `Models/Pistols/Glock_34.glb/.obj`

**Missing (6/8):**
- AR-15, SKS, AWP, MP5, Remington 870 (distinct), SR-47

## First Test Configuration
Minimal viable test setup:
1. **ClassicBlackSuit** — Full 1K textured suit
2. **TTI Pit Viper Display Prop** — Single pistol display prop with `HoldPistol` animation

## Size Comparison

| Metric | Full Build (develop/unity-cosmetics-final) | Lightweight Build |
|--------|--------------------------------------------|-------------------|
| Cosmetic Models | 9 (5 suits + 2 garments + 1 briefcase + 1) | 1 (ClassicBlackSuit) |
| Display Props | 6 | 1 |
| Accessory Variants | 36 | 0 |
| NPC Cosmetic System | Yes | No |
| Estimated Bundle Size | ~15-20 MB | ~2-5 MB |
| Build Time | ~5-10 min | ~1-3 min |

## Build Instructions
```bash
# Validate sources
python3 Tools/validate_android_sources.py

# Check Unity installation
python3 Tools/build_local.py --check

# Build lightweight cosmetic bundle
python3 Tools/build_local.py --output BuildOutput
```

## Restoring Full Build
To restore the full cosmetic build:
```bash
git checkout develop/unity-cosmetics-final
# Or merge specific files from that branch
```

## License Compliance
All included assets maintain their original licenses:
- ClassicBlackSuit: CC0 1.0 (MakeHuman Community)
- Weapon models: CC0 (petroulacl FPS Kit) / CC BY 4.0 (Vinish.dev)
- Procedural assets: Original project work
