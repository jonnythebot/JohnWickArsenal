using UnityEngine;
using BladeAndSorcery.Gameplay;
using System.Collections.Generic;
using System.Linq;

namespace JohnWickArsenal.NPCs
{
    /// <summary>
    /// NPC integration system for John Wick Arsenal.
    /// Spawns enemies with JWA weapons, equipment, and custom behaviors.
    /// </summary>
    public class JWA_NPCSystem : MonoBehaviour
    {
        public static JWA_NPCSystem Instance { get; private set; }

        [Header("NPC Presets")]
        [SerializeField] private List<NPCPreset> npcPresets = new List<NPCPreset>();

        [Header("Spawn Settings")]
        [SerializeField] private float spawnRadius = 10f;
        [SerializeField] private int maxActiveNPCs = 20;
        [SerializeField] private LayerMask spawnLayerMask;

        private Dictionary<string, NPCPreset> presetRegistry = new Dictionary<string, NPCPreset>();
        private List<Creature> activeJWANPCs = new List<Creature>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePresets();
        }

        private void InitializePresets()
        {
            presetRegistry.Clear();

            // PRESET 1: THE ASSASSIN (Chapter 1-4 style)
            var assassin = new NPCPreset
            {
                id = "jwa_npc_assassin",
                name = "Assassin",
                description = "Elite hitman. Dual pistols, suit, high accuracy. The Baba Yaga's peers.",
                archetype = NPCArchetype.Assassin,
                difficulty = NPCDifficulty.Elite,
                healthMultiplier = 1.5f,
                damageMultiplier = 1.3f,
                speedMultiplier = 1.2f,
                accuracyMultiplier = 1.5f,
                reactionTimeMultiplier = 0.7f,
                suitId = "jwa_suit_chapter2",
                equipmentIds = new[] { "jwa_shoulder_holster", "jwa_belt_pouches" },
                primaryWeaponId = "jwa_hk_p30l",
                secondaryWeaponId = "jwa_glock_34",
                throwableIds = new[] { "jwa_throwing_knife" },
                throwableCount = 3,
                behaviorProfile = NPCBehaviorProfile.Assassin,
                spawnWeight = 10,
                minPlayerLevel = 1
            };
            RegisterPreset(assassin);

            // PRESET 2: THE HEAVY GUNNER (Shotgun/Rifle specialist)
            var heavyGunner = new NPCPreset
            {
                id = "jwa_npc_heavygunner",
                name = "Heavy Gunner",
                description = "Armored enforcer. Shotgun or rifle, high damage, slow but durable.",
                archetype = NPCArchetype.HeavyGunner,
                difficulty = NPCDifficulty.Heavy,
                healthMultiplier = 2.5f,
                damageMultiplier = 1.5f,
                speedMultiplier = 0.8f,
                accuracyMultiplier = 1.0f,
                reactionTimeMultiplier = 1.0f,
                suitId = "jwa_suit_contractor",
                equipmentIds = new[] { "jwa_chest_rig", "jwa_shotgun_shell_carrier" },
                primaryWeaponId = "jwa_benelli_m4",
                secondaryWeaponId = "jwa_keltec_ksg",
                throwableIds = new string[0],
                throwableCount = 0,
                behaviorProfile = NPCBehaviorProfile.HeavyGunner,
                spawnWeight = 8,
                minPlayerLevel = 3
            };
            RegisterPreset(heavyGunner);

            // PRESET 3: THE TACTICAL OPERATOR (AR + Pistol, balanced)
            var tactical = new NPCPreset
            {
                id = "jwa_npc_tactical",
                name = "Tactical Operator",
                description = "Professional mercenary. TR-1 Ultralight + sidearm. Coordinated tactics.",
                archetype = NPCArchetype.Tactical,
                difficulty = NPCDifficulty.Veteran,
                healthMultiplier = 1.3f,
                damageMultiplier = 1.2f,
                speedMultiplier = 1.1f,
                accuracyMultiplier = 1.3f,
                reactionTimeMultiplier = 0.8f,
                suitId = "jwa_suit_chapter3",
                equipmentIds = new[] { "jwa_chest_rig", "jwa_hip_holster", "jwa_belt_pouches" },
                primaryWeaponId = "jwa_tti_tr1_ultralight",
                secondaryWeaponId = "jwa_glock_34",
                throwableIds = new[] { "jwa_throwing_knife" },
                throwableCount = 2,
                behaviorProfile = NPCBehaviorProfile.Tactical,
                spawnWeight = 12,
                minPlayerLevel = 2
            };
            RegisterPreset(tactical);

            // PRESET 4: THE CLEANER (Silenced, stealth, precision)
            var cleaner = new NPCPreset
            {
                id = "jwa_npc_cleaner",
                name = "Cleaner",
                description = "Silent specialist. Suppressed Pit Viper, throwing knives, high stealth.",
                archetype = NPCArchetype.Cleaner,
                difficulty = NPCDifficulty.Elite,
                healthMultiplier = 1.2f,
                damageMultiplier = 1.4f,
                speedMultiplier = 1.3f,
                accuracyMultiplier = 1.6f,
                reactionTimeMultiplier = 0.6f,
                suitId = "jwa_suit_chapter4",
                equipmentIds = new[] { "jwa_shoulder_holster", "jwa_knife_bandolier", "jwa_pencil_case" },
                primaryWeaponId = "jwa_tti_pit_viper",
                secondaryWeaponId = "jwa_hk_p30l",
                throwableIds = new[] { "jwa_throwing_knife", "jwa_pencil" },
                throwableCount = 4,
                behaviorProfile = NPCBehaviorProfile.Cleaner,
                spawnWeight = 6,
                minPlayerLevel = 5
            };
            RegisterPreset(cleaner);

            // PRESET 5: THE ENFORCER (Revolver + Shotgun, close range brute)
            var enforcer = new NPCPreset
            {
                id = "jwa_npc_enforcer",
                name = "Enforcer",
                description = "Brute force. Remington 1875 + Benelli M4. High damage, melee capable.",
                archetype = NPCArchetype.Enforcer,
                difficulty = NPCDifficulty.Heavy,
                healthMultiplier = 2.0f,
                damageMultiplier = 1.6f,
                speedMultiplier = 0.9f,
                accuracyMultiplier = 1.1f,
                reactionTimeMultiplier = 0.9f,
                suitId = "jwa_suit_chapter1",
                equipmentIds = new[] { "jwa_thigh_holster", "jwa_shotgun_shell_carrier" },
                primaryWeaponId = "jwa_benelli_m4",
                secondaryWeaponId = "jwa_remington_1875",
                throwableIds = new[] { "jwa_throwing_knife" },
                throwableCount = 2,
                behaviorProfile = NPCBehaviorProfile.Enforcer,
                spawnWeight = 7,
                minPlayerLevel = 4
            };
            RegisterPreset(enforcer);

            // PRESET 6: HIGH TABLE ELITE (Boss-tier)
            var highTable = new NPCPreset
            {
                id = "jwa_npc_hightable_elite",
                name = "High Table Elite",
                description = "The best of the best. Full loadout, maximum stats. Boss encounter.",
                archetype = NPCArchetype.Assassin,
                difficulty = NPCDifficulty.Boss,
                healthMultiplier = 4.0f,
                damageMultiplier = 1.8f,
                speedMultiplier = 1.3f,
                accuracyMultiplier = 1.8f,
                reactionTimeMultiplier = 0.5f,
                suitId = "jwa_suit_contractor",
                equipmentIds = new[] { "jwa_shoulder_holster", "jwa_chest_rig", "jwa_knife_bandolier", "jwa_pencil_case" },
                primaryWeaponId = "jwa_tti_pit_viper",
                secondaryWeaponId = "jwa_tti_tr1_ultralight",
                throwableIds = new[] { "jwa_throwing_knife", "jwa_pencil" },
                throwableCount = 5,
                behaviorProfile = NPCBehaviorProfile.Boss,
                spawnWeight = 1,
                minPlayerLevel = 10,
                isBoss = true,
                bossTitle = "HIGH TABLE ELITE"
            };
            RegisterPreset(highTable);

            Debug.Log($"[JWA NPC] Registered {presetRegistry.Count} NPC presets");
        }

