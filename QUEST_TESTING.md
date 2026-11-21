# John Wick Arsenal - Meta Quest Testing Plan

**Version:** 1.0  
**Date:** 2026-11-19  
**Target Platform:** Meta Quest 2/3/Pro (Android/ARM64)  
**B&S Version:** Nomad 1.3.1  
**Unity Version:** 2022.3 LTS (URP)

---

## Testing Overview

This document outlines the complete manual testing procedure for the John Wick Arsenal mod on Meta Quest devices. Tests are organized by category and priority.

### Device Coverage Matrix

| Device | SoC | RAM | GPU | Priority |
|--------|-----|-----|-----|----------|
| Quest 2 | Snapdragon XR2 | 6GB | Adreno 650 | **Critical** (baseline) |
| Quest 3 | Snapdragon XR2 Gen 2 | 8GB | Adreno 740 | **High** |
| Quest Pro | Snapdragon XR2+ | 12GB | Adreno 650 | **Medium** |
| Quest 3S | Snapdragon XR2 Gen 2 | 8GB | Adreno 740 | **Medium** |

### Test Environment Requirements
- Clean B&S Nomad 1.3.1 install (no other mods)
- Developer mode enabled
- ADB access for logcat capture
- Oculus Developer Hub for performance metrics

---

## Phase 1: Installation & Launch Tests

### 1.1 Mod Installation
| Test ID | Description | Expected Result | Pass/Fail |
|---------|-------------|-----------------|-----------|
| INST-01 | Install mod ZIP via SideQuest/ADB | Mod appears in B&S mod list | ☐ |
| INST-02 | Launch B&S with mod enabled | No crash on startup | ☐ |
| INST-03 | Verify mod.json loads correctly | Mod name "John Wick Arsenal" v1.0.0 shown | ☐ |
| INST-04 | Verify icon.png displays | Custom icon in mod menu | ☐ |
| INST-05 | Check mod dependencies | No missing assembly errors | ☐ |

### 1.2 Main Menu Integration
| Test ID | Description | Expected Result | Pass/Fail |
|---------|-------------|-----------------|-----------|
| MENU-01 | "John Wick Arsenal" category in spawn menu | Category visible with custom icon | ☐ |
| MENU-02 | All 10 subcategories present | Pistols, Shotguns, Revolvers, Rifles, Throwables, Suits, Holsters, Pouches, Throwable Carriers, NPCs, Loadouts | ☐ |
| MENU-03 | Featured items show correctly | Pit Viper, Pencil, Benelli M4, Chapter 4 Suit, High Table Elite | ☐ |
| MENU-04 | Loadouts subcategory works | 5 loadouts selectable | ☐ |

---

## Phase 2: Weapon Spawning & Basic Functionality

### 2.1 Pistol Tests (4 weapons)
| Test ID | Weapon | Test | Expected Result | Pass/Fail |
|---------|--------|------|-----------------|-----------|
| W-P30L-01 | H&K P30L | Spawn from menu | Weapon appears in hand, correct model | ☐ |
| W-P30L-02 | H&K P30L | Fire (semi-auto) | Muzzle flash, sound, casing ejection, recoil | ☐ |
| W-P30L-03 | H&K P30L | Tactical reload | Magazine ejects, new mag inserts, slide release | ☐ |
| W-P30L-04 | H&K P30L | Empty reload | Slide locks back, mag swap, slide release | ☐ |
| W-P30L-05 | H&K P30L | Compensator effect | Reduced muzzle rise vs non-compensated | ☐ |
| W-P30L-06 | H&K P30L | Inspect animation | Slide rack, check chamber, press check | ☐ |
| W-G34-01 | Glock 34 | Spawn + RMR dot visible | Red dot sight active when held | ☐ |
| W-G34-02 | Glock 34 | Fire suppressed | Quieter report, suppressed muzzle flash | ☐ |
| W-G34-03 | Glock 34 | Trigger feel | Light crisp break (~3.5lb) | ☐ |
| W-STI-01 | STI 2011 | Spawn + gold accents | Gold TiN barrel, controls visible | ☐ |
| W-STI-02 | STI 2011 | Compensator gas | Side gas ports vent on fire | ☐ |
| W-STI-03 | STI 2011 | Race gun handling | Fast draw, minimal recoil | ☐ |
| W-PV-01 | TTI Pit Viper | Spawn + Viper Green | Custom cerakote, gold parts | ☐ |
| W-PV-02 | TTI Pit Viper | RMRcc mounted | Compact red dot direct to slide | ☐ |
| W-PV-03 | TTI Pit Viper | 9mm Major power | Sharper crack, higher velocity | ☐ |

