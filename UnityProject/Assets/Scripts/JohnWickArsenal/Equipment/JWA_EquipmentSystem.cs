using UnityEngine;
using BladeAndSorcery.Gameplay;
using System.Collections.Generic;

namespace JohnWickArsenal.Equipment
{
    /// <summary>
    /// Central equipment system managing holsters, pouches, suits, and their interactions.
    /// All equipment is wearable and integrates with B&S inventory/creature systems.
    /// </summary>
    public class JWA_EquipmentSystem : MonoBehaviour
    {
        public static JWA_EquipmentSystem Instance { get; private set; }

        [Header("Equipment Database")]
        [SerializeField] private EquipmentDatabase equipmentDatabase;

        [Header("Suit Presets")]
        [SerializeField] private List<SuitPreset> suitPresets = new List<SuitPreset>();

        [Header("NPC Equipment Loadouts")]
        [SerializeField] private List<NPCLoadout> npcLoadouts = new List<NPCLoadout>();

        private Dictionary<string, EquipmentItem> equipmentRegistry = new Dictionary<string, EquipmentItem>();
        private Dictionary<Creature, EquipmentState> creatureEquipment = new Dictionary<Creature, EquipmentState>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeEquipmentRegistry();
        }

        private void InitializeEquipmentRegistry()
        {
            equipmentRegistry.Clear();

            // Register all holsters
            RegisterEquipment(new EquipmentItem
            {
                id = "jwa_shoulder_holster",
                name = "Shoulder Holster (Double)",
                type = EquipmentType.Holster,
                slot = EquipmentSlot.Chest,
                compatibleWeapons = new[] { "jwa_hk_p30l", "jwa_glock_34", "jwa_sti_2011_combat_master", "jwa_tti_pit_viper" },
                capacity = 2,
                weight = 0.8f,
                concealment = 0.3f,
                armorBonus = 0f,
                prefabPath = "Equipment/Holsters/ShoulderHolster_Double",
                description = "Classic double shoulder rig. Holds two full-size pistols. Worn under suit jacket."
            });

            RegisterEquipment(new EquipmentItem
            {
                id = "jwa_hip_holster",
                name = "Hip Holster (TTI Race)",
                type = EquipmentType.Holster,
                slot = EquipmentSlot.Hip,
                compatibleWeapons = new[] { "jwa_sti_2011_combat_master", "jwa_tti_pit_viper", "jwa_glock_34" },
                capacity = 1,
                weight = 0.4f,
                concealment = 0.1f,
                armorBonus = 0f,
                prefabPath = "Equipment/Holsters/HipHolster_TTI",
                description = "TTI competition hip holster. Fast draw, adjustable retention. For race guns."
            });

            RegisterEquipment(new EquipmentItem
            {
                id = "jwa_thigh_holster",
                name = "Thigh Holster (Tactical)",
                type = EquipmentType.Holster,
                slot = EquipmentSlot.Leg,
                compatibleWeapons = new[] { "jwa_hk_p30l", "jwa_glock_34" },
                capacity = 1,
                weight = 0.5f,
                concealment = 0.0f,
                armorBonus = 0f,
                prefabPath = "Equipment/Holsters/ThighHolster_Tactical",
                description = "Tactical drop-leg holster. Secure retention, quick release. For duty pistols."
            });

            RegisterEquipment(new EquipmentItem
            {
                id = "jwa_ankle_holster",
                name = "Ankle Holster (Concealed)",
                type = EquipmentType.Holster,
                slot = EquipmentSlot.Ankle,
                compatibleWeapons = new[] { "jwa_glock_34", "jwa_hk_p30l" },
                capacity = 1,
                weight = 0.3f,
                concealment = 0.9f,
                armorBonus = 0f,
                prefabPath = "Equipment/Holsters/AnkleHolster_Concealed",
                description = "Deep concealment ankle rig. Slow draw but completely hidden. Backup only."
            });

            RegisterEquipment(new EquipmentItem
            {
                id = "jwa_chest_rig",
                name = "Chest Rig (Magazine Carrier)",
                type = EquipmentType.Pouch,
                slot = EquipmentSlot.Chest,
                compatibleWeapons = new[] { "jwa_hk_p30l", "jwa_glock_34", "jwa_sti_2011_combat_master", "jwa_tti_pit_viper", "jwa_tti_tr1_ultralight", "jwa_benelli_m4", "jwa_keltec_ksg" },
                capacity = 6,
                weight = 1.2f,
                concealment = 0.2f,
                armorBonus = 0f,
                prefabPath = "Equipment/Pouches/ChestRig_Mags",
                description = "Modular chest rig. 6 rifle/pistol mag slots. MOLLE compatible."
            });

            RegisterEquipment(new EquipmentItem
            {
                id = "jwa_belt_pouches",
                name = "Belt Magazine Pouches (4x)",
                type = EquipmentType.Pouch,
                slot = EquipmentSlot.Waist,
                compatibleWeapons = new[] { "jwa_hk_p30l", "jwa_glock_34", "jwa_sti_2011_combat_master", "jwa_tti_pit_viper" },
                capacity = 4,
                weight = 0.6f,
                concealment = 0.4f,
                armorBonus = 0f,
                prefabPath = "Equipment/Pouches/BeltPouches_4x",
                description = "Kydex belt pouches. 4 pistol magazines. Fast reload access."
            });

            RegisterEquipment(new EquipmentItem
            {
                id = "jwa_shotgun_shell_carrier",
                name = "Shotgun Shell Carrier (Side Saddle)",
                type = EquipmentType.Pouch,
                slot = EquipmentSlot.Receiver,
                compatibleWeapons = new[] { "jwa_benelli_m4" },
                capacity = 6,
                weight = 0.5f,
                concealment = 0.0f,
                armorBonus = 0f,
                prefabPath = "Equipment/Pouches/ShellCarrier_BenelliM4",
                description = "Side-saddle shell carrier for Benelli M4. 6 rounds. Mounts on receiver."
            });

            RegisterEquipment(new EquipmentItem
            {
                id = "jwa_ksg_shell_carrier",
                name = "KSG Dual Tube Carrier",
                type = EquipmentType.Pouch,
                slot = EquipmentSlot.Receiver,
                compatibleWeapons = new[] { "jwa_keltec_ksg" },
                capacity = 14,
                weight = 0.8f,
                concealment = 0.0f,
                armorBonus = 0f,
                prefabPath = "Equipment/Pouches/ShellCarrier_KSG",
                description = "Integrated dual-tube shell storage. Matches KSG capacity."
            });

            // Register suits
            RegisterEquipment(new EquipmentItem
            {
                id = "jwa_suit_chapter1",
                name = "Chapter 1 Tactical Suit",
                type = EquipmentType.Suit,
                slot = EquipmentSlot.FullBody,
                compatibleWeapons = new string[0],
                capacity = 0,
                weight = 2.5f,
                concealment = 0.7f,
                armorBonus = 0.15f, // 15% damage reduction
                prefabPath = "Equipment/Suits/Suit_Chapter1",
                description = "Charcoal tactical suit. Light Kevlar weave in lining. Chapter 1 aesthetic."
            });

            RegisterEquipment(new EquipmentItem
            {
                id = "jwa_suit_chapter2",
                name = "Chapter 2 Bulletproof Suit",
                type = EquipmentType.Suit,
                slot = EquipmentSlot.FullBody,
                compatibleWeapons = new string[0],
                capacity = 0,
                weight = 3.2f,
                concealment = 0.8f,
                armorBonus = 0.25f, // 25% damage reduction
                prefabPath = "Equipment/Suits/Suit_Chapter2",
                description = "Grey three-piece with ballistic lining. Tailored fit. Chapter 2 Rome/Continental."
            });

            RegisterEquipment(new EquipmentItem
            {
                id = "jwa_suit_chapter3",
                name = "Chapter 3 Armored Suit",
                type = EquipmentType.Suit,
                slot = EquipmentSlot.FullBody,
                compatibleWeapons = new string[0],
                capacity = 0,
                weight = 4.0f,
                concealment = 0.6f,
                armorBonus = 0.35f, // 35% damage reduction
                prefabPath = "Equipment/Suits/Suit_Chapter3",
                description = "Black tactical suit with trauma plates. Desk job to gunfight ready. Chapter 3."
            });

            RegisterEquipment(new EquipmentItem
            {
                id = "jwa_suit_chapter4",
                name = "Chapter 4 Parisian Suit",
                type = EquipmentType.Suit,
                slot = EquipmentSlot.FullBody,
                compatibleWeapons = new string[0],
                capacity = 0,
                weight = 3.5f,
                concealment = 0.75f,
                armorBonus = 0.30f, // 30% damage reduction
                prefabPath = "Equipment/Suits/Suit_Chapter4",
                description = "Dark navy bespoke suit. D3O impact protection. Paris/Oscars aesthetic. Chapter 4."
            });

            RegisterEquipment(new EquipmentItem
            {
                id = "jwa_suit_contractor",
                name = "Contractor Tactical Suit",
                type = EquipmentType.Suit,
                slot = EquipmentSlot.FullBody,
                compatibleWeapons = new string[0],
                capacity = 0,
                weight = 4.5f,
                concealment = 0.5f,
                armorBonus = 0.45f, // 45% damage reduction (highest)
                prefabPath = "Equipment/Suits/Suit_Contractor",
                description = "Maximum protection. Full plate carrier under suit. For high-threat contracts. Not movie-accurate — gameplay addition."
            });

            // Register throwables pouches
            RegisterEquipment(new EquipmentItem
            {
                id = "jwa_knife_bandolier",
                name = "Throwing Knife Bandolier",
                type = EquipmentType.Pouch,
                slot = EquipmentSlot.Chest,
                compatibleWeapons = new[] { "jwa_throwing_knife" },
                capacity = 8,
                weight = 1.0f,
                concealment = 0.3f,
                armorBonus = 0f,
                prefabPath = "Equipment/Pouches/KnifeBandolier",
                description = "Chest-mounted bandolier. 8 throwing knives. Quick-draw loops."
            });

            RegisterEquipment(new EquipmentItem
            {
                id = "jwa_pencil_case",
                name = "Tactical Pencil Case",
                type = EquipmentType.Pouch,
                slot = EquipmentSlot.Waist,
                compatibleWeapons = new[] { "jwa_pencil" },
                capacity = 3,
                weight = 0.1f,
                concealment = 0.9f,
                armorBonus = 0f,
                prefabPath = "Equipment/Pouches/PencilCase",
                description = "\"Just a pencil case.\" Holds 3 HB #2 pencils. Innocent until it isn't."
            });

            Debug.Log($"[JWA Equipment] Registered {equipmentRegistry.Count} equipment items");
        }