        private void RegisterPreset(NPCPreset preset)
        {
            presetRegistry[preset.id] = preset;
        }

        public NPCPreset GetPreset(string id)
        {
            presetRegistry.TryGetValue(id, out var preset);
            return preset;
        }

        public List<NPCPreset> GetAllPresets() => new List<NPCPreset>(presetRegistry.Values);

        public List<NPCPreset> GetPresetsByDifficulty(NPCDifficulty difficulty)
        {
            return presetRegistry.Values.Where(p => p.difficulty == difficulty).ToList();
        }

        public List<NPCPreset> GetPresetsByArchetype(NPCArchetype archetype)
        {
            return presetRegistry.Values.Where(p => p.archetype == archetype).ToList();
        }

        // ========================================================================
        // SPAWNING
        // ========================================================================

        public Creature SpawnNPC(string presetId, Vector3 position, Quaternion rotation, Creature.Faction faction = Creature.Faction.Enemy)
        {
            var preset = GetPreset(presetId);
            if (preset == null)
            {
                Debug.LogError($"[JWA NPC] Preset not found: {presetId}");
                return null;
            }

            if (activeJWANPCs.Count >= maxActiveNPCs)
            {
                Debug.LogWarning("[JWA NPC] Max active NPCs reached");
                return null;
            }

            // Use B&S creature spawning system
            var creature = SpawnCreature(preset, position, rotation, faction);
            if (creature == null) return null;

            // Configure creature stats
            ConfigureCreatureStats(creature, preset);

            // Apply equipment loadout
            ApplyEquipmentLoadout(creature, preset);

            // Give weapons
            GiveWeapons(creature, preset);

            // Apply behavior profile
            ApplyBehaviorProfile(creature, preset);

            // Track
            activeJWANPCs.Add(creature);
            creature.OnDespawnEvent += () => OnNPCDied(creature);

            Debug.Log($"[JWA NPC] Spawned {preset.name} at {position}");
            return creature;
        }