### 2.2 Shotgun Tests (2 weapons)
| Test ID | Weapon | Test | Expected Result | Pass/Fail |
|---------|--------|------|-----------------|-----------|
| W-M4-01 | Benelli M4 | Spawn + ARGO | Gas port particles on fire | ☐ |
| W-M4-02 | Benelli M4 | Pump action | Manual pump cycles shell | ☐ |
| W-M4-03 | Benelli M4 | Shell carrier reload | 6 shells visible on side saddle | ☐ |
| W-M4-04 | Benelli M4 | Ghost ring sights | Aperture + post aligned | ☐ |
| W-M4-05 | Benelli M4 | Collapsible stock | Stock extends/retracts | ☐ |
| W-KSG-01 | Kel-Tec KSG | Spawn + dual tubes | Two magazine tubes visible | ☐ |
| W-KSG-02 | Kel-Tec KSG | Tube switch (T key) | Switches between tube A/B | ☐ |
| W-KSG-03 | Kel-Tec KSG | Forward ejection | Shells eject forward/down | ☐ |
| W-KSG-04 | Kel-Tec KSG | Bullpup ergonomics | Magazine behind grip | ☐ |
| W-KSG-05 | Kel-Tec KSG | Flip-up sights | Front/rear sights deploy | ☐ |

### 2.3 Revolver Test (1 weapon)
| Test ID | Weapon | Test | Expected Result | Pass/Fail |
|---------|--------|------|-----------------|-----------|
| W-R1875-01 | Remington 1875 | Spawn + case hardened | Frame color correct | ☐ |
| W-R1875-02 | Remington 1875 | Single action | Hammer must be cocked | ☐ |
| W-R1875-03 | Remington 1875 | Loading gate | Opens for reload | ☐ |
| W-R1875-04 | Remington 1875 | Ejector rod | Pushes out spent casings | ☐ |
| W-R1875-05 | Remington 1875 | Cylinder rotation | 60° per shot | ☐ |
| W-R1875-06 | Remington 1875 | Black powder smoke | Smoke on fire | ☐ |

### 2.4 Rifle Test (1 weapon)
| Test ID | Weapon | Test | Expected Result | Pass/Fail |
|---------|--------|------|-----------------|-----------|
| W-TR1-01 | TTI TR-1 | Spawn + skeletonized | Receiver cutouts visible | ☐ |
| W-TR1-02 | TTI TR-1 | Carbon fiber handguard | CF texture visible | ☐ |
| W-TR1-03 | TTI TR-1 | MRO red dot | Trijicon MRO model | ☐ |
| W-TR1-04 | TTI TR-1 | Gas block adjust (G) | 4 positions: Supp/Norm/High/Adv | ☐ |
| W-TR1-05 | TTI TR-1 | Lightweight BCG | Reduced reciprocating mass feel | ☐ |
| W-TR1-06 | TTI TR-1 | Charging handle | Ambidextrous operation | ☐ |

