# John Wick Arsenal - Installation Guide

**Version:** 1.0.0  
**Target:** Blade & Sorcery Nomad 1.3.1 on Meta Quest 2/3/Pro

---

## Requirements

### Hardware
- **Meta Quest 2**, **Quest 3**, **Quest Pro**, or **Quest 3S**
- Minimum 2GB free storage space
- Developer Mode enabled (for manual install)

### Software
- **Blade & Sorcery Nomad** version 1.3.1 or later
- **BSMG Mod Loader** (recommended) OR **Mod.io** app
- **SideQuest** (for manual APK install) OR **ADB** command line

### Mod File
- `JohnWickArsenal_v1.0.0.zip` (from GitHub Releases)

---

## Installation Methods

### Method 1: BSMG Mod Loader (Recommended - Easiest)

1. **Install BSMG** on your Quest:
   - Open SideQuest on PC
   - Search "BSMG" and install to Quest
   - Launch BSMG on Quest, follow setup wizard

2. **Download the mod**:
   - Go to GitHub Releases: `https://github.com/<your-repo>/JohnWickArsenal/releases`
   - Download `JohnWickArsenal_v1.0.0.zip`

3. **Install via BSMG**:
   - Open BSMG on Quest
   - Go to "Mods" tab → "Import Mod"
   - Select the downloaded ZIP file
   - Wait for "Mod installed successfully"

4. **Enable the mod**:
   - In BSMG, find "John Wick Arsenal" in mod list
   - Toggle ON
   - Restart Blade & Sorcery when prompted

---

### Method 2: Mod.io (In-Game)

1. **Install Mod.io app** on Quest:
   - From Quest Store, search "Mod.io" and install

2. **Subscribe to mod**:
   - Launch Mod.io app
   - Search "John Wick Arsenal"
   - Click "Subscribe"
   - Wait for download

3. **Enable in B&S**:
   - Launch Blade & Sorcery
   - Go to Mods menu
   - Find "John Wick Arsenal" and enable
   - Restart game

---

### Method 3: Manual Install (ADB/SideQuest - Advanced)

#### Prerequisites
- ADB installed on PC (`adb version` works)
- Quest in Developer Mode with USB debugging authorized

#### Steps

1. **Connect Quest via USB**:
   ```bash
   adb devices
   # Should show your Quest serial number
   ```

2. **Create mods directory** (if not exists):
   ```bash
   adb shell mkdir -p /sdcard/Mods
   ```

3. **Extract ZIP to Quest**:
   ```bash
   # On PC - extract ZIP first
   unzip JohnWickArsenal_v1.0.0.zip -d JohnWickArsenal
   
   # Push to Quest
   adb push JohnWickArsenal /sdcard/Mods/
   ```

4. **Verify installation**:
   ```bash
   adb shell ls -la /sdcard/Mods/JohnWickArsenal/
   # Should show: mod.json, icon.png, JohnWickArsenal.dll, johnwickarsenal.bundle, README.md
   ```

5. **Enable in B&S**:
   - Launch Blade & Sorcery
   - Go to Mods menu
   - Find "John Wick Arsenal" and enable
   - Restart game

---

## Verification Checklist

After installation, verify in-game:

- [ ] **Mod Menu**: "John Wick Arsenal" appears in mods list, enabled
- [ ] **Spawn Menu**: New category "John Wick Arsenal" with custom icon
- [ ] **Subcategories**: 11 subcategories visible:
  - [ ] Pistols (4 weapons)
  - [ ] Shotguns (2 weapons)
  - [ ] Revolvers (1 weapon)
  - [ ] Rifles (1 weapon)
  - [ ] Throwables (2 items)
  - [ ] Suits & Armor (5 suits)
  - [ ] Holsters (4 holsters)
  - [ ] Magazine Pouches (4 pouches)
  - [ ] Throwable Carriers (2 carriers)
  - [ ] NPC Enemies (6 NPCs)
  - [ ] Complete Loadouts (5 loadouts)
- [ ] **Featured Items**: Pit Viper, Pencil, Benelli M4, Chapter 4 Suit, High Table Elite shown
- [ ] **Weapon Spawn**: Each weapon spawns correctly (model, sounds, behavior)
- [ ] **Equipment**: Suits, holsters, pouches appear on player body
- [ ] **NPCs**: NPC enemies spawn with correct loadouts and behavior
- [ ] **Loadouts**: Chapter loadouts apply full equipment + weapon sets

---

## Troubleshooting

### Mod not appearing in B&S Mods Menu
| Cause | Solution |
|-------|----------|
| Wrong B&S version | Requires Nomad 1.3.1+ |
| Mod not in correct folder | Must be in `/sdcard/Mods/JohnWickArsenal/` |
| Missing mod.json | Verify ZIP extracted correctly |
| BSMG not updated | Update BSMG to latest version |