        public Creature SpawnRandomNPC(Vector3 position, Quaternion rotation, int playerLevel = 1, Creature.Faction faction = Creature.Faction.Enemy)
        {
            // Filter by player level
            var available = presetRegistry.Values
                .Where(p => p.minPlayerLevel <= playerLevel)
                .ToList();

            if (available.Count == 0) return null;

            // Weighted random selection
            int totalWeight = available.Sum(p => p.spawnWeight);
            int roll = Random.Range(0, totalWeight);
            int cumulative = 0;

            foreach (var preset in available)
            {
                cumulative += preset.spawnWeight;
                if (roll < cumulative)
                {
                    return SpawnNPC(preset.id, position, rotation, faction);
                }
            }

            return SpawnNPC(available[0].id, position, rotation, faction);
        }

        public void SpawnWave(int count, Vector3 center, float radius, int playerLevel = 1)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos = center + Random.insideUnitSphere * radius;
                spawnPos.y = center.y;
                SpawnRandomNPC(spawnPos, Quaternion.Euler(0, Random.Range(0, 360), 0), playerLevel);
            }
        }

        private Creature SpawnCreature(NPCPreset preset, Vector3 position, Quaternion rotation, Creature.Faction faction)
        {
            // This uses B&S's creature spawning - we need to reference the appropriate catalog
            // For now, return a placeholder that the mod entry will hook into
            // Actual implementation depends on B&S's CreatureFactory/Catalog system

            var catalog = Catalog.GetCatalog<CreatureData>();
            if (catalog == null) return null;

            // Try to find a base human creature data
            var baseCreatureData = catalog.GetData("Human") ?? catalog.GetData("Enemy") ?? catalog.GetData("NPC");
            if (baseCreatureData == null) return null;

            // Spawn via B&S system
            var creature = Creature.Instantiate(baseCreatureData, position, rotation);
            if (creature == null) return null;

            creature.faction = faction;
            creature.creatureName = preset.name;

            return creature;
        }

        private void ConfigureCreatureStats(Creature creature, NPCPreset preset)
        {
            // Apply stat multipliers
            var health = creature.GetComponent<CreatureHealth>();
            if (health != null)
            {
                health.maxHealth *= preset.healthMultiplier;
                health.currentHealth = health.maxHealth;
            }

            // Damage multiplier applied via behavior
            creature.SetData("JWA_DamageMultiplier", preset.damageMultiplier);
            creature.SetData("JWA_SpeedMultiplier", preset.speedMultiplier);
            creature.SetData("JWA_AccuracyMultiplier", preset.accuracyMultiplier);
            creature.SetData("JWA_ReactionTimeMultiplier", preset.reactionTimeMultiplier);

            if (preset.isBoss)
            {
                creature.SetData("JWA_IsBoss", true);
                creature.SetData("JWA_BossTitle", preset.bossTitle);
            }
        }

        private void ApplyEquipmentLoadout(Creature creature, NPCPreset preset)
        {
            var equipSystem = JWA_EquipmentSystem.Instance;
            if (equipSystem == null) return;

            // Apply suit
            if (!string.IsNullOrEmpty(preset.suitId))
            {
                equipSystem.EquipOnCreature(creature, preset.suitId);
            }

            // Apply holsters/pouches
            if (preset.equipmentIds != null)
            {
                foreach (var equipId in preset.equipmentIds)
                {
                    equipSystem.EquipOnCreature(creature, equipId);
                }
            }
        }

        private void GiveWeapons(Creature creature, NPCPreset preset)
        {
            // Primary weapon
            if (!string.IsNullOrEmpty(preset.primaryWeaponId))
            {
                GiveWeaponToCreature(creature, preset.primaryWeaponId, true);
            }

            // Secondary weapon
            if (!string.IsNullOrEmpty(preset.secondaryWeaponId))
            {
                GiveWeaponToCreature(creature, preset.secondaryWeaponId, false);
            }

            // Throwables
            if (preset.throwableIds != null && preset.throwableCount > 0)
            {
                for (int i = 0; i < preset.throwableCount; i++)
                {
                    var throwableId = preset.throwableIds[Random.Range(0, preset.throwableIds.Length)];
                    GiveWeaponToCreature(creature, throwableId, false);
                }
            }
        }

        private void GiveWeaponToCreature(Creature creature, string weaponId, bool isPrimary)
        {
            // Use B&S item spawning system
            var catalog = Catalog.GetCatalog<ItemData>();
            if (catalog == null) return;

            var itemData = catalog.GetData(weaponId);
            if (itemData == null)
            {
                Debug.LogWarning($"[JWA NPC] Weapon data not found: {weaponId}");
                return;
            }

            var item = Item.Instantiate(itemData, creature.transform.position, creature.transform.rotation);
            if (item == null) return;

            // Put in hand or holster
            var hand = isPrimary ? creature.ragdoll.GetHand(Side.Right) : creature.ragdoll.GetHand(Side.Left);
            if (hand != null)
            {
                hand.Grab(item.GetComponent<Item>());
            }
        }

        private void ApplyBehaviorProfile(Creature creature, NPCPreset preset)
        {
            // Add custom AI behavior component
            var behavior = creature.gameObject.AddComponent<JWA_NPCBehavior>();
            behavior.Initialize(preset);
        }

        private void OnNPCDied(Creature creature)
        {
            activeJWANPCs.Remove(creature);

            // Clean up equipment
            var equipSystem = JWA_EquipmentSystem.Instance;
            if (equipSystem != null)
            {
                equipSystem.ClearCreatureEquipment(creature);
            }
        }

        // ========================================================================
        // PUBLIC API
        // ========================================================================

        public int ActiveNPCCount => activeJWANPCs.Count;
        public List<Creature> ActiveNPCs => new List<Creature>(activeJWANPCs);

        public void DespawnAllNPCs()
        {
            foreach (var npc in activeJWANPCs)
            {
                if (npc != null) npc.Despawn();
            }
            activeJWANPCs.Clear();
        }
    }

    // ========================================================================
    // NPC DATA CLASSES
    // ========================================================================

    public enum NPCArchetype
    {
        Assassin,
        HeavyGunner,
        Tactical,
        Cleaner,
        Enforcer
    }

    public enum NPCDifficulty
    {
        Standard,
        Veteran,
        Elite,
        Heavy,
        Boss
    }

    public enum NPCBehaviorProfile
    {
        Assassin,      // Dual wield, flank, high mobility
        HeavyGunner,   // Hold position, suppress, slow advance
        Tactical,      // Cover, coordinated, switch weapons
        Cleaner,       // Stealth, silent kills, avoid detection
        Enforcer,      // Rush, melee, shotgun close range
        Boss           // All behaviors, adaptive, call reinforcements
    }

    [System.Serializable]
    public class NPCPreset
    {
        public string id;
        public string name;
        public string description;
        public NPCArchetype archetype;
        public NPCDifficulty difficulty;
        public float healthMultiplier = 1f;
        public float damageMultiplier = 1f;
        public float speedMultiplier = 1f;
        public float accuracyMultiplier = 1f;
        public float reactionTimeMultiplier = 1f;
        public string suitId;
        public string[] equipmentIds;
        public string primaryWeaponId;
        public string secondaryWeaponId;
        public string[] throwableIds;
        public int throwableCount;
        public NPCBehaviorProfile behaviorProfile;
        public int spawnWeight = 10;
        public int minPlayerLevel = 1;
        public bool isBoss = false;
        public string bossTitle = "";
    }

    // ========================================================================
    // NPC BEHAVIOR COMPONENT
    // ========================================================================

    public class JWA_NPCBehavior : MonoBehaviour
    {
        private NPCPreset preset;
        private Creature creature;
        private BrainModule brain;
        private float lastBehaviorSwitch = 0f;
        private NPCBehaviorState currentState = NPCBehaviorState.Idle;

        public void Initialize(NPCPreset npcPreset)
        {
            preset = npcPreset;
            creature = GetComponent<Creature>();
            brain = creature?.brain;

            // Subscribe to events
            if (creature != null)
            {
                creature.OnDamageEvent += OnDamaged;
                creature.OnKillEvent += OnKill;
            }

            // Initialize brain modules based on profile
            SetupBrainModules();
        }

        private void SetupBrainModules()
        {
            if (brain == null) return;

            switch (preset.behaviorProfile)
            {
                case NPCBehaviorProfile.Assassin:
                    // High mobility, flanking, dual wield
                    ConfigureAssassinBrain();
                    break;
                case NPCBehaviorProfile.HeavyGunner:
                    // Hold ground, suppress
                    ConfigureHeavyGunnerBrain();
                    break;
                case NPCBehaviorProfile.Tactical:
                    // Cover, coordinate, weapon switching
                    ConfigureTacticalBrain();
                    break;
                case NPCBehaviorProfile.Cleaner:
                    // Stealth, silent takedowns
                    ConfigureCleanerBrain();
                    break;
                case NPCBehaviorProfile.Enforcer:
                    // Rush, close range, melee
                    ConfigureEnforcerBrain();
                    break;
                case NPCBehaviorProfile.Boss:
                    // Adaptive, all behaviors
                    ConfigureBossBrain();
                    break;
            }
        }

        private void ConfigureAssassinBrain()
        {
            // Increase movement speed, decrease cover usage, increase flank chance
            creature.SetData("JWA_FlankChance", 0.7f);
            creature.SetData("JWA_CoverUsage", 0.2f);
            creature.SetData("JWA_DualWieldPreference", 0.9f);
            creature.SetData("JWA_EngageDistance", 15f);
        }

        private void ConfigureHeavyGunnerBrain()
        {
            // Decrease movement, increase cover, suppress fire
            creature.SetData("JWA_FlankChance", 0.1f);
            creature.SetData("JWA_CoverUsage", 0.8f);
            creature.SetData("JWA_SuppressFire", 0.9f);
            creature.SetData("JWA_EngageDistance", 25f);
        }

        private void ConfigureTacticalBrain()
        {
            // Balanced, use cover, switch weapons by range
            creature.SetData("JWA_FlankChance", 0.4f);
            creature.SetData("JWA_CoverUsage", 0.6f);
            creature.SetData("JWA_WeaponSwitchDistance", true);
            creature.SetData("JWA_EngageDistance", 30f);
        }

        private void ConfigureCleanerBrain()
        {
            // Stealth, avoid detection, silent weapons
            creature.SetData("JWA_StealthPriority", 0.9f);
            creature.SetData("JWA_SilencedWeaponsOnly", true);
            creature.SetData("JWA_AvoidAlerts", true);
            creature.SetData("JWA_EngageDistance", 20f);
        }

        private void ConfigureEnforcerBrain()
        {
            // Rush, melee, close range
            creature.SetData("JWA_RushChance", 0.8f);
            creature.SetData("JWA_MeleePriority", 0.7f);
            creature.SetData("JWA_EngageDistance", 10f);
        }

        private void ConfigureBossBrain()
        {
            // Adaptive - switches based on situation
            creature.SetData("JWA_AdaptiveAI", true);
            creature.SetData("JWA_CallReinforcements", true);
            creature.SetData("JWA_ReinforcementCooldown", 30f);
            creature.SetData("JWA_PhaseThresholds", new float[] { 0.75f, 0.5f, 0.25f });
        }

        private void Update()
        {
            if (creature == null || creature.isDead) return;

            // Handle behavior state machine
            UpdateBehaviorState();

            // Boss phase transitions
            if (preset.isBoss)
            {
                CheckBossPhases();
            }
        }

        private void UpdateBehaviorState()
        {
            var target = brain?.targetCreature;
            if (target == null)
            {
                currentState = NPCBehaviorState.Idle;
                return;
            }

            float distance = Vector3.Distance(creature.transform.position, target.transform.position);

            // State transitions based on distance and profile
            switch (preset.behaviorProfile)
            {
                case NPCBehaviorProfile.Assassin:
                    if (distance > 20f) currentState = NPCBehaviorState.Approach;
                    else if (distance > 8f) currentState = NPCBehaviorState.Flank;
                    else currentState = NPCBehaviorState.Engage;
                    break;
                case NPCBehaviorProfile.HeavyGunner:
                    if (distance > 30f) currentState = NPCBehaviorState.Approach;
                    else currentState = NPCBehaviorState.Suppress;
                    break;
                case NPCBehaviorProfile.Cleaner:
                    if (distance > 25f) currentState = NPCBehaviorState.Stalk;
                    else if (distance > 10f) currentState = NPCBehaviorState.SilentApproach;
                    else currentState = NPCBehaviorState.SilentTakedown;
                    break;
                case NPCBehaviorProfile.Enforcer:
                    if (distance > 15f) currentState = NPCBehaviorState.Rush;
                    else currentState = NPCBehaviorState.Melee;
                    break;
                default:
                    if (distance > 15f) currentState = NPCBehaviorState.Approach;
                    else currentState = NPCBehaviorState.Engage;
                    break;
            }
        }

        private void CheckBossPhases()
        {
            var health = creature.GetComponent<CreatureHealth>();
            if (health == null) return;

            float healthPercent = health.currentHealth / health.maxHealth;
            var thresholds = creature.GetData<float[]>("JWA_PhaseThresholds");
            if (thresholds == null) return;

            int currentPhase = creature.GetData<int>("JWA_CurrentPhase");
            int newPhase = 0;
            for (int i = 0; i < thresholds.Length; i++)
            {
                if (healthPercent <= thresholds[i]) newPhase = i + 1;
            }

            if (newPhase > currentPhase)
            {
                creature.SetData("JWA_CurrentPhase", newPhase);
                OnBossPhaseChange(newPhase);
            }
        }

        private void OnBossPhaseChange(int phase)
        {
            switch (phase)
            {
                case 1:
                    // Phase 1: Standard tactics
                    creature.SetData("JWA_DamageMultiplier", preset.damageMultiplier * 1.2f);
                    creature.SetData("JWA_SpeedMultiplier", preset.speedMultiplier * 1.1f);
                    break;
                case 2:
                    // Phase 2: More aggressive, call reinforcements
                    creature.SetData("JWA_DamageMultiplier", preset.damageMultiplier * 1.4f);
                    creature.SetData("JWA_CallReinforcements", true);
                    break;
                case 3:
                    // Phase 3: Desperation - full auto, rush
                    creature.SetData("JWA_DamageMultiplier", preset.damageMultiplier * 1.6f);
                    creature.SetData("JWA_RushChance", 0.9f);
                    creature.SetData("JWA_FullAuto", true);
                    break;
            }
        }

        private void OnDamaged(Creature attacker, DamageStruct damage)
        {
            // React to damage based on profile
            if (preset.behaviorProfile == NPCBehaviorProfile.Cleaner)
            {
                // Cleaners try to break contact when hit
                creature.SetData("JWA_BreakContact", true);
            }
            else if (preset.behaviorProfile == NPCBehaviorProfile.Enforcer)
            {
                // Enforcers get angrier
                creature.SetData("JWA_RageMeter", creature.GetData<float>("JWA_RageMeter") + damage.damage * 0.1f);
            }
        }

        private void OnKill(Creature victim)
        {
            // Track kills for behavior adaptation
            int kills = creature.GetData<int>("JWA_KillCount") + 1;
            creature.SetData("JWA_KillCount", kills);

            if (preset.isBoss && kills % 3 == 0)
            {
                // Boss taunts or calls reinforcements every 3 kills
                creature.SetData("JWA_CallReinforcements", true);
            }
        }

        private void OnDestroy()
        {
            if (creature != null)
            {
                creature.OnDamageEvent -= OnDamaged;
                creature.OnKillEvent -= OnKill;
            }
        }

        private enum NPCBehaviorState
        {
            Idle,
            Approach,
            Flank,
            Engage,
            Suppress,
            Stalk,
            SilentApproach,
            SilentTakedown,
            Rush,
            Melee,
            BreakContact
        }
    }
}