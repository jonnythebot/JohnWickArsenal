using UnityEngine;
using BladeAndSorcery.Gameplay;

namespace JohnWickArsenal.Behaviors
{
    /// <summary>
    /// Custom behavior for John Wick pistols - adds compensator effects, tactical reloads, and competition features
    /// </summary>
    public class JohnWickPistolBehavior : WeaponBehavior
    {
        [Header("John Wick Pistol Settings")]
        [SerializeField] private bool hasCompensator = true;
        [SerializeField] private float compensatorRecoilReduction = 0.3f;
        [SerializeField] private bool hasRedDot = false;
        [SerializeField] private bool isRaceGun = false;
        [SerializeField] private float matchTriggerPull = 2.5f; // lbs

        [Header("Tactical Reload")]
        [SerializeField] private bool enableTacticalReload = true;
        [SerializeField] private float tacticalReloadSpeedMultiplier = 1.3f;

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem compensatorGasPorts;
        [SerializeField] private GameObject redDotReticle;
        [SerializeField] private Material goldAccentMaterial;

        private Weapon weapon;
        private Gun gun;
        private Magazine magazine;
        private bool wasEmpty = false;

        protected override void Awake()
        {
            base.Awake();
            weapon = GetComponent<Weapon>();
            gun = GetComponent<Gun>();
            magazine = GetComponentInChildren<Magazine>();

            if (goldAccentMaterial != null)
            {
                ApplyGoldAccents();
            }
        }

        protected override void OnHeld(RagdollHand hand)
        {
            base.OnHeld(hand);
            wasEmpty = false;

            if (hasRedDot && redDotReticle != null)
            {
                redDotReticle.SetActive(true);
            }
        }

        protected override void OnReleased(RagdollHand hand)
        {
            base.OnReleased(hand);

            if (hasRedDot && redDotReticle != null)
            {
                redDotReticle.SetActive(false);
            }
        }

        protected override void OnFire()
        {
            base.OnFire();

            // Compensator gas effect
            if (hasCompensator && compensatorGasPorts != null)
            {
                compensatorGasPorts.Emit(1);
            }

            // Track empty reload state
            if (magazine != null && magazine.CurrentAmmoCount == 0)
            {
                wasEmpty = true;
            }
        }

        protected override void OnReloadStart()
        {
            base.OnReloadStart();

            // Tactical reload speed boost
            if (enableTacticalReload && magazine != null && magazine.CurrentAmmoCount > 0)
            {
                weapon.reloadSpeedMultiplier *= tacticalReloadSpeedMultiplier;
            }
        }

        protected override void OnReloadEnd()
        {
            base.OnReloadEnd();
            weapon.reloadSpeedMultiplier = 1f;
            wasEmpty = false;
        }

        public override float GetRecoilMultiplier()
        {
            float multiplier = base.GetRecoilMultiplier();

            if (hasCompensator)
            {
                multiplier *= (1f - compensatorRecoilReduction);
            }

            // Race guns have additional recoil control
            if (isRaceGun)
            {
                multiplier *= 0.85f;
            }

            return multiplier;
        }

        public override float GetSpreadMultiplier()
        {
            float multiplier = base.GetSpreadMultiplier();

            if (isRaceGun)
            {
                multiplier *= 0.75f; // Match-grade barrel
            }

            return multiplier;
        }

        private void ApplyGoldAccents()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                var materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i].name.Contains("Gold") || materials[i].name.Contains("Accent"))
                    {
                        materials[i] = goldAccentMaterial;
                    }
                }
                renderer.materials = materials;
            }
        }

        // Public API for UI/inspection
        public bool HasCompensator => hasCompensator;
        public bool HasRedDot => hasRedDot;
        public bool IsRaceGun => isRaceGun;
        public float TriggerPullWeight => matchTriggerPull;
    }
}