### 2.5 Throwable Tests (2 items)
| Test ID | Item | Test | Expected Result | Pass/Fail |
|---------|------|------|-----------------|-----------|
| W-TK-01 | Throwing Knife | Spawn + balance | Spins stable in flight | ☐ |
| W-TK-02 | Throwing Knife | Headshot kill | 3x damage, instant kill | ☐ |
| W-TK-03 | Throwing Knife | Stick & retrieve | Sticks in target, grab to retrieve | ☐ |
| W-TK-04 | Throwing Knife | Paracord wrap | Handle texture visible | ☐ |
| W-PEN-01 | Pencil | Spawn + quote | "Yeah, I'm thinking I'm back" plays | ☐ |
| W-PEN-02 | Pencil | Headshot instant kill | One-hit kill on head/neck | ☐ |
| W-PEN-03 | Pencil | Breaks after 3 kills | Shatters, pieces fly | ☐ |
| W-PEN-04 | Pencil | Graphite dust | Particles on impact | ☐ |

---

## Phase 3: Equipment System Tests

### 3.1 Suit Tests (5 suits)
| Test ID | Suit | Test | Expected Result | Pass/Fail |
|---------|------|------|-----------------|-----------|
| EQ-S1-01 | Ch.1 Tactical | Spawn + wear | Suit visible on player body | ☐ |
| EQ-S1-02 | Ch.1 Tactical | Armor 15% DR | Takes 15% less damage | ☐ |
| EQ-S2-01 | Ch.2 Bulletproof | Spawn + wear | Grey three-piece visible | ☐ |
| EQ-S2-02 | Ch.2 Bulletproof | Armor 25% DR | Takes 25% less damage | ☐ |
| EQ-S3-01 | Ch.3 Armored | Spawn + wear | Black tactical with plates | ☐ |
| EQ-S3-02 | Ch.3 Armored | Armor 35% DR | Takes 35% less damage | ☐ |
| EQ-S4-01 | Ch.4 Parisian | Spawn + wear | Navy bespoke visible | ☐ |
| EQ-S4-02 | Ch.4 Parisian | Armor 30% DR | Takes 30% less damage | ☐ |
| EQ-S5-01 | Contractor | Spawn + wear | Full plate carrier under suit | ☐ |
| EQ-S5-02 | Contractor | Armor 45% DR | Takes 45% less damage (max) | ☐ |
| EQ-S-03 | All Suits | Movement penalty | Speed reduced by weight | ☐ |
| EQ-S-04 | All Suits | Concealment | NPCs detect slower at distance | ☐ |

### 3.2 Holster Tests (4 holsters)
| Test ID | Holster | Test | Expected Result | Pass/Fail |
|---------|---------|------|-----------------|-----------|
| EQ-H-01 | Shoulder (Double) | Wear + draw 2 pistols | Both pistols accessible | ☐ |
| EQ-H-02 | Hip (TTI Race) | Fast draw | Fastest draw speed | ☐ |
| EQ-H-03 | Thigh (Tactical) | Secure retention | Pistol stays during movement | ☐ |
| EQ-H-04 | Ankle (Concealed) | Slow draw | Slowest but hidden | ☐ |
| EQ-H-05 | All | Compatibility | Only fits listed weapons | ☐ |

### 3.3 Pouch Tests (4 pouches)
| Test ID | Pouch | Test | Expected Result | Pass/Fail |
|---------|-------|------|-----------------|-----------|
| EQ-P-01 | Chest Rig | Wear + 6 mags | 6 mag slots visible | ☐ |
| EQ-P-02 | Belt Pouches | Wear + 4 mags | 4 mag slots on belt | ☐ |
| EQ-P-03 | Benelli Shell Carrier | Mount on M4 | 6 shells on receiver side | ☐ |
| EQ-P-04 | KSG Shell Carrier | Integrated tubes | Matches KSG dual tube | ☐ |

### 3.4 Throwable Carrier Tests (2 carriers)
| Test ID | Carrier | Test | Expected Result | Pass/Fail |
|---------|---------|------|-----------------|-----------|
| EQ-TC-01 | Knife Bandolier | Wear + 8 knives | 8 knife loops on chest | ☐ |
| EQ-TC-02 | Pencil Case | Wear + 3 pencils | Innocent pencil case on belt | ☐ |

