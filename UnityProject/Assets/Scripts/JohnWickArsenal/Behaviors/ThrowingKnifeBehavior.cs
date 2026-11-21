using UnityEngine;
using BladeAndSorcery.Gameplay;

namespace JohnWickArsenal.Behaviors
{
    /// <summary>
    /// Custom behavior for Throwing Knives - balanced, retrievable, silent kills
    /// </summary>
    public class ThrowingKnifeBehavior : ItemBehavior
    {
        [Header("Throwing Knife Settings")]
        [SerializeField] private bool isBalanced = true;
        [SerializeField] private bool isDoubleEdged = true;
        [SerializeField] private float headshotMultiplier = 3.0f;
        [SerializeField] private float neckMultiplier = 2.5f;
        [SerializeField] private float stickProbability = 0.95f;
        [SerializeField] private int maxRetrievalDistance = 50;

        [Header("Physics")]
        [SerializeField] private float throwForce = 18f;
        [SerializeField] private float spinRate = 720f; // degrees per second
        [SerializeField] private float stabilizeDistance = 3f; // Distance before knife stabilizes

        [Header("Visual/Audio")]
        [SerializeField] private AudioClip throwSound;
        [SerializeField] private AudioClip impactFleshSound;
        [SerializeField] private AudioClip impactHardSound;
        [SerializeField] private ParticleSystem bloodParticles;
        [SerializeField] private TrailRenderer spinTrail;

        [Header("Paracord Wrap")]
        [SerializeField] private Material paracordMaterial;
        [SerializeField] private Color paracordColor = new Color(0.2f, 0.15f, 0.1f);

        private Throwable throwable;
        private Rigidbody rb;
        private bool hasStuck = false;
        private Transform stuckTarget;
        private Vector3 stuckLocalPosition;
        private Quaternion stuckLocalRotation;

        protected override void Awake()
        {
            base.Awake();
            throwable = GetComponent<Throwable>();
            rb = GetComponent<Rigidbody>();

            if (spinTrail != null)
            {
                spinTrail.enabled = false;
            }

            ApplyParacordWrap();
        }

        protected override void OnThrow(RagdollHand hand, Vector3 velocity, Vector3 angularVelocity)
        {
            base.OnThrow(hand, velocity, angularVelocity);

            hasStuck = false;
            stuckTarget = null;

            // Apply spin for stability
            if (isBalanced)
            {
                rb.angularVelocity = transform.right * spinRate * Mathf.Deg2Rad;
            }

            // Enable trail
            if (spinTrail != null)
            {
                spinTrail.enabled = true;
                spinTrail.Clear();
            }

            // Play throw sound
            if (throwSound != null)
            {
                AudioSource.PlayClipAtPoint(throwSound, transform.position, 0.8f);
            }
        }

        protected override void OnCollisionEnter(Collision collision)
        {
            base.OnCollisionEnter(collision);

            if (hasStuck) return;

            var hitRagdoll = collision.collider.GetComponentInParent<RagdollPart>();
            var hitCreature = collision.collider.GetComponentInParent<Creature>();

            if (hitRagdoll != null || hitCreature != null)
            {
                HandleFleshImpact(collision, hitRagdoll, hitCreature);
            }
            else
            {
                HandleHardImpact(collision);
            }
        }

        private void HandleFleshImpact(Collision collision, RagdollPart ragdollPart, Creature creature)
        {
            // Check for head/neck shot
            bool isHeadshot = false;
            bool isNeckShot = false;

            if (ragdollPart != null)
            {
                string partName = ragdollPart.transform.name.ToLower();
                isHeadshot = partName.Contains("head");
                isNeckShot = partName.Contains("neck");
            }

            // Calculate damage
            float damage = throwable.damage;
            if (isHeadshot) damage *= headshotMultiplier;
            else if (isNeckShot) damage *= neckMultiplier;

            // Apply damage
            if (creature != null)
            {
                creature.Damage(damage, collision.contacts[0].point, collision.relativeVelocity.normalized,
                    DamageType.Pierce, throwable.owner);
            }

            // Stick logic
            if (Random.value < stickProbability)
            {
                StickToTarget(collision, ragdollPart?.transform ?? creature?.transform);
            }

            // Effects
            if (bloodParticles != null)
            {
                bloodParticles.transform.position = collision.contacts[0].point;
                bloodParticles.transform.rotation = Quaternion.LookRotation(collision.contacts[0].normal);
                bloodParticles.Emit(20);
            }

            if (impactFleshSound != null)
            {
                AudioSource.PlayClipAtPoint(impactFleshSound, collision.contacts[0].point, 1f);
            }

            // Disable trail
            if (spinTrail != null)
            {
                spinTrail.enabled = false;
            }
        }

        private void HandleHardImpact(Collision collision)
        {
            // Knife bounces off hard surfaces
            if (impactHardSound != null)
            {
                AudioSource.PlayClipAtPoint(impactHardSound, collision.contacts[0].point, 0.8f);
            }

            // Spark particles could go here
            Debug.Log("[JWA] Knife hit hard surface, bouncing off");

            // Disable trail
            if (spinTrail != null)
            {
                spinTrail.enabled = false;
            }
        }

        private void StickToTarget(Collision collision, Transform target)
        {
            if (target == null) return;

            hasStuck = true;
            stuckTarget = target;

            // Store local position/rotation for retrieval
            stuckLocalPosition = target.InverseTransformPoint(transform.position);
            stuckLocalRotation = Quaternion.Inverse(target.rotation) * transform.rotation;

            // Make kinematic and parent to target
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.SetParent(target);

            // Disable collider to prevent physics issues
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            Debug.Log($"[JWA] Knife stuck in {target.name}");
        }

        public override bool OnGrab(RagdollHand hand)
        {
            // Allow retrieval if stuck
            if (hasStuck && stuckTarget != null)
            {
                RetrieveFromTarget();
            }

            return base.OnGrab(hand);
        }

        private void RetrieveFromTarget()
        {
            hasStuck = false;
            stuckTarget = null;

            // Restore physics
            rb.isKinematic = false;
            transform.SetParent(null);

            var col = GetComponent<Collider>();
            if (col != null) col.enabled = true;

            Debug.Log("[JWA] Knife retrieved");
        }

        protected override void OnDespawn()
        {
            base.OnDespawn();
            hasStuck = false;
            stuckTarget = null;

            if (spinTrail != null)
            {
                spinTrail.enabled = false;
                spinTrail.Clear();
            }
        }

        private void ApplyParacordWrap()
        {
            if (paracordMaterial != null)
            {
                var renderers = GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    var materials = renderer.materials;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        if (materials[i].name.ToLower().Contains("handle") ||
                            materials[i].name.ToLower().Contains("grip") ||
                            materials[i].name.ToLower().Contains("wrap"))
                        {
                            materials[i] = paracordMaterial;
                        }
                    }
                    renderer.materials = materials;
                }
            }
        }

        // Public API
        public bool IsStuck => hasStuck;
        public Transform StuckTarget => stuckTarget;
        public bool IsBalanced => isBalanced;
        public float CurrentSpinRate => rb != null ? rb.angularVelocity.magnitude * Mathf.Rad2Deg : 0f;
    }
}