        private void RegisterEquipment(EquipmentItem item)
        {
            equipmentRegistry[item.id] = item;
        }

        public EquipmentItem GetEquipment(string id)
        {
            equipmentRegistry.TryGetValue(id, out var item);
            return item;
        }

        public List<EquipmentItem> GetAllEquipment() => new List<EquipmentItem>(equipmentRegistry.Values);

        public List<EquipmentItem> GetEquipmentByType(EquipmentType type)
        {
            var result = new List<EquipmentItem>();
            foreach (var item in equipmentRegistry.Values)
            {
                if (item.type == type) result.Add(item);
            }
            return result;
        }

        public List<EquipmentItem> GetCompatibleEquipment(string weaponId)
        {
            var result = new List<EquipmentItem>();
            foreach (var item in equipmentRegistry.Values)
            {
                if (item.type == EquipmentType.Holster || item.type == EquipmentType.Pouch)
                {
                    if (item.compatibleWeapons == null || item.compatibleWeapons.Length == 0 ||
                        System.Array.Exists(item.compatibleWeapons, w => w == weaponId))
                    {
                        result.Add(item);
                    }
                }
            }
            return result;
        }

        // ========================================================================
        // CREATURE EQUIPMENT STATE
        // ========================================================================

        public void EquipOnCreature(Creature creature, string equipmentId)
        {
            if (!equipmentRegistry.TryGetValue(equipmentId, out var item))
            {
                Debug.LogWarning($"[JWA Equipment] Unknown equipment: {equipmentId}");
                return;
            }

            if (!creatureEquipment.TryGetValue(creature, out var state))
            {
                state = new EquipmentState { creature = creature };
                creatureEquipment[creature] = state;
            }

            // Check slot availability
            if (state.occupiedSlots.Contains(item.slot))
            {
                Debug.LogWarning($"[JWA Equipment] Slot {item.slot} already occupied on {creature.name}");
                return;
            }

            // Spawn equipment prefab
            var equipmentGO = SpawnEquipment(item, creature);
            if (equipmentGO != null)
            {
                state.equippedItems[equipmentId] = equipmentGO;
                state.occupiedSlots.Add(item.slot);

                // Apply armor bonus
                if (item.armorBonus > 0f)
                {
                    ApplyArmorBonus(creature, item.armorBonus);
                }

                Debug.Log($"[JWA Equipment] Equipped {item.name} on {creature.name}");
            }
        }

