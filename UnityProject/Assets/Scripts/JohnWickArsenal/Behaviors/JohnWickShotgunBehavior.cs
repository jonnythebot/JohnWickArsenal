using UnityEngine;
using BladeAndSorcery.Gameplay;

namespace JohnWickArsenal.Behaviors
{
    /// <summary>
    /// Custom behavior for John Wick shotguns - dual tube switching, shell carrier, ARGO system
    /// </summary>
    public class JohnWickShotgunBehavior : WeaponBehavior
    {
        [Header("Shotgun Specific")]
        [SerializeField] private bool hasDualTube = false;
        [SerializeField] private bool hasShellCarrier = false;
        [SerializeField] private int shellCarrierCapacity = 6;
        [SerializeField] private bool hasARGO = false; // Auto-Regulating Gas Operation
        [SerializeField] private bool hasGhostRingSights = true;
        [SerializeField] private bool hasCollapsibleStock = true;

        [Header("Dual Tube (KSG)")]
        [SerializeField] private Transform tubeA;
        [SerializeField] private Transform tubeB;
        [SerializeField] private KeyCode tubeSwitchKey = KeyCode.T;

        [Header("Shell Carrier Visual")]
        [SerializeField] private Transform shellCarrierMount;
        [SerializeField] private GameObject shellPrefab;
        private GameObject[] carrierShells;

        private Gun gun;
        private Magazine magazine;
        private int currentTube = 0; // 0 = A, 1 = B
        private int shellsInCarrier = 0;

        protected override void Awake()
        {
            base.Awake();
            gun = GetComponent<Gun>();
            magazine = GetComponentInChildren<Magazine>();

            if (hasShellCarrier && shellCarrierMount != null && shellPrefab != null)
            {
                InitializeShellCarrier();
            }

            if (hasDualTube)
            {
                UpdateTubeVisuals();
            }
        }

        protected override void Update()
        {
            base.Update();

            if (hasDualTube && Input.GetKeyDown(tubeSwitchKey))
            {
                SwitchTube();
            }
        }

        protected override void OnFire()
        {
            base.OnFire();

            // ARGO system - reduces recoil with lighter loads
            if (hasARGO && magazine != null)
            {
                float ammoRatio = (float)magazine.CurrentAmmoCount / magazine.MaxAmmoCount;
                // Lighter recoil when magazine is full (gas system regulates)
                gun.recoilMultiplier = Mathf.Lerp(0.7f, 1.0f, 1f - ammoRatio);
            }
        }

        protected override void OnReloadStart()
        {
            base.OnReloadStart();

            // Shell carrier reload animation would go here
            if (hasShellCarrier)
            {
                StartCoroutine(ShellCarrierReloadRoutine());
            }
        }

        private System.Collections.IEnumerator ShellCarrierReloadRoutine()
        {
            // Simulate grabbing shells from carrier
            for (int i = 0; i < Mathf.Min(shellCarrierCapacity, magazine.MaxAmmoCount - magazine.CurrentAmmoCount); i++)
            {
                if (carrierShells != null && i < carrierShells.Length && carrierShells[i] != null)
                {
                    carrierShells[i].SetActive(false);
                }
                shellsInCarrier = Mathf.Max(0, shellsInCarrier - 1);
                yield return new WaitForSeconds(0.4f);
            }
        }

        public void RefillShellCarrier()
        {
            shellsInCarrier = shellCarrierCapacity;
            if (carrierShells != null)
            {
                foreach (var shell in carrierShells)
                {
                    if (shell != null) shell.SetActive(true);
                }
            }
        }

        private void InitializeShellCarrier()
        {
            carrierShells = new GameObject[shellCarrierCapacity];
            for (int i = 0; i < shellCarrierCapacity; i++)
            {
                var shell = Instantiate(shellPrefab, shellCarrierMount);
                shell.transform.localPosition = new Vector3(0, i * 0.03f, 0);
                shell.transform.localRotation = Quaternion.Euler(90, 0, 0);
                carrierShells[i] = shell;
            }
            shellsInCarrier = shellCarrierCapacity;
        }

        private void SwitchTube()
        {
            currentTube = 1 - currentTube;
            UpdateTubeVisuals();

            // In a real implementation, this would switch the magazine source
            Debug.Log($"[JWA] Switched to Tube {(char)('A' + currentTube)}");
        }

        private void UpdateTubeVisuals()
        {
            if (tubeA != null) tubeA.gameObject.SetActive(currentTube == 0);
            if (tubeB != null) tubeB.gameObject.SetActive(currentTube == 1);
        }

        public override float GetDamageMultiplier(float distance)
        {
            float multiplier = base.GetDamageMultiplier(distance);

            // Shotguns have better damage at close range
            if (distance < 10f)
            {
                multiplier *= 1.2f;
            }

            return multiplier;
        }

        // Public API
        public bool HasDualTube => hasDualTube;
        public int CurrentTube => currentTube;
        public int ShellsInCarrier => shellsInCarrier;
        public bool HasARGO => hasARGO;
    }
}