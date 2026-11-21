using UnityEngine;
using System.Collections.Generic;

namespace JohnWickArsenal.Gameplay
{
    /// <summary>
    /// Centralized gameplay configuration for John Wick Arsenal.
    /// All tweakable values in one place for easy balancing and Quest optimization.
    /// </summary>
    [CreateAssetMenu(fileName = "JWA_GameplayConfig", menuName = "John Wick Arenal/Gameplay Config")]
    public class JWA_GameplayConfig : ScriptableObject
    {
        [Header("=== GLOBAL SETTINGS ===")]
        [Tooltip("Master enable/disable for all JWA features")]
        public bool enableMod = true;

        [Tooltip("Enable debug logging")]
        public bool debugMode = false;

        [Tooltip("Enable cinematic features (slow-mo, camera shake, etc.)")]
        public bool cinematicFeatures = true;

        [Header("=== PERFORMANCE (QUEST OPTIMIZATION) ===")]
        [Tooltip("Max active particle systems simultaneously")]
        [Range(10, 200)] public int maxParticles = 50;

        [Tooltip("Max active audio sources")]
        [Range(8, 64)] public int maxAudioSources = 24;

        [Tooltip("Physics update rate for NPCs (Hz)")]
        [Range(15, 60)] public int npcPhysicsRate = 30;

        [Tooltip("Enable LOD switching for weapons at distance")]
        public bool weaponLODs = true;

        [Tooltip("Distance to switch to low-LOD weapon model")]
        [Range(5f, 50f)] public float lodDistance = 15f;

        [Tooltip("Disable weapon visual effects beyond this distance")]
        [Range(10f, 100f)] public float vfxCullDistance = 30f;

        [Tooltip("Max simultaneous NPCs with JWA behaviors")]
        [Range(5, 30)] public int maxJWANPCs = 15;

        [Header("=== WEAPON BALANCE ===")]
        [Tooltip("Global damage multiplier for all JWA weapons")]
        [Range(0.1f, 3f)] public float globalDamageMultiplier = 1f;

        [Tooltip("Global recoil multiplier")]
        [Range(0.1f, 3f)] public float globalRecoilMultiplier = 1f;

        [Tooltip("Global fire rate multiplier")]
        [Range(0.1f, 3f)] public float globalFireRateMultiplier = 1f;

        [Tooltip("Global reload speed multiplier")]
        [Range(0.1f, 3f)] public float globalReloadSpeedMultiplier = 1f;

        [Tooltip("Enable movie-accurate damage (headshots = instant kill on unarmored)")]
        public bool movieAccurateDamage = true;

        [Tooltip("Penetration enabled for rifle rounds")]
        public bool riflePenetration = true;

        [Tooltip("Shotgun pellet count multiplier")]
        [Range(0.5f, 2f)] public float shotgunPelletMultiplier = 1f;

        [Header("=== PISTOL SPECIFIC ===")]
        [Tooltip("P30L compensator recoil reduction")]
        [Range(0f, 0.8f)] public float p30lCompensatorReduction = 0.3f;

        [Tooltip("Glock 34 trigger pull weight (lbs)")]
        [Range(2f, 8f)] public float glock34TriggerPull = 3.5f;

        [Tooltip("STI 2011 compensator recoil reduction")]
        [Range(0f, 0.8f)] public float sti2011CompensatorReduction = 0.4f;

        [Tooltip("STI 2011 trigger pull weight (lbs)")]
        [Range(1.5f, 5f)] public float sti2011TriggerPull = 2.5f;

        [Tooltip("Pit Viper compensator recoil reduction")]
        [Range(0f, 0.8f)] public float pitViperCompensatorReduction = 0.45f;

        [Tooltip("Pit Viper trigger pull weight (lbs)")]
        [Range(1f, 4f)] public float pitViperTriggerPull = 2f;

        [Header("=== SHOTGUN SPECIFIC ===")]
        [Tooltip("Benelli M4 ARGO system effectiveness")]
        [Range(0f, 1f)] public float benelliARGOEffectiveness = 0.7f;

        [Tooltip("KSG dual tube switch speed (seconds)")]
        [Range(0.1f, 2f)] public float ksgTubeSwitchSpeed = 0.5f;

        [Tooltip("Shell carrier reload speed multiplier")]
        [Range(0.5f, 3f)] public float shellCarrierReloadSpeed = 1.2f;

        [Header("=== REVOLVER SPECIFIC ===")]
        [Tooltip("Remington 1875 hammer cock speed")]
        [Range(50f, 300f)] public float remingtonHammerSpeed = 180f;

        [Tooltip("Loading gate open angle (degrees)")]
        [Range(45f, 180f)] public float remingtonGateAngle = 90f;