        public void UnequipFromCreature(Creature creature, string equipmentId)
        {
            if (!creatureEquipment.TryGetValue(creature, out var state)) return;
            if (!state.equippedItems.TryGetValue(equipmentId, out var equipmentGO)) return;

            if (equipmentRegistry.TryGetValue(equipmentId, out var item))
            {
                state.occupiedSlots.Remove(item.slot);
                if (item.armorBonus > 0f)
                {
                    RemoveArmorBonus(creature, item.armorBonus);
                }
            }

            if (equipmentGO != null)
            {
                Destroy(equipmentGO);
            }

            state.equippedItems.Remove(equipmentId);
            Debug.Log($"[JWA Equipment] Unequipped {equipmentId} from {creature.name}");
        }

        public void ClearCreatureEquipment(Creature creature)
        {
            if (!creatureEquipment.TryGetValue(creature, out var state)) return;

            foreach (var kvp in state.equippedItems)
            {
                if (equipmentRegistry.TryGetValue(kvp.Key, out var item))
                {
                    if (item.armorBonus > 0f)
                    {
                        RemoveArmorBonus(creature, item.armorBonus);
                    }
                }

                if (kvp.Value != null) Destroy(kvp.Value);
            }

            state.equippedItems.Clear();
            state.occupiedSlots.Clear();
        }

