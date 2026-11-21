using UnityEngine;
using BladeAndSorcery.Gameplay;

namespace JohnWickArsenal.Behaviors
{
    /// <summary>
    /// Custom behavior for TTI TR-1 Ultralight - skeletonized AR, adjustable gas, lightweight BCG
    /// </summary>
    public class JohnWickRifleBehavior : WeaponBehavior
    {
        [Header("TR-1 Ultralight Features")]
        [SerializeField] private bool hasSkeletonizedReceiver = true;
        [SerializeField] private bool hasCarbonFiberHandguard = true;
        [SerializeField] private bool hasAdjustableGasBlock = true;
        [SerializeField] private bool hasLightweightBCG = true;
        [SerializeField] private bool hasBinaryTrigger = false;
        [SerializeField] private bool hasRedDot = true;

        [Header("Gas System")]
        [SerializeField] private GasSetting currentGasSetting = GasSetting.Normal;
        [SerializeField] private KeyCode gasAdjustKey = KeyCode.G;

        [Header("Visual")]
        [SerializeField] private GameObject redDotSight;
        [SerializeField] private Transform gasBlockDial;
        [SerializeField] private ParticleSystem[] gasPortParticles;

        [Header("Weight Reduction")]
        [SerializeField] private float weightReduction = 0.35f; // 35% lighter than standard AR

        private Gun gun;
        private Magazine magazine;
        private bool isBinaryMode = false;

        public enum GasSetting
        {
            Suppressed = 0,  // Low gas for suppressor
            Normal = 1,      // Standard operation
            High = 2,        // High gas for dirty/underpowered ammo
            Adverse = 3      // Maximum gas for adverse conditions
        }

        protected override void Awake()
        {
            base.Awake();
            gun = GetComponent<Gun>();
            magazine = GetComponentInChildren<Magazine>();

            if (hasRedDot && redDotSight != null)
            {
                redDotSight.SetActive(true);
            }

            ApplyGasSetting(currentGasSetting);
        }

        protected override void Update()
        {
            base.Update();

            if (hasAdjustableGasBlock && Input.GetKeyDown(gasAdjustKey))
            {
                CycleGasSetting();
            }
        }

        protected override void OnFire()
        {
            base.OnFire();

            // Gas port visual effect
            if (gasPortParticles != null)
            {
                foreach (var ps in gasPortParticles)
                {
                    if (ps != null) ps.Emit(1);
                }
            }

            // Binary trigger simulation
            if (hasBinaryTrigger && isBinaryMode)
            {
                // Binary fires on pull AND release
                // This would require input system integration
            }
        }

        protected override void OnHeld(RagdollHand hand)
        {
            base.OnHeld(hand);

            if (hasRedDot && redDotSight != null)
            {
                redDotSight.SetActive(true);
            }
        }

        protected override void OnReleased(RagdollHand hand)
        {
            base.OnReleased(hand);

            if (hasRedDot && redDotSight != null)
            {
                redDotSight.SetActive(false);
            }
        }

        private void CycleGasSetting()
        {
            int nextSetting = ((int)currentGasSetting + 1) % 4;
            currentGasSetting = (GasSetting)nextSetting;
            ApplyGasSetting(currentGasSetting);

            // Visual feedback on gas block
            if (gasBlockDial != null)
            {
                float angle = nextSetting * 90f;
                gasBlockDial.localEulerAngles = new Vector3(0, 0, angle);
            }

            Debug.Log($"[JWA TR-1] Gas setting: {currentGasSetting}");
        }

        private void ApplyGasSetting(GasSetting setting)
        {
            switch (setting)
            {
                case GasSetting.Suppressed:
                    gun.recoilMultiplier = 0.6f;
                    gun.cyclicRateMultiplier = 0.9f;
                    gun.reliability = 0.85f;
                    break;
                case GasSetting.Normal:
                    gun.recoilMultiplier = 1.0f;
                    gun.cyclicRateMultiplier = 1.0f;
                    gun.reliability = 1.0f;
                    break;
                case GasSetting.High:
                    gun.recoilMultiplier = 1.15f;
                    gun.cyclicRateMultiplier = 1.05f;
                    gun.reliability = 1.1f;
                    break;
                case GasSetting.Adverse:
                    gun.recoilMultiplier = 1.3f;
                    gun.cyclicRateMultiplier = 1.1f;
                    gun.reliability = 1.2f;
                    break;
            }
        }

        public override float GetRecoilMultiplier()
        {
            float multiplier = base.GetRecoilMultiplier();

            // Lightweight BCG reduces reciprocating mass
            if (hasLightweightBCG)
            {
                multiplier *= 0.88f;
            }

            // Skeletonized receiver slightly increases felt recoil but reduces weight
            if (hasSkeletonizedReceiver)
            {
                multiplier *= 1.05f;
            }

            return multiplier;
        }

        public override float GetHandlingMultiplier()
        {
            // Significant handling improvement from weight reduction
            return base.GetHandlingMultiplier() * (1f + weightReduction);
        }

        public override float GetADSSpeedMultiplier()
        {
            // Faster ADS from lighter weight
            return base.GetADSSpeedMultiplier() * 1.25f;
        }

        public void ToggleBinaryTrigger()
        {
            if (hasBinaryTrigger)
            {
                isBinaryMode = !isBinaryMode;
                Debug.Log($"[JWA TR-1] Binary trigger: {(isBinaryMode ? "ON" : "OFF")}");
            }
        }

        // Public API
        public GasSetting CurrentGasSetting => currentGasSetting;
        public bool IsBinaryMode => isBinaryMode;
        public bool HasAdjustableGas => hasAdjustableGasBlock;
        public float WeightReductionPercent => weightReduction * 100f;
    }
}