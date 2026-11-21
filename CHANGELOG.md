# John Wick Arsenal - Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] - 2026-11-19 (Planned Release)

### Added
- **10 John Wick Weapons** with custom behaviors:
  - H&K P30L (Chapter 1) - Compensator, TruGlo night sights
  - Glock 34 Gen 5 MOS (Chapter 2) - RMR cut, minus connector
  - STI 2011 Combat Master (Chapter 3/4) - Integrated comp, gold TiN, race trigger
  - TTI Pit Viper (Chapter 4) - 9mm Major, RMRcc, Viper Green cerakote
  - Benelli M4 Super 90 (Chapter 1) - ARGO gas system, shell carrier
  - Kel-Tec KSG (Chapter 3) - Dual tube selector, bullpup, forward ejection
  - Remington 1875 (Chapter 2) - Single action, loading gate, ejector rod
  - TTI TR-1 Ultralight (Chapter 4) - Skeletonized, carbon fiber, adjustable gas, MRO
  - Throwing Knives (Chapter 1/3) - Balanced, retrievable, silent kills
  - Pencil (Chapter 2/3) - Instant kill head/neck, breaks after 3 kills, quote Easter egg

- **Equipment System** (11 wearable items):
  - 4 Holsters: Shoulder (Double), Hip (TTI Race), Thigh (Tactical), Ankle (Concealed)
  - 4 Magazine Pouches: Chest Rig (6), Belt (4), Benelli Shell Carrier (6), KSG Carrier (14)
  - 2 Throwable Carriers: Knife Bandolier (8), Pencil Case (3)
  - Slot-based equipping with 14 equipment slots
  - Armor bonuses, concealment, weight penalties

- **5 Bullet-Resistant Suits** (film-accurate):
  - Chapter 1 Tactical Suit (15% DR)
  - Chapter 2 Bulletproof Suit (25% DR)
  - Chapter 3 Armored Suit (35% DR)
  - Chapter 4 Parisian Suit (30% DR)
  - Contractor Suit - Gameplay addition (45% DR)

- **6 NPC Enemy Presets** with custom AI:
  - Assassin (Dual P30L/Glock, flank behavior)
  - Heavy Gunner (M4/KSG, suppress behavior)
  - Tactical Operator (TR-1/Glock, cover/switch behavior)
  - Cleaner (Pit Viper/Knives, stealth behavior)
  - Enforcer (M4/1875, rush/melee behavior)
  - High Table Elite - BOSS (Full loadout, adaptive phases, reinforcements)

- **5 Complete Chapter Loadouts** + Baba Yaga Ultimate:
  - Chapter 1: P30L + M4 + Tactical Suit
  - Chapter 2: Glock 34 + Pit Viper + Bulletproof Suit
  - Chapter 3: STI 2011 + TR-1 + Armored Suit
  - Chapter 4: Pit Viper + TR-1 + Parisian Suit
  - Baba Yaga: ALL weapons + Contractor Suit + Max Ammo

- **Comprehensive Gameplay Configuration** (100+ parameters):
  - Weapon balance (damage, recoil, fire rate, reload)
  - Equipment settings (armor caps, weight penalties)
  - NPC scaling (health, damage, accuracy by difficulty)
  - Cinematic features (slow-mo, bullet time, camera shake)
  - Audio/Visual quality settings
  - Quest quality presets (Low/Medium/High)
  - Accessibility options

- **Dedicated Spawn Menu** with 11 subcategories:
  - Pistols, Shotguns, Revolvers, Rifles, Throwables
  - Suits & Armor, Holsters, Magazine Pouches, Throwable Carriers
  - NPC Enemies, Complete Loadouts
  - Featured items highlighting

- **Automated Build Pipeline** (GitHub Actions):
  - Unity 2022.3 LTS Android AssetBundle build
  - DLL compilation (local - requires B&S assemblies)
  - Automated mod packaging
  - Release publishing to GitHub Releases
  - Weekly scheduled builds
  - Unity license activation support