        private GameObject SpawnEquipment(EquipmentItem item, Creature creature)
        {
            var prefab = Resources.Load<GameObject>(item.prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[JWA Equipment] Prefab not found: {item.prefabPath}");
                return null;
            }

            var instance = Instantiate(prefab);
            instance.name = item.id;

            // Parent to creature and position at slot
            var slotTransform = GetSlotTransform(creature, item.slot);
            if (slotTransform != null)
            {
                instance.transform.SetParent(slotTransform);
                instance.transform.localPosition = item.localOffset;
                instance.transform.localRotation = Quaternion.Euler(item.localRotation);
            }
            else
            {
                instance.transform.SetParent(creature.transform);
                instance.transform.localPosition = item.localOffset;
                instance.transform.localRotation = Quaternion.Euler(item.localRotation);
            }

            // Add equipment behavior
            var behavior = instance.AddComponent<EquipmentBehavior>();
            behavior.Initialize(item, creature);

            return instance;
        }

        private Transform GetSlotTransform(Creature creature, EquipmentSlot slot)
        {
            // Map equipment slots to creature bones
            var animator = creature.GetComponent<Animator>();
            if (animator == null) return creature.transform;

            switch (slot)
            {
                case EquipmentSlot.Head: return animator.GetBoneTransform(HumanBodyBones.Head);
                case EquipmentSlot.Neck: return animator.GetBoneTransform(HumanBodyBones.Neck);
                case EquipmentSlot.Chest: return animator.GetBoneTransform(HumanBodyBones.Chest);
                case EquipmentSlot.Spine: return animator.GetBoneTransform(HumanBodyBones.Spine);
                case EquipmentSlot.Hip: return animator.GetBoneTransform(HumanBodyBones.Hips);
                case EquipmentSlot.Waist: return animator.GetBoneTransform(HumanBodyBones.Hips);
                case EquipmentSlot.Leg: return animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                case EquipmentSlot.Ankle: return animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                case EquipmentSlot.Shoulder: return animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
                case EquipmentSlot.UpperArm: return animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                case EquipmentSlot.Forearm: return animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                case EquipmentSlot.Hand: return animator.GetBoneTransform(HumanBodyBones.LeftHand);
                case EquipmentSlot.Receiver: return creature.transform.Find("Weapon_R") ?? creature.transform;
                case EquipmentSlot.FullBody: return creature.transform;
                default: return creature.transform;
            }
        }

        private void ApplyArmorBonus(Creature creature, float bonus)
        {
            // Modify creature's damage multiplier or add damage reduction
            var health = creature.GetComponent<CreatureHealth>();
            if (health != null)
            {
                health.damageMultiplier *= (1f - bonus);
            }
        }

        private void RemoveArmorBonus(Creature creature, float bonus)
        {
            var health = creature.GetComponent<CreatureHealth>();
            if (health != null)
            {
                health.damageMultiplier /= (1f - bonus);
            }
        }

        // ========================================================================
        // NPC LOADOUTS
        // ========================================================================