---

## Phase 4: NPC System Tests

### 4.1 NPC Preset Spawning
| Test ID | NPC | Test | Expected Result | Pass/Fail |
|---------|-----|------|-----------------|-----------|
| NPC-01 | Assassin | Spawn + equipment | Suit + shoulder holster + P30L + Glock | ☐ |
| NPC-02 | Assassin | Behavior | Flanks, dual wields, high accuracy | ☐ |
| NPC-03 | Heavy Gunner | Spawn + equipment | Contractor suit + M4/KSG + chest rig | ☐ |
| NPC-04 | Heavy Gunner | Behavior | Holds ground, suppresses, slow advance | ☐ |
| NPC-05 | Tactical | Spawn + equipment | Ch.3 suit + TR-1 + Glock + chest rig | ☐ |
| NPC-06 | Tactical | Behavior | Uses cover, switches weapons by range | ☐ |
| NPC-07 | Cleaner | Spawn + equipment | Ch.4 suit + Pit Viper + knife bandolier | ☐ |
| NPC-08 | Cleaner | Behavior | Stealth, silent kills, avoids alerts | ☐ |
| NPC-09 | Enforcer | Spawn + equipment | Ch.1 suit + M4 + 1875 + thigh holster | ☐ |
| NPC-10 | Enforcer | Behavior | Rushes, melee, close range shotgun | ☐ |
| NPC-11 | High Table Elite | Spawn + equipment | Contractor suit + full loadout | ☐ |
| NPC-12 | High Table Elite | Boss behavior | Phases, calls reinforcements, adaptive | ☐ |

### 4.2 NPC Combat Tests
| Test ID | Test | Expected Result | Pass/Fail |
|---------|------|-----------------|-----------|
| NPC-C-01 | NPC takes damage | Armor reduction applies | ☐ |
| NPC-C-02 | NPC dies | Equipment despawns cleanly | ☐ |
| NPC-C-03 | NPC drops weapon | Weapon pickupable by player | ☐ |
| NPC-C-04 | Multiple NPCs | No performance drop < 15 NPCs | ☐ |
| NPC-C-05 | NPC weapon switching | Tactical switches TR-1 ↔ Glock | ☐ |

---

## Phase 5: Loadout System Tests

### 5.1 Loadout Application
| Test ID | Loadout | Test | Expected Result | Pass/Fail |
|---------|---------|------|-----------------|-----------|
| LO-01 | Chapter 1 | Apply to player | Suit + holster + pouches + P30L + M4 | ☐ |
| LO-02 | Chapter 2 | Apply to player | Suit + holster + pouches + G34 + Pit Viper | ☐ |
| LO-03 | Chapter 3 | Apply to player | Suit + holster + pouches + STI + TR-1 | ☐ |
| LO-04 | Chapter 4 | Apply to player | Suit + holster + pouches + Pit Viper + TR-1 | ☐ |
| LO-05 | Baba Yaga | Apply to player | Contractor suit + ALL weapons + max ammo | ☐ |

### 5.2 Loadout Persistence
| Test ID | Test | Expected Result | Pass/Fail |
|---------|------|-----------------|-----------|
| LO-P-01 | Save game with loadout | Loadout persists on reload | ☐ |
| LO-P-02 | Switch loadouts | Clean swap, no duplicate items | ☐ |
| LO-P-03 | Unlock requirements | Chapters unlock progressively | ☐ |

---

## Phase 6: Performance & Stability (QUEST CRITICAL)