        [Tooltip("Ejector rod travel distance (meters)")]
        [Range(0.02f, 0.1f)] public float remingtonEjectorTravel = 0.05f;

        [Tooltip("Black powder smoke amount")]
        [Range(0f, 1f)] public float remingtonSmokeAmount = 0.5f;

        [Header("=== RIFLE SPECIFIC ===")]
        [Tooltip("TR-1 weight reduction handling bonus")]
        [Range(0f, 0.6f)] public float tr1WeightReduction = 0.35f;

        [Tooltip("TR-1 lightweight BCG recoil reduction")]
        [Range(0f, 0.5f)] public float tr1LightweightBCGReduction = 0.12f;

        [Tooltip("TR-1 adjustable gas: suppressed recoil multiplier")]
        [Range(0.3f, 1f)] public float tr1GasSuppressed = 0.6f;

        [Tooltip("TR-1 adjustable gas: normal recoil multiplier")]
        [Range(0.8f, 1.2f)] public float tr1GasNormal = 1f;

        [Tooltip("TR-1 adjustable gas: high recoil multiplier")]
        [Range(1f, 1.5f)] public float tr1GasHigh = 1.15f;

        [Tooltip("TR-1 adjustable gas: adverse recoil multiplier")]
        [Range(1.1f, 2f)] public float tr1GasAdverse = 1.3f;

        [Header("=== THROWABLES ===")]
        [Tooltip("Throwing knife headshot damage multiplier")]
        [Range(1f, 5f)] public float knifeHeadshotMultiplier = 3f;

        [Tooltip("Throwing knife neck damage multiplier")]
        [Range(1f, 5f)] public float knifeNeckMultiplier = 2.5f;

        [Tooltip("Throwing knife stick probability")]
        [Range(0f, 1f)] public float knifeStickProbability = 0.95f;

        [Tooltip("Pencil instant kill on head/neck")]
        public bool pencilInstantKill = true;

        [Tooltip("Pencil penetration depth (meters)")]
        [Range(0.05f, 0.3f)] public float pencilPenetrationDepth = 0.15f;

        [Tooltip("Pencil max kills before breaking")]
        [Range(1, 10)] public int pencilMaxKills = 3;

        [Tooltip("Pencil throw force")]
        [Range(10f, 50f)] public float pencilThrowForce = 25f;

        [Tooltip("Pencil spin rate (degrees/sec)")]
        [Range(360f, 2000f)] public float pencilSpinRate = 1080f;

        [Header("=== EQUIPMENT ===")]
        [Tooltip("Suit armor damage reduction is applied multiplicatively")]
        public bool suitArmorMultiplicative = true;

        [Tooltip("Max suit armor damage reduction cap")]
        [Range(0f, 0.9f)] public float maxSuitArmorReduction = 0.5f;

        [Tooltip("Holster draw speed multiplier")]
        [Range(0.5f, 2f)] public float holsterDrawSpeed = 1.2f;

        [Tooltip("Pouch reload speed bonus")]
        [Range(0f, 1f)] public float pouchReloadBonus = 0.15f;

        [Tooltip("Equipment weight affects movement speed")]
        public bool equipmentWeightAffectsSpeed = true;

        [Tooltip("Movement speed reduction per kg of equipment")]
        [Range(0f, 0.1f)] public float speedReductionPerKg = 0.02f;

        [Header("=== NPC SETTINGS ===")]
        [Tooltip("NPC base health multiplier")]
        [Range(0.5f, 5f)] public float npcBaseHealthMultiplier = 1f;

        [Tooltip("NPC base damage multiplier")]
        [Range(0.5f, 3f)] public float npcBaseDamageMultiplier = 1f;

        [Tooltip("NPC accuracy scaling with difficulty")]
        [Range(0f, 2f)] public float npcAccuracyScaling = 0.2f;

        [Tooltip("NPC reaction time scaling with difficulty")]
        [Range(0f, 1f)] public float npcReactionScaling = 0.15f;

        [Tooltip("Boss health multiplier")]
        [Range(2f, 10f)] public float bossHealthMultiplier = 4f;

        [Tooltip("Reinforcement spawn cooldown (seconds)")]
        [Range(10f, 120f)] public float reinforcementCooldown = 30f;

        [Tooltip("Max reinforcements per boss")]
        [Range(0, 10)] public int maxReinforcements = 4;

        [Header("=== CINEMATIC ===")]
        [Tooltip("Enable slow-mo on final kill")]
        public bool slowmoOnFinalKill = true;

        [Tooltip("Slow-mo duration (seconds)")]
        [Range(0.5f, 5f)] public float slowmoDuration = 1.5f;