- **Development Tools**:
  - Prefab Setup Wizard (Editor window)
  - Asset Importer with validation
  - Build Validator with Quest optimization checks
  - Dependency Checker
  - Project Auto-Configuration

- **Documentation**:
  - MOVIE_ACCURACY_REPORT.md (verified film appearances)
  - QUEST_TESTING.md (100+ test cases across 9 phases)
  - IMPLEMENTATION_STATUS.md (progress tracker)
  - BUILD_SYSTEM.md, BUILD_INSTRUCTIONS.md
  - AUDIT_REPORT.md

### Changed
- N/A (Initial release)

### Fixed
- N/A (Initial release)

### Removed
- N/A (Initial release)

---

## [0.9.0] - 2026-11-18 (Pre-Release - Code Complete)

### Added
- All 6 weapon behavior scripts (Pistol, Shotgun, Revolver, Rifle, ThrowingKnife, Pencil)
- Main mod entry point (JohnWickArsenal.cs) with Catalog registration
- Mod configuration (mod.json, weapons.json, spawnmenu.json)
- Unity project structure with URP, Addressables
- Assembly definitions for modular compilation
- Build automation scripts (build.sh, setup.sh, verify_structure.sh)

### Fixed
- Assembly reference errors (invalid UnityEngine refs in Source asmdef)
- Missing B&S references in UnityProject asmdef
- GitHub Actions Unity download URL corrected
- CS0246 compilation errors (316 fixed)
- BladeAndSorcery.Controls imports removed from behaviors
- Build validation for Quest optimization

---

## [0.8.0] - 2026-11-17 (Alpha - Core Systems)

### Added
- JWA_EquipmentSystem.cs (complete equipment registry)
- JWA_NPCSystem.cs (6 NPC presets with behavior profiles)
- JWA_LoadoutManager.cs (5 chapter loadouts + Baba Yaga)
- JWA_GameplayConfig.cs (ScriptableObject with 100+ params)
- JWA_ConfigManager.cs (Runtime config application)
- JWA_PrefabSetupWizard.cs (Editor automation)
- JWA_AssetImporter.cs, JWA_BuildValidator.cs, JWA_DependencyChecker.cs
- JWA_AutoConfiguration.cs (Project setup)

### Fixed
- UnityProject Packages manifest.json created with 6 required packages
- Source/JohnWickArsenal.csproj conditional Unity references
- DLL build now documented as local-only (requires B&S assemblies)

---

## [0.7.0] - 2026-11-16 (Foundation)

### Added
- Initial project structure
- GitHub Actions workflow (build-android.yml)
- Basic build scripts
- Mod metadata (mod.json, icon.png)

---

## Versioning Policy

| Version | Meaning |
|---------|---------|
| MAJOR | Breaking changes (B&S version bump, API changes) |
| MINOR | New weapons, equipment, NPCs, features |
| PATCH | Bug fixes, balance tweaks, config adjustments |

---

## Release Checklist (for 1.0.0)

- [ ] All 10 weapon models imported and prefabs created
- [ ] All 70+ animation clips imported and configured
- [ ] All 82 audio clips imported and assigned
- [ ] All 30+ PBR material sets created and assigned
- [ ] All 12 VFX prefabs created and assigned
- [ ] All 11 equipment models imported and prefabs created
- [ ] All 5 suit models imported and prefabs created
- [ ] NPC models imported (or base B&S humanoid used)
- [ ] Local DLL build successful (build.sh)
- [ ] GitHub Actions AssetBundle build successful
- [ ] Mod ZIP package created and verified
- [ ] Quest 2 testing passed (all Phase 1-5 tests)
- [ ] Quest 3 testing passed (performance)
- [ ] Quest Pro testing passed
- [ ] No memory leaks in 30-min session
- [ ] No crashes in 1-hour session
- [ ] CHANGELOG.md finalized
- [ ] INSTALLATION.md finalized
- [ ] GitHub Release published with mod ZIP

---

*Generated with John Wick Arsenal Build System*