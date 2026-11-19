using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace JohnWickArsenal.Data
{
    /// <summary>
    /// Loads weapon/item configuration from JSON files at runtime
    /// Allows tweaking stats without recompiling
    /// </summary>
    public static class ConfigLoader
    {
        private static WeaponConfigData weaponConfig;
        private static SpawnMenuConfig spawnMenuConfig;
        private static bool isLoaded = false;

        public static WeaponConfigData WeaponConfig
        {
            get
            {
                if (!isLoaded) LoadAll();
                return weaponConfig;
            }
        }

        public static SpawnMenuConfig SpawnMenuConfig
        {
            get
            {
                if (!isLoaded) LoadAll();
                return spawnMenuConfig;
            }
        }

        public static void LoadAll()
        {
            string modPath = GetModPath();

            LoadWeaponConfig(Path.Combine(modPath, "ModConfig", "weapons.json"));
            LoadSpawnMenuConfig(Path.Combine(modPath, "ModConfig", "spawnmenu.json"));

            isLoaded = true;
            Debug.Log("[JWA] Configuration loaded successfully");
        }

        private static string GetModPath()
        {
            // In Nomad, mods are typically at /sdcard/Mods/JohnWickArsenal/
            // In Unity Editor, use streaming assets or persistent data path
#if UNITY_ANDROID && !UNITY_EDITOR
            return "/sdcard/Mods/JohnWickArsenal";
#else
            return Application.streamingAssetsPath;
#endif
        }

        private static void LoadWeaponConfig(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    weaponConfig = JsonUtility.FromJson<WeaponConfigData>(json);
                    Debug.Log($"[JWA] Loaded {weaponConfig.weapons.Count} weapons + {weaponConfig.throwables.Count} throwables from config");
                }
                else
                {
                    Debug.LogWarning($"[JWA] Weapon config not found at {path}, using defaults");
                    weaponConfig = new WeaponConfigData();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[JWA] Failed to load weapon config: {e.Message}");
                weaponConfig = new WeaponConfigData();
            }
        }

        private static void LoadSpawnMenuConfig(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    spawnMenuConfig = JsonUtility.FromJson<SpawnMenuConfig>(json);
                }
                else
                {
                    spawnMenuConfig = new SpawnMenuConfig();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[JWA] Failed to load spawn menu config: {e.Message}");
                spawnMenuConfig = new SpawnMenuConfig();
            }
        }

        public static WeaponEntry GetWeapon(string id)
        {
            return WeaponConfig?.weapons?.Find(w => w.id == id);
        }

        public static ThrowableEntry GetThrowable(string id)
        {
            return WeaponConfig?.throwables?.Find(t => t.id == id);
        }

        public static List<WeaponEntry> GetWeaponsByCategory(string category)
        {
            return WeaponConfig?.weapons?.FindAll(w => w.category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        // Data structures matching JSON
        [Serializable]
        public class WeaponConfigData
        {
            public List<WeaponEntry> weapons = new List<WeaponEntry>();
            public List<ThrowableEntry> throwables = new List<ThrowableEntry>();
        }

        [Serializable]
        public class WeaponEntry
        {
            public string id;
            public string name;
            public string category;
            public string film;
            public string scene;
            public string caliber;
            public int capacity;
            public string barrelLength;
            public string weight;
            public List<string> features;
            public string unityPrefab;
            public string behavior;
            public BehaviorConfig behaviorConfig;
            public AnimationConfig animations;
            public AudioConfig audio;
            public ParticleConfig particles;
        }

        [Serializable]
        public class ThrowableEntry
        {
            public string id;
            public string name;
            public string category;
            public string film;
            public string scene;
            public string type;
            public int damage;
            public float throwForce;
            public float spinRate;
            public float stickProbability;
            public int maxStack;
            public bool retrievable;
            public List<string> features;
            public string unityPrefab;
            public string behavior;
            public BehaviorConfig behaviorConfig;
            public AnimationConfig animations;
            public AudioConfig audio;
            public ParticleConfig particles;
        }

        [Serializable]
        public class BehaviorConfig
        {
            // Pistol
            public bool hasCompensator;
            public float compensatorRecoilReduction;
            public bool hasRedDot;
            public bool isRaceGun;
            public float matchTriggerPull;

            // Shotgun
            public bool hasDualTube;
            public bool hasShellCarrier;
            public int shellCarrierCapacity;
            public bool hasARGO;
            public bool hasGhostRingSights;
            public bool hasCollapsibleStock;

            // Revolver
            public bool isSingleAction;
            public bool hasLoadingGate;
            public bool hasEjectorRod;
            public float loadingGateOpenAngle;
            public float loadingGateSpeed;
            public float ejectorRodTravel;
            public float ejectorSpeed;

            // Rifle
            public bool hasSkeletonizedReceiver;
            public bool hasCarbonFiberHandguard;
            public bool hasAdjustableGasBlock;
            public bool hasLightweightBCG;
            public bool hasBinaryTrigger;
            public float weightReduction;

            // Throwables
            public bool isBalanced;
            public bool isDoubleEdged;
            public float headshotMultiplier;
            public float neckMultiplier;
            public float throwForce;
            public float spinRate;
            public int maxKillsBeforeBreak;
            public float penetrationDepth;
            public bool instantKillHeadshot;
            public bool instantKillNeck;
        }

        [Serializable]
        public class AnimationConfig
        {
            public string draw;
            public string idle;
            public string fire;
            public string fire_auto;
            public string reload_tactical;
            public string reload_empty;
            public string reload_start;
            public string reload_shell;
            public string reload_end;
            public string reload_open_gate;
            public string reload_eject;
            public string reload_load;
            public string reload_close_gate;
            public string cock_hammer;
            public string charging_handle;
            public string pump;
            public string tube_switch;
            public string gas_adjust;
            public string inspect;
            public string holster;
            public string throw_anim;
            public string retrieve;
            public string break_anim;
        }

        [Serializable]
        public class AudioConfig
        {
            public string fire;
            public string fire_suppressed;
            public string reload_mag_out;
            public string reload_mag_in;
            public string slide_release;
            public string slide_rack;
            public string bolt_release;
            public string charging_handle;
            public string pump;
            public string shell_load;
            public string shell_load_a;
            public string shell_load_b;
            public string shell_eject;
            public string casing_eject;
            public string round_load;
            public string hammer_cock;
            public string hammer_fall;
            public string gate_open;
            public string gate_close;
            public string ejector_rod;
            public string gas_adjust;
            public string tube_switch;
            public string dry_fire;
            public string throw_sound;
            public string impact_flesh;
            public string impact_hard;
            public string retrieve;
            public string break_sound;
            public string quote;
        }

        [Serializable]
        public class ParticleConfig
        {
            public string muzzle_flash;
            public string compensator_gas;
            public string gas_port;
            public string ejection;
            public string shell_eject;
            public string smoke;
            public string blood;
            public string trail;
            public string graphite_dust;
            public string break_pieces;
        }

        [Serializable]
        public class SpawnMenuConfig
        {
            public string category;
            public string icon;
            public List<Subcategory> subcategories = new List<Subcategory>();
            public List<string> featuredItems = new List<string>();
        }

        [Serializable]
        public class Subcategory
        {
            public string name;
            public string icon;
            public List<SubcategoryItem> items = new List<SubcategoryItem>();
        }

        [Serializable]
        public class SubcategoryItem
        {
            public string id;
            public int sortOrder;
        }
    }
}