        [Tooltip("Slow-mo time scale")]
        [Range(0.05f, 0.5f)] public float slowmoTimeScale = 0.15f;

        [Tooltip("Camera shake on heavy impacts")]
        public bool cameraShake = true;

        [Tooltip("Camera shake intensity")]
        [Range(0f, 1f)] public float cameraShakeIntensity = 0.6f;

        [Tooltip("Bullet time on perfect parry/dodge")]
        public bool bulletTimeOnParry = true;

        [Tooltip("Bullet time duration (seconds)")]
        [Range(0.2f, 2f)] public float bulletTimeDuration = 0.5f;

        [Header("=== AUDIO ===")]
        [Tooltip("Global volume for JWA weapons")]
        [Range(0f, 2f)] public float weaponVolume = 1f;

        [Tooltip("Suppressed weapon volume multiplier")]
        [Range(0f, 1f)] public float suppressedVolumeMultiplier = 0.3f;

        [Tooltip("Enable dynamic range compression for Quest")]
        public bool dynamicRangeCompression = true;

        [Tooltip("Max simultaneous weapon fire sounds")]
        [Range(2, 16)] public int maxFireSounds = 8;

        [Header("=== VISUAL EFFECTS ===")]
        [Tooltip("Muzzle flash intensity")]
        [Range(0f, 2f)] public float muzzleFlashIntensity = 1f;

        [Tooltip("Shell ejection force")]
        [Range(0.5f, 5f)] public float shellEjectionForce = 2f;

        [Tooltip("Shell casing lifetime (seconds)")]
        [Range(5f, 60f)] public float shellCasingLifetime = 15f;

        [Tooltip("Blood particle amount multiplier")]
        [Range(0f, 2f)] public float bloodParticleMultiplier = 1f;

        [Tooltip("Enable compensator gas particles")]
        public bool compensatorGasParticles = true;

        [Tooltip("Enable TR-1 gas port particles")]
        public bool tr1GasPortParticles = true;

        [Header("=== PROGRESSION ===")]
        [Tooltip("Enable weapon mastery/unlock system")]
        public bool weaponMastery = false;

        [Tooltip("Kills required for weapon mastery")]
        [Range(10, 500)] public int masteryKillsRequired = 100;

        [Tooltip("Mastery reward: damage bonus")]
        [Range(0f, 0.5f)] public float masteryDamageBonus = 0.1f;

        [Tooltip("Mastery reward: handling bonus")]
        [Range(0f, 0.5f)] public float masteryHandlingBonus = 0.1f;

        [Header("=== ACCESSIBILITY ===")]
        [Tooltip("Reduce screen shake for motion sensitivity")]
        public bool reduceScreenShake = false;

        [Tooltip("Simplified reloading (auto-reload)")]
        public bool simplifiedReloading = false;

        [Tooltip("Auto-aim assist for throwing knives")]
        public bool throwingAssist = false;

        [Tooltip("Highlight interactive objects")]
        public bool highlightInteractables = true;

        // ========================================================================
        // SINGLETON ACCESS
        // ========================================================================