        public void ApplyNPCLoadout(Creature creature, string loadoutId)
        {
            var loadout = npcLoadouts.Find(l => l.id == loadoutId);
            if (loadout == null)
            {
                Debug.LogWarning($"[JWA Equipment] NPC loadout not found: {loadoutId}");
                return;
            }

            ClearCreatureEquipment(creature);

            // Equip suit
            if (!string.IsNullOrEmpty(loadout.suitId))
            {
                EquipOnCreature(creature, loadout.suitId);
            }

            // Equip holsters/pouches
            foreach (var equipId in loadout.equipmentIds)
            {
                EquipOnCreature(creature, equipId);
            }

            // Give weapons (handled by NPC weapon spawner)
            Debug.Log($"[JWA Equipment] Applied NPC loadout '{loadout.name}' to {creature.name}");
        }

        public List<NPCLoadout> GetNPCLoadouts() => new List<NPCLoadout>(npcLoadouts);

        // ========================================================================
        // SUIT PRESETS
        // ========================================================================

        public SuitPreset GetSuitPreset(string id)
        {
            return suitPresets.Find(s => s.id == id);
        }

        public List<SuitPreset> GetAllSuitPresets() => new List<SuitPreset>(suitPresets);
    }

    // ========================================================================
    // DATA CLASSES
    // ========================================================================

    public enum EquipmentType
    {
        Holster,
        Pouch,
        Suit,
        Accessory
    }

    public enum EquipmentSlot
    {
        Head,
        Neck,
        Chest,
        Spine,
        Hip,
        Waist,
        Leg,
        Ankle,
        Shoulder,
        UpperArm,
        Forearm,
        Hand,
        Receiver,
        FullBody
    }

    [System.Serializable]
    public class EquipmentItem
    {
        public string id;
        public string name;
        public EquipmentType type;
        public EquipmentSlot slot;
        public string[] compatibleWeapons;
        public int capacity;
        public float weight;
        [Range(0f, 1f)] public float concealment;
        [Range(0f, 1f)] public float armorBonus;
        public string prefabPath;
        public string description;
        public Vector3 localOffset = Vector3.zero;
        public Vector3 localRotation = Vector3.zero;
    }

    [System.Serializable]
    public class EquipmentState
    {
        public Creature creature;
        public Dictionary<string, GameObject> equippedItems = new Dictionary<string, GameObject>();
        public HashSet<EquipmentSlot> occupiedSlots = new HashSet<EquipmentSlot>();
    }

    [System.Serializable]
    public class SuitPreset
    {
        public string id;
        public string name;
        public string description;
        public string suitEquipmentId;
        public string[] holsterIds;
        public string[] pouchIds;
        public float totalArmorBonus;
        public float totalWeight;
        public float totalConcealment;
        public Sprite previewImage;
    }

    [System.Serializable]
    public class NPCLoadout
    {
        public string id;
        public string name;
        public string description;
        public string suitId;
        public string[] equipmentIds;
        public string[] weaponIds;
        public NPCArchetype archetype;
        public float difficultyMultiplier = 1f;
    }

    public enum NPCArchetype
    {
        Assassin,      // High skill, suit + pistols
        HeavyGunner,   // Armor + shotgun/rifle
        Tactical,      // Balanced, AR + pistol
        Cleaner,       // Silenced weapons, stealth
        Enforcer       // Heavy armor, revolver/shotgun
    }

    // ========================================================================
    // EQUIPMENT BEHAVIOR
    // ========================================================================

    public class EquipmentBehavior : MonoBehaviour
    {
        private EquipmentItem itemData;
        private Creature wearer;
        private bool isInitialized = false;

        public void Initialize(EquipmentItem item, Creature creature)
        {
            itemData = item;
            wearer = creature;
            isInitialized = true;

            // Set layer for equipment
            gameObject.layer = LayerMask.NameToLayer("Equipment");

            // Add visual effects for suits
            if (itemData.type == EquipmentType.Suit && itemData.armorBonus > 0f)
            {
                SetupArmorVisuals();
            }
        }

        private void SetupArmorVisuals()
        {
            // Could add subtle armor glow or impact effects here
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                // Ensure materials support armor shader
                foreach (var mat in r.materials)
                {
                    if (mat.HasProperty("_ArmorLevel"))
                    {
                        mat.SetFloat("_ArmorLevel", itemData.armorBonus);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            // Cleanup handled by EquipmentSystem
        }

        public EquipmentItem ItemData => itemData;
        public Creature Wearer => wearer;
    }
}