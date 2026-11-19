using UnityEngine;
using BladeAndSorcery.Gameplay;

namespace JohnWickArsenal.Behaviors
{
    /// <summary>
    /// Custom behavior for the Remington 1875 - single action, loading gate, ejector rod mechanics
    /// </summary>
    public class JohnWickRevolverBehavior : WeaponBehavior
    {
        [Header("Revolver Mechanics")]
        [SerializeField] private bool isSingleAction = true;
        [SerializeField] private bool hasLoadingGate = true;
        [SerializeField] private bool hasEjectorRod = true;
        [SerializeField] private Transform cylinder;
        [SerializeField] private Transform loadingGate;
        [SerializeField] private Transform ejectorRod;
        [SerializeField] private Transform hammer;

        [Header("Loading Gate")]
        [SerializeField] private float loadingGateOpenAngle = 90f;
        [SerializeField] private float loadingGateSpeed = 180f;

        [Header("Ejector Rod")]
        [SerializeField] private float ejectorRodTravel = 0.05f;
        [SerializeField] private float ejectorSpeed = 2f;

        [Header("Antique Visuals")]
        [SerializeField] private Material caseHardenedMaterial;
        [SerializeField] private Material bluedSteelMaterial;
        [SerializeField] private ParticleSystem smokeParticles;

        private Gun gun;
        private Magazine magazine;
        private bool isLoadingGateOpen = false;
        private float loadingGateCurrentAngle = 0f;
        private bool isEjecting = false;
        private float ejectorCurrentTravel = 0f;
        private int lastChamberIndex = 0;

        protected override void Awake()
        {
            base.Awake();
            gun = GetComponent<Gun>();
            magazine = GetComponentInChildren<Magazine>();

            ApplyAntiqueFinish();
        }

        protected override void Update()
        {
            base.Update();

            AnimateLoadingGate();
            AnimateEjectorRod();
            AnimateHammer();
        }

        protected override void OnFire()
        {
            base.OnFire();

            // Single action - hammer must be cocked manually or via trigger pull
            if (isSingleAction && hammer != null)
            {
                // Hammer falls
                StartCoroutine(CockHammerRoutine());
            }

            // Smoke effect for black powder feel
            if (smokeParticles != null)
            {
                smokeParticles.Emit(3);
            }

            // Cylinder rotation
            if (cylinder != null && magazine != null)
            {
                RotateCylinder();
            }
        }

        protected override void OnReloadStart()
        {
            base.OnReloadStart();

            if (hasLoadingGate)
            {
                OpenLoadingGate();
            }

            if (hasEjectorRod)
            {
                StartEjectAll();
            }
        }

        protected override void OnReloadEnd()
        {
            base.OnReloadEnd();

            if (hasLoadingGate)
            {
                CloseLoadingGate();
            }
        }

        private void RotateCylinder()
        {
            if (cylinder == null) return;

            int chambers = 6;
            float anglePerChamber = 360f / chambers;
            lastChamberIndex = (lastChamberIndex + 1) % chambers;
            float targetAngle = lastChamberIndex * anglePerChamber;

            StartCoroutine(RotateCylinderRoutine(targetAngle));
        }

        private System.Collections.IEnumerator RotateCylinderRoutine(float targetAngle)
        {
            float startAngle = cylinder.localEulerAngles.y;
            float duration = 0.08f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float angle = Mathf.LerpAngle(startAngle, targetAngle, elapsed / duration);
                cylinder.localEulerAngles = new Vector3(0, angle, 0);
                yield return null;
            }

            cylinder.localEulerAngles = new Vector3(0, targetAngle, 0);
        }

        private void OpenLoadingGate()
        {
            isLoadingGateOpen = true;
        }

        private void CloseLoadingGate()
        {
            isLoadingGateOpen = false;
        }

        private void AnimateLoadingGate()
        {
            if (loadingGate == null) return;

            float targetAngle = isLoadingGateOpen ? loadingGateOpenAngle : 0f;
            loadingGateCurrentAngle = Mathf.MoveTowards(loadingGateCurrentAngle, targetAngle, loadingGateSpeed * Time.deltaTime);
            loadingGate.localEulerAngles = new Vector3(0, 0, loadingGateCurrentAngle);
        }

        private void StartEjectAll()
        {
            isEjecting = true;
            ejectorCurrentTravel = 0f;
        }

        private void AnimateEjectorRod()
        {
            if (ejectorRod == null || !isEjecting) return;

            ejectorCurrentTravel = Mathf.MoveTowards(ejectorCurrentTravel, ejectorRodTravel, ejectorSpeed * Time.deltaTime);
            ejectorRod.localPosition = new Vector3(0, 0, ejectorCurrentTravel);

            if (ejectorCurrentTravel >= ejectorRodTravel)
            {
                // Eject spent casings
                EjectSpentCasings();
                isEjecting = false;
            }
        }

        private void EjectSpentCasings()
        {
            // In a full implementation, this would spawn casing objects
            Debug.Log("[JWA] Ejecting spent casings via ejector rod");
        }

        private System.Collections.IEnumerator CockHammerRoutine()
        {
            if (hammer == null) yield break;

            Vector3 startRot = hammer.localEulerAngles;
            Vector3 cockedRot = startRot + new Vector3(-45, 0, 0); // Cock back
            float duration = 0.1f;
            float elapsed = 0f;

            // Cock back
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                hammer.localEulerAngles = Vector3.Lerp(startRot, cockedRot, elapsed / duration);
                yield return null;
            }

            hammer.localEulerAngles = cockedRot;

            // Fall forward (trigger pull)
            yield return new WaitForSeconds(0.05f);

            elapsed = 0f;
            while (elapsed < duration * 0.5f)
            {
                elapsed += Time.deltaTime;
                hammer.localEulerAngles = Vector3.Lerp(cockedRot, startRot, elapsed / (duration * 0.5f));
                yield return null;
            }

            hammer.localEulerAngles = startRot;
        }

        private void AnimateHammer()
        {
            // Additional hammer animations could go here
        }

        private void ApplyAntiqueFinish()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                var materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    string matName = materials[i].name.ToLower();
                    if (matName.Contains("frame") || matName.Contains("receiver"))
                    {
                        if (caseHardenedMaterial != null)
                            materials[i] = caseHardenedMaterial;
                    }
                    else if (matName.Contains("barrel") || matName.Contains("cylinder") || matName.Contains("hammer"))
                    {
                        if (bluedSteelMaterial != null)
                            materials[i] = bluedSteelMaterial;
                    }
                }
                renderer.materials = materials;
            }
        }

        public override float GetRecoilMultiplier()
        {
            // Single action revolvers have sharp but manageable recoil
            return base.GetRecoilMultiplier() * 1.1f;
        }

        // Public API
        public bool IsSingleAction => isSingleAction;
        public bool LoadingGateOpen => isLoadingGateOpen;
        public int ChambersRemaining => magazine != null ? magazine.CurrentAmmoCount : 0;
    }
}