        private static JWA_GameplayConfig instance;
        public static JWA_GameplayConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<JWA_GameplayConfig>("JWA_GameplayConfig");
                    if (instance == null)
                    {
                        instance = CreateInstance<JWA_GameplayConfig>();
                        instance.name = "JWA_GameplayConfig";
                    }
                }
                return instance;
            }
        }

        // ========================================================================
        // VALIDATION
        // ========================================================================

        private void OnValidate()
        {
            // Clamp values to valid ranges
            globalDamageMultiplier = Mathf.Max(0.1f, globalDamageMultiplier);
            globalRecoilMultiplier = Mathf.Max(0.1f, globalRecoilMultiplier);
            globalFireRateMultiplier = Mathf.Max(0.1f, globalFireRateMultiplier);
            globalReloadSpeedMultiplier = Mathf.Max(0.1f, globalReloadSpeedMultiplier);

            maxSuitArmorReduction = Mathf.Clamp01(maxSuitArmorReduction);
            suppressedVolumeMultiplier = Mathf.Clamp01(suppressedVolumeMultiplier);
        }

        // ========================================================================
        // HELPER METHODS
        // ========================================================================

        public float GetPistolCompensatorReduction(string weaponId)
        {
            switch (weaponId)
            {
                case "jwa_hk_p30l": return p30lCompensatorReduction;
                case "jwa_sti_2011_combat_master": return sti2011CompensatorReduction;
                case "jwa_tti_pit_viper": return pitViperCompensatorReduction;
                default: return 0f;
            }
        }

        public float GetPistolTriggerPull(string weaponId)
        {
            switch (weaponId)
            {
                case "jwa_glock_34": return glock34TriggerPull;
                case "jwa_sti_2011_combat_master": return sti2011TriggerPull;
                case "jwa_tti_pit_viper": return pitViperTriggerPull;
                default: return 5.5f;
            }
        }

        public float GetTR1GasSettingRecoil(int gasSetting)
        {
            switch (gasSetting)
            {
                case 0: return tr1GasSuppressed;   // Suppressed
                case 1: return tr1GasNormal;       // Normal
                case 2: return tr1GasHigh;         // High
                case 3: return tr1GasAdverse;      // Adverse
                default: return tr1GasNormal;
            }
        }

        public float CalculateEquipmentSpeedPenalty(float totalWeightKg)
        {
            if (!equipmentWeightAffectsSpeed) return 1f;
            return Mathf.Max(0.5f, 1f - (totalWeightKg * speedReductionPerKg));
        }

        public float CalculateSuitArmorReduction(float suitArmorBonus)
        {
            float reduction = Mathf.Min(suitArmorBonus, maxSuitArmorReduction);
            return suitArmorMultiplicative ? reduction : reduction; // Both same for now, can expand
        }
    }

    /// <summary>
    /// Runtime config manager for accessing settings and applying to weapons/NPCs
    /// </summary>
    public class JWA_ConfigManager : MonoBehaviour
    {
        public static JWA_ConfigManager Instance { get; private set; }
        public JWA_GameplayConfig Config => JWA_GameplayConfig.Instance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Apply global settings
            ApplyGlobalSettings();
        }

        private void ApplyGlobalSettings()
        {
            var config = Config;

            // Physics settings
            if (config.dynamicRangeCompression)
            {
                AudioListener.volume = Mathf.Min(AudioListener.volume, 1f);
            }

            // Quality settings for Quest
            if (config.weaponLODs)
            {
                // LOD bias would be set per-weapon
            }
        }

        public void ApplyToWeapon(GameObject weapon, string weaponId)
        {
            var config = Config;
            var gun = weapon.GetComponent<Gun>();
            if (gun == null) return;

            // Apply global multipliers
            gun.damageMultiplier *= config.globalDamageMultiplier;
            gun.recoilMultiplier *= config.globalRecoilMultiplier;
            gun.cyclicRateMultiplier *= config.globalFireRateMultiplier;
            gun.reloadSpeedMultiplier *= config.globalReloadSpeedMultiplier;

            // Weapon-specific
            if (weaponId.Contains("p30l") || weaponId.Contains("sti") || weaponId.Contains("pitviper"))
            {
                float compReduction = config.GetPistolCompensatorReduction(weaponId);
                gun.recoilMultiplier *= (1f - compReduction);
            }

            if (weaponId.Contains("tr1"))
            {
                gun.recoilMultiplier *= (1f - config.tr1LightweightBCGReduction);
                gun.handlingMultiplier *= (1f + config.tr1WeightReduction);
            }
        }

        public void ApplyToNPC(Creature npc, string presetId)
        {
            var config = Config;
            var health = npc.GetComponent<CreatureHealth>();
            if (health != null)
            {
                health.maxHealth *= config.npcBaseHealthMultiplier;
                health.currentHealth = health.maxHealth;
            }

            npc.SetData("JWA_DamageMultiplier", config.npcBaseDamageMultiplier);
        }

        public void SetQualityLevel(int level)
        {
            // 0 = Low (Quest 1), 1 = Medium (Quest 2), 2 = High (Quest 3/Pro)
            var config = Config;
            switch (level)
            {
                case 0: // Low
                    config.maxParticles = 25;
                    config.maxAudioSources = 16;
                    config.muzzleFlashIntensity = 0.5f;
                    config.bloodParticleMultiplier = 0.5f;
                    config.compensatorGasParticles = false;
                    config.tr1GasPortParticles = false;
                    config.cameraShake = false;
                    break;
                case 1: // Medium
                    config.maxParticles = 50;
                    config.maxAudioSources = 24;
                    config.muzzleFlashIntensity = 1f;
                    config.bloodParticleMultiplier = 1f;
                    config.compensatorGasParticles = true;
                    config.tr1GasPortParticles = true;
                    config.cameraShake = true;
                    break;
                case 2: // High
                    config.maxParticles = 100;
                    config.maxAudioSources = 32;
                    config.muzzleFlashIntensity = 1.5f;
                    config.bloodParticleMultiplier = 1.5f;
                    config.compensatorGasParticles = true;
                    config.tr1GasPortParticles = true;
                    config.cameraShake = true;
                    break;
            }
        }
    }
}