### 6.1 Frame Rate Tests
| Test ID | Scenario | Target FPS | Measured FPS | Pass/Fail |
|---------|----------|------------|--------------|-----------|
| PERF-01 | Empty scene, mod loaded | 72/90 | | ☐ |
| PERF-02 | 1 weapon spawned, idle | 72/90 | | ☐ |
| PERF-03 | Firing P30L (auto) | 72/90 | | ☐ |
| PERF-04 | Firing Benelli M4 | 72/90 | | ☐ |
| PERF-05 | Firing TR-1 (full auto) | 72/90 | | ☐ |
| PERF-06 | 5 NPCs active combat | 72/90 | | ☐ |
| PERF-07 | 10 NPCs active combat | 72/90 | | ☐ |
| PERF-08 | 15 NPCs (max) | 60+ | | ☐ |
| PERF-09 | All VFX active (explosions, fire) | 72/90 | | ☐ |
| PERF-10 | Baba Yaga loadout equipped | 72/90 | | ☐ |

### 6.2 Memory Tests
| Test ID | Metric | Target | Measured | Pass/Fail |
|---------|--------|--------|----------|-----------|
| MEM-01 | App memory (empty) | < 1.5 GB | | ☐ |
| MEM-02 | App memory (full loadout) | < 2.0 GB | | ☐ |
| MEM-03 | Texture memory | < 512 MB | | ☐ |
| MEM-04 | Mesh memory | < 256 MB | | ☐ |
| MEM-05 | Audio memory | < 64 MB | | ☐ |
| MEM-06 | No memory leaks (10 min play) | Stable | | ☐ |

### 6.3 Thermal Tests (Quest 2/3)
| Test ID | Duration | Max Temp | Throttling? | Pass/Fail |
|---------|----------|----------|-------------|-----------|
| THERM-01 | 15 min combat | < 45°C | No | ☐ |
| THERM-02 | 30 min combat | < 48°C | No | ☐ |
| THERM-03 | 60 min session | < 50°C | Minimal | ☐ |

### 6.4 Stability Tests
| Test ID | Test | Expected Result | Pass/Fail |
|---------|------|-----------------|-----------|
| STAB-01 | 1 hour continuous play | No crash | ☐ |
| STAB-02 | Rapid weapon switching | No crash | ☐ |
| STAB-03 | Spawn/despawn 100x weapons | No crash | ☐ |
| STAB-04 | Load/unload mod 10x | No crash | ☐ |
| STAB-05 | Quest sleep/wake with mod | No crash | ☐ |

---

## Phase 7: Audio & Visual Quality

### 7.1 Audio Tests
| Test ID | Test | Expected Result | Pass/Fail |
|---------|------|-----------------|-----------|
| AUD-01 | All weapon fire sounds | Distinct, high quality | ☐ |
| AUD-02 | Suppressed variants | Noticeably quieter | ☐ |
| AUD-03 | Reload sounds | Mag out/in, slide rack, bolt | ☐ |
| AUD-04 | Spatial audio | Directional, distance attenuation | ☐ |
| AUD-05 | No audio clipping | Clean at max volume | ☐ |
| AUD-06 | Dynamic range compression | Consistent levels | ☐ |

### 7.2 Visual Tests
| Test ID | Test | Expected Result | Pass/Fail |
|---------|------|-----------------|-----------|
| VIS-01 | Weapon models | High detail, correct proportions | ☐ |
| VIS-02 | Materials/textures | PBR correct, no pink/missing | ☐ |
| VIS-03 | Muzzle flashes | Distinct per caliber | ☐ |
| VIS-04 | Shell casings | Correct caliber, eject properly | ☐ |
| VIS-05 | Compensator gas | Visible on comp weapons | ☐ |
| VIS-06 | Blood/impact effects | Satisfying, not excessive | ☐ |
| VIS-07 | Suit armor visual | Subtle Kevlar/plate indication | ☐ |
| VIS-08 | No Z-fighting | Clean at all distances | ☐ |

---

## Phase 8: Compatibility & Edge Cases

### 8.1 B&S Compatibility
| Test ID | Test | Expected Result | Pass/Fail |
|---------|------|-----------------|-----------|
| COMP-01 | B&S 1.3.1 vanilla | Works without other mods | ☐ |
| COMP-02 | With popular mods (Outer Rim, etc.) | No conflicts | ☐ |
| COMP-03 | Mod enable/disable in menu | Clean toggle | ☐ |
| COMP-04 | Update B&S to newer version | Graceful degradation | ☐ |

