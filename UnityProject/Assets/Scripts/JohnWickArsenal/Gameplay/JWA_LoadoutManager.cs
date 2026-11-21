using UnityEngine;
using BladeAndSorcery.Gameplay;
using System.Collections.Generic;
using System.Linq;

namespace JohnWickArsenal.Gameplay
{
    /// <summary>
    /// Manages complete loadout application for player and NPCs.
    /// Handles suit + holsters + pouches + weapons + throwables as cohesive sets.
    /// </summary>
    public class JWA_LoadoutManager : MonoBehaviour
    {
        public static JWA_LoadoutManager Instance { get; private set; }

        [Header("Loadout Definitions")]
        [SerializeField] private List<LoadoutDefinition> loadouts = new List<LoadoutDefinition>();

        private Dictionary<string, LoadoutDefinition> loadoutRegistry = new Dictionary<string, LoadoutDefinition>();
        private Creature playerCreature;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeLoadouts();
        }

        private void Start()
        {
            // Find player creature
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerCreature = player.GetComponent<Creature>();
            }
        }

        private void InitializeLoadouts()
        {
            loadoutRegistry.Clear();

            // CHAPTER 1 LOADOUT
            loadouts.Add(new LoadoutDefinition
            {
                id = "jwa_loadout_chapter1",
                name = "Chapter 1: The Beginning",
                description = "The original Baba Yaga kit. P30L with compensator, Benelli M4, charcoal tactical suit.",
                chapter = 1,
                difficulty = LoadoutDifficulty.Standard,
                suitId = "jwa_suit_chapter1",
                holsterId = "jwa_shoulder_holster",
                pouchIds = new[] { "jwa_belt_pouches", "jwa_shotgun_shell_carrier" },
                primaryWeaponId = "jwa_hk_p30l",
                secondaryWeaponId = "",
                throwableIds = new[] { "jwa_throwing_knife" },
                throwableCarrierId = "jwa_knife_bandolier",
                startingAmmo = new Dictionary<string, int>
                {
                    { "jwa_hk_p30l", 60 },
                    { "jwa_benelli_m4", 28 }
                },
                unlockRequirement = "",
                isDefault = true
            });

            // CHAPTER 2 LOADOUT
            loadouts.Add(new LoadoutDefinition
            {
                id = "jwa_loadout_chapter2",
                name = "Chapter 2: The Marker",
                description = "Rome Continental. Glock 34 MOS, TTI Pit Viper, grey bulletproof three-piece suit.",
                chapter = 2,
                difficulty = LoadoutDifficulty.Standard,
                suitId = "jwa_suit_chapter2",
                holsterId = "jwa_shoulder_holster",
                pouchIds = new[] { "jwa_belt_pouches" },
                primaryWeaponId = "jwa_glock_34",
                secondaryWeaponId = "jwa_tti_pit_viper",
                throwableIds = new[] { "jwa_throwing_knife", "jwa_pencil" },
                throwableCarrierId = "jwa_knife_bandolier",
                startingAmmo = new Dictionary<string, int>
                {
                    { "jwa_glock_34", 68 },
                    { "jwa_tti_pit_viper", 84 }
                },
                unlockRequirement = "Complete Chapter 1 loadout challenges"
            });

            // CHAPTER 3 LOADOUT
            loadouts.Add(new LoadoutDefinition
            {
                id = "jwa_loadout_chapter3",
                name = "Chapter 3: Parabellum",
                description = "Taran Tactical training. STI 2011 Combat Master, TR-1 Ultralight, black armored suit.",
                chapter = 3,
                difficulty = LoadoutDifficulty.Advanced,
                suitId = "jwa_suit_chapter3",
                holsterId = "jwa_hip_holster",
                pouchIds = new[] { "jwa_chest_rig", "jwa_belt_pouches" },
                primaryWeaponId = "jwa_sti_2011_combat_master",
                secondaryWeaponId = "jwa_tti_tr1_ultralight",
                throwableIds = new[] { "jwa_throwing_knife" },
                throwableCarrierId = "jwa_knife_bandolier",
                startingAmmo = new Dictionary<string, int>
                {
                    { "jwa_sti_2011_combat_master", 80 },
                    { "jwa_tti_tr1_ultralight", 120 }
                },
                unlockRequirement = "Complete Chapter 2 loadout challenges"
            });

            // CHAPTER 4 LOADOUT
            loadouts.Add(new LoadoutDefinition
            {
                id = "jwa_loadout_chapter4",
                name = "Chapter 4: The High Table",
                description = "Paris, Osaka, New York. Pit Viper, TR-1 Ultralight, navy Parisian suit with D3O armor.",
                chapter = 4,
                difficulty = LoadoutDifficulty.Advanced,
                suitId = "jwa_suit_chapter4",
                holsterId = "jwa_shoulder_holster",
                pouchIds = new[] { "jwa_chest_rig", "jwa_belt_pouches" },
                primaryWeaponId = "jwa_tti_pit_viper",
                secondaryWeaponId = "jwa_tti_tr1_ultralight",
                throwableIds = new[] { "jwa_throwing_knife", "jwa_pencil" },
                throwableCarrierId = "jwa_knife_bandolier",
                startingAmmo = new Dictionary<string, int>
                {
                    { "jwa_tti_pit_viper", 84 },
                    { "jwa_tti_tr1_ultralight", 120 }
                },
                unlockRequirement = "Complete Chapter 3 loadout challenges"
            });

            // BABA YAGA - ULTIMATE LOADOUT
            loadouts.Add(new LoadoutDefinition
            {
                id = "jwa_loadout_baba_yaga",
                name = "BABA YAGA - The One They Fear",
                description = "EVERYTHING. All weapons, Contractor suit (max armor), infinite potential. You are the myth.",
                chapter = 99,
                difficulty = LoadoutDifficulty.Legendary,
                suitId = "jwa_suit_contractor",
                holsterId = "jwa_shoulder_holster",
                pouchIds = new[] { "jwa_chest_rig", "jwa_belt_pouches", "jwa_shotgun_shell_carrier", "jwa_knife_bandolier", "jwa_pencil_case" },
                primaryWeaponId = "jwa_tti_pit_viper",
                secondaryWeaponId = "jwa_tti_tr1_ultralight",
                additionalWeaponIds = new[]
                {
                    "jwa_hk_p30l",
                    "jwa_glock_34",
                    "jwa_sti_2011_combat_master",
                    "jwa_benelli_m4",
                    "jwa_keltec_ksg",
                    "jwa_remington_1875"
                },
                throwableIds = new[] { "jwa_throwing_knife", "jwa_pencil" },
                throwableCarrierId = "jwa_knife_bandolier",
                startingAmmo = new Dictionary<string, int>
                {
                    { "jwa_hk_p30l", 150 },
                    { "jwa_glock_34", 170 },
                    { "jwa_sti_2011_combat_master", 200 },
                    { "jwa_tti_pit_viper", 210 },
                    { "jwa_benelli_m4", 56 },
                    { "jwa_keltec_ksg", 280 },
                    { "jwa_remington_1875", 60 },
                    { "jwa_tti_tr1_ultralight", 300 }
                },
                unlockRequirement = "Complete all Chapter loadouts on Nightmare difficulty"
            });

            // Register all
            foreach (var loadout in loadouts)
            {
                loadoutRegistry[loadout.id] = loadout;
            }

            Debug.Log($"[JWA Loadout] Registered {loadouts.Count} loadouts");
        }

        // ========================================================================
        // PUBLIC API
        // ========================================================================

        public LoadoutDefinition GetLoadout(string id)
        {
            loadoutRegistry.TryGetValue(id, out var loadout);
            return loadout;
        }

        public List<LoadoutDefinition> GetAllLoadouts() => new List<LoadoutDefinition>(loadouts);

        public List<LoadoutDefinition> GetAvailableLoadouts()
        {
            // In a full implementation, check unlock requirements
            return loadouts.Where(l => string.IsNullOrEmpty(l.unlockRequirement) || IsUnlocked(l.unlockRequirement)).ToList();
        }

        public void ApplyLoadoutToPlayer(string loadoutId)
        {
            var loadout = GetLoadout(loadoutId);
            if (loadout == null)
            {
                Debug.LogError($"[JWA Loadout] Loadout not found: {loadoutId}");
                return;
            }

            if (playerCreature == null)
            {
                Debug.LogError("[JWA Loadout] Player creature not found");
                return;
            }

            ApplyLoadout(playerCreature, loadout);
            Debug.Log($"[JWA Loadout] Applied {loadout.name} to player");
        }

        public void ApplyLoadoutToCreature(Creature creature, string loadoutId)
        {
            var loadout = GetLoadout(loadoutId);
            if (loadout == null) return;

            ApplyLoadout(creature, loadout);
        }

        private void ApplyLoadout(Creature creature, LoadoutDefinition loadout)
        {
            var equipSystem = JWA_EquipmentSystem.Instance;
            if (equipSystem == null) return;

            // Clear existing equipment
            equipSystem.ClearCreatureEquipment(creature);

            // 1. Apply Suit
            if (!string.IsNullOrEmpty(loadout.suitId))
            {
                equipSystem.EquipOnCreature(creature, loadout.suitId);
            }

            // 2. Apply Holster
            if (!string.IsNullOrEmpty(loadout.holsterId))
            {
                equipSystem.EquipOnCreature(creature, loadout.holsterId);
            }

            // 3. Apply Pouches
            if (loadout.pouchIds != null)
            {
                foreach (var pouchId in loadout.pouchIds)
                {
                    equipSystem.EquipOnCreature(creature, pouchId);
                }
            }

            // 4. Apply Throwable Carrier
            if (!string.IsNullOrEmpty(loadout.throwableCarrierId))
            {
                equipSystem.EquipOnCreature(creature, loadout.throwableCarrierId);
            }

            // 5. Give Weapons
            GiveWeapons(creature, loadout);

            // 6. Apply starting ammo
            ApplyStartingAmmo(creature, loadout);
        }

        private void GiveWeapons(Creature creature, LoadoutDefinition loadout)
        {
            var catalog = Catalog.GetCatalog<ItemData>();
            if (catalog == null) return;

            // Primary weapon (right hand)
            if (!string.IsNullOrEmpty(loadout.primaryWeaponId))
            {
                GiveWeapon(creature, loadout.primaryWeaponId, true);
            }

            // Secondary weapon (left hand or holstered)
            if (!string.IsNullOrEmpty(loadout.secondaryWeaponId))
            {
                GiveWeapon(creature, loadout.secondaryWeaponId, false);
            }

            // Additional weapons (for Baba Yaga)
            if (loadout.additionalWeaponIds != null)
            {
                foreach (var weaponId in loadout.additionalWeaponIds)
                {
                    if (weaponId != loadout.primaryWeaponId && weaponId != loadout.secondaryWeaponId)
                    {
                        GiveWeapon(creature, weaponId, false);
                    }
                }
            }

            // Throwables
            if (loadout.throwableIds != null)
            {
                foreach (var throwableId in loadout.throwableIds)
                {
                    for (int i = 0; i < 3; i++) // Give 3 of each throwable by default
                    {
                        GiveWeapon(creature, throwableId, false);
                    }
                }
            }
        }

        private void GiveWeapon(Creature creature, string weaponId, bool isPrimary)
        {
            var catalog = Catalog.GetCatalog<ItemData>();
            if (catalog == null) return;

            var itemData = catalog.GetData(weaponId);
            if (itemData == null) return;

            var item = Item.Instantiate(itemData, creature.transform.position, creature.transform.rotation);
            if (item == null) return;

            var hand = isPrimary ? creature.ragdoll.GetHand(Side.Right) : creature.ragdoll.GetHand(Side.Left);
            if (hand != null)
            {
                hand.Grab(item.GetComponent<Item>());
            }
            else
            {
                // Put in inventory/holster if hand not available
                creature.inventory.AddItem(item.GetComponent<Item>());
            }
        }

        private void ApplyStartingAmmo(Creature creature, LoadoutDefinition loadout)
        {
            if (loadout.startingAmmo == null) return;

            foreach (var kvp in loadout.startingAmmo)
            {
                // Ammo is handled by B&S magazine system
                // This would need to integrate with B&S's ammo pool
                creature.SetData($"JWA_StartingAmmo_{kvp.Key}", kvp.Value);
            }
        }

        // ========================================================================
        // UNLOCK SYSTEM
        // ========================================================================

        public bool IsUnlocked(string requirement)
        {
            // Check PlayerPrefs or save system
            return PlayerPrefs.GetInt($"JWA_Unlocked_{requirement.GetHashCode()}", 0) == 1;
        }

        public void UnlockLoadout(string loadoutId)
        {
            var loadout = GetLoadout(loadoutId);
            if (loadout == null) return;

            if (!string.IsNullOrEmpty(loadout.unlockRequirement))
            {
                PlayerPrefs.SetInt($"JWA_Unlocked_{loadout.unlockRequirement.GetHashCode()}", 1);
                PlayerPrefs.Save();
            }
        }

        public void CheckUnlocks()
        {
            // Check chapter completion, kill counts, etc.
            // This would integrate with game progression
        }

        // ========================================================================
        // RANDOM LOADOUT
        // ========================================================================

        public LoadoutDefinition GetRandomLoadout(LoadoutDifficulty? difficulty = null)
        {
            var available = GetAvailableLoadouts();
            if (difficulty.HasValue)
            {
                available = available.Where(l => l.difficulty == difficulty.Value).ToList();
            }

            if (available.Count == 0) return null;

            return available[Random.Range(0, available.Count)];
        }
    }

    // ========================================================================
    // DATA CLASSES
    // ========================================================================

    public enum LoadoutDifficulty
    {
        Standard,
        Advanced,
        Legendary
    }

    [System.Serializable]
    public class LoadoutDefinition
    {
        public string id;
        public string name;
        public string description;
        public int chapter;
        public LoadoutDifficulty difficulty;
        public string suitId;
        public string holsterId;
        public string[] pouchIds;
        public string primaryWeaponId;
        public string secondaryWeaponId;
        public string[] additionalWeaponIds;
        public string[] throwableIds;
        public string throwableCarrierId;
        public Dictionary<string, int> startingAmmo = new Dictionary<string, int>();
        public string unlockRequirement;
        public bool isDefault = false;
        public Sprite previewImage;
    }
}