### Weapons spawn but no model (invisible/pink)
| Cause | Solution |
|-------|----------|
| AssetBundle not loaded | Restart B&S completely |
| Wrong Unity version | Mod built for Unity 2022.3 LTS |
| Missing URP | Ensure B&S is on supported version |

### Weapons spawn but no custom behavior (vanilla behavior)
| Cause | Solution |
|-------|----------|
| DLL not loaded | Check `JohnWickArsenal.dll` exists in mod folder |
| Assembly mismatch | Mod built for specific B&S version |

### Crashes on weapon spawn
| Cause | Solution |
|-------|----------|
| Out of memory | Close other apps, restart Quest |
| Too many mods | Disable other mods, test JWA alone |
| Corrupt download | Re-download and reinstall |

### NPCs don't spawn / spawn with errors
| Cause | Solution |
|-------|----------|
| Missing NPC data | Ensure mod.json includes NPC entries |
| Behavior script error | Check logcat for JWA_NPCSystem errors |

### Performance Issues (Low FPS)
| Solution | Description |
|----------|-------------|
| Lower quality | In JWA settings (if available) or B&S graphics settings |
| Reduce NPC count | Spawn fewer enemies at once |
| Restart Quest | Clear memory leaks |
| Disable other mods | Test JWA alone |

---

## Uninstallation

### BSMG
1. Open BSMG on Quest
2. Find "John Wick Arsenal"
3. Click "Uninstall"
4. Restart B&S

### Manual
```bash
adb shell rm -rf /sdcard/Mods/JohnWickArsenal
```

### Mod.io
1. Open Mod.io app
2. Find "John Wick Arsenal"
3. Click "Unsubscribe"

---

## Updating

1. **Backup saves** (optional but recommended):
   ```bash
   adb pull /sdcard/BladeAndSorcery/Saves/ ./backup_saves/
   ```

2. **Install new version** (same method as initial install)
   - BSMG: Import new ZIP (will replace)
   - Mod.io: Auto-updates when subscribed
   - Manual: Delete old folder, push new one

3. **Verify** in-game that version number updated

---

## Known Issues (v1.0.0)

| Issue | Workaround | Fix Target |
|-------|------------|------------|
| Remington 1875 not in films | Labeled as "Inspired Addition" in menu | N/A - Design choice |
| Baba Yaga loadout heavy | May impact FPS on Quest 2 | Use Chapter loadouts instead |
| NPC reinforcement spawn | Can exceed max NPC limit | v1.0.1 |
| Pencil quote volume | May be quiet on some devices | v1.0.1 |

---

## Support

- **GitHub Issues**: `https://github.com/<your-repo>/JohnWickArsenal/issues`
- **BSMG Discord**: `#mod-support` channel
- **Mod.io Comments**: On mod page

When reporting issues, include:
1. Quest model (2/3/Pro/3S)
2. B&S version
3. Mod version
4. Steps to reproduce
5. Logcat if possible: `adb logcat -d > log.txt`

---

## File Structure (for Manual Install Verification)

```
/sdcard/Mods/JohnWickArsenal/
├── mod.json                    # Mod metadata (id, version, dependencies)
├── icon.png                    # 256x256 mod icon
├── JohnWickArsenal.dll         # Compiled C# behaviors (REQUIRED)
├── johnwickarsenal.bundle      # Unity AssetBundle (models, textures, audio)
├── README.md                   # This file
└── CHANGELOG.md                # Version history
```

**Critical Files:** `mod.json`, `JohnWickArsenal.dll`, `johnwickarsenal.bundle` — all three required for mod to function.

---

## Advanced: Building from Source

If you want to build the mod yourself (Chromebook/Linux):

### Prerequisites
- Unity 2022.3.20f1 LTS (Linux)
- .NET SDK 8.0
- Android SDK (API 33, NDK 25)
- Blade & Sorcery Nomad installed (for assembly references)

### Build Steps
```bash
# Clone repository
git clone https://github.com/<your-repo>/JohnWickArsenal.git
cd JohnWickArsenal

# Run setup (installs Unity packages, configures project)
./setup.sh

# Build DLL (requires B&S assemblies - LOCAL ONLY)
./build.sh dll

# Build AssetBundle (requires Unity)
./build.sh bundle

# Package mod
./build.sh package
```

### GitHub Actions (Automated)
- Push to `master` branch triggers build
- Creates Release on tag push
- Weekly scheduled builds every Monday

See `BUILD_INSTRUCTIONS.md` and `BUILD_SYSTEM.md` for details.

---

*"People keep asking if I'm back. Yeah, I'm thinking I'm back."* — John Wick

**Enjoy the arsenal.** 🎯