### 8.2 Edge Cases
| Test ID | Test | Expected Result | Pass/Fail |
|---------|------|-----------------|-----------|
| EDGE-01 | Drop weapon in water | Retrievable, no errors | ☐ |
| EDGE-02 | Throw knife at physics object | Sticks, retrievable | ☐ |
| EDGE-03 | Pencil throw at wall | Sticks or breaks | ☐ |
| EDGE-04 | Dual wield pistols | Both fire independently | ☐ |
| EDGE-05 | Switch gas block while firing | Changes take effect next shot | ☐ |
| EDGE-06 | NPC picks up player weapon | Uses correctly | ☐ |
| EDGE-07 | Player picks up NPC weapon | Works correctly | ☐ |

---

## Phase 9: Accessibility Tests

| Test ID | Feature | Test | Pass/Fail |
|---------|---------|------|-----------|
| ACC-01 | Reduce screen shake | Setting works | ☐ |
| ACC-02 | Simplified reloading | Auto-reload option works | ☐ |
| ACC-03 | Throwing assist | Auto-aim for knives works | ☐ |
| ACC-04 | Highlight interactables | Equipment glows | ☐ |
| ACC-05 | Left-handed mode | Controls mirror correctly | ☐ |

---

## Test Execution Checklist

### Pre-Test Setup
- [ ] Fresh B&S Nomad 1.3.1 install
- [ ] Developer mode enabled on Quest
- [ ] ADB connected, logcat running
- [ ] Oculus Developer Hub open
- [ ] Mod ZIP built and ready

### Test Run Procedure
1. [ ] Install mod via ADB: `adb install -r JohnWickArsenal_v1.0.0.apk` (or SideQuest)
2. [ ] Launch B&S, enable mod in menu
3. [ ] Run Phase 1 (Installation) - **BLOCKER if any fail**
4. [ ] Run Phase 2 (Weapons) - Test all 12 items
5. [ ] Run Phase 3 (Equipment) - Test all 15 equipment items
6. [ ] Run Phase 4 (NPCs) - Test all 6 presets
7. [ ] Run Phase 5 (Loadouts) - Test all 5 loadouts
8. [ ] Run Phase 6 (Performance) - **CRITICAL for Quest**
9. [ ] Run Phase 7 (Audio/Visual)
10. [ ] Run Phase 8 (Compatibility)
11. [ ] Run Phase 9 (Accessibility)

### Post-Test
- [ ] Collect logcat: `adb logcat -d > jwa_test_log.txt`
- [ ] Capture performance screenshots from ODH
- [ ] Document any failures with repro steps
- [ ] Update IMPLEMENTATION_STATUS.md

---

## Pass/Fail Criteria

### Release Blockers (Must Fix)
- Any crash in Phases 1-5
- FPS < 60 in PERF-01 through PERF-05
- Memory > 2.0 GB in MEM-02
- Thermal throttling in THERM-01
- Missing weapon/equipment/NPC from spawn menu

### Major Issues (Should Fix)
- FPS < 72 in PERF-06 through PERF-08
- Audio clipping or missing sounds
- Visual artifacts (pink materials, Z-fighting)
- NPC behavior clearly broken

### Minor Issues (Nice to Fix)
- FPS < 90 on Quest 3/Pro
- Minor animation hitches
- Missing inspect animations
- Loadout unlock progression not working

---

## Sign-Off

| Role | Name | Date | Signature |
|------|------|------|-----------|
| Lead Developer | | | |
| QA Tester | | | |
| Quest Optimization | | | |

**Overall Result:** ☐ PASS ☐ CONDITIONAL PASS ☐ FAIL

---

*This testing plan should be executed on each target Quest device before release. Document all results with screenshots and logcat captures.*