using UnityEngine;
using BladeAndSorcery.Gameplay;
using System.Collections;

namespace JohnWickArsenal.Behaviors
{
    /// <summary>
    /// The legendary Pencil - "In the right hands, deadlier than a gun."
    /// Instant kill on head/neck hits, breaks after 3 kills, pure John Wick Easter egg.
    /// </summary>
    public class PencilBehavior : ItemBehavior
    {
        [Header("Pencil Settings")]
        [SerializeField] private bool instantKillHeadshot = true;
        [SerializeField] private bool instantKillNeck = true;
        [SerializeField] private float penetrationDepth = 0.15f;
        [SerializeField] private int maxKillsBeforeBreak = 3;
        [SerializeField] private float throwForce = 25f;
        [SerializeField] private float spinRate = 1080f;
        [SerializeField] private float stickProbability = 0.85f;

        [Header("Visual")]
        [SerializeField] private Material pencilBodyMaterial;
        [SerializeField] private Material pencilTipMaterial;
        [SerializeField] private Material pencilEraserMaterial;
        [SerializeField] private ParticleSystem graphiteDust;
        [SerializeField] private AudioClip throwSound;
        [SerializeField] private AudioClip impactFleshSound;
        [SerializeField] private AudioClip impactHardSound;
        [SerializeField] private AudioClip breakSound;
        [SerializeField] private AudioClip johnWickQuote; // "Yeah, I'm thinking I'm back"

        [Header("Break Visuals")]
        [SerializeField] private GameObject brokenPencilPrefab;
        [SerializeField] private float breakForce = 5f;

        private Throwable throwable;
        private Rigidbody rb;
        private int killCount = 0;
        private bool isBroken = false;
        private bool hasPlayedQuote = false;

        protected override void Awake()
        {
            base.Awake();
            throwable = GetComponent<Throwable>();
            rb = GetComponent<Rigidbody>();

            ApplyPencilMaterials();
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            killCount = 0;
            isBroken = false;
            hasPlayedQuote = false;

            // Play John Wick quote on first spawn
            if (johnWickQuote != null && !hasPlayedQuote)
            {
                StartCoroutine(PlayQuoteDelayed());
            }
        }

        private IEnumerator PlayQuoteDelayed()
        {
            yield return new WaitForSeconds(0.5f);
            AudioSource.PlayClipAtPoint(johnWickQuote, transform.position, 1f);
            hasPlayedQuote = true;
        }

        protected override void OnThrow(RagdollHand hand, Vector3 velocity, Vector3 angularVelocity)
        {
            base.OnThrow(hand, velocity, angularVelocity);

            if (isBroken) return;

            // Extreme spin for armor penetration
            rb.angularVelocity = transform.right * spinRate * Mathf.Deg2Rad;

            if (throwSound != null)
            {
                AudioSource.PlayClipAtPoint(throwSound, transform.position, 1f);
            }
        }

        protected override void OnCollisionEnter(Collision collision)
        {
            if (isBroken) return;

            base.OnCollisionEnter(collision);

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
            bool isHeadshot = false;
            bool isNeckShot = false;

            if (ragdollPart != null)
            {
                string partName = ragdollPart.transform.name.ToLower();
                isHeadshot = partName.Contains("head");
                isNeckShot = partName.Contains("neck");
            }

            // INSTANT KILL LOGIC
            if ((isHeadshot && instantKillHeadshot) || (isNeckShot && instantKillNeck))
            {
                PerformInstantKill(collision, creature, ragdollPart, isHeadshot);
            }
            else
            {
                // Normal damage for body shots
                float damage = throwable.damage; // 999 from data
                if (creature != null)
                {
                    creature.Damage(damage, collision.contacts[0].point, collision.relativeVelocity.normalized,
                        DamageType.Pierce, throwable.owner);
                }
            }

            // Effects
            if (graphiteDust != null)
            {
                graphiteDust.transform.position = collision.contacts[0].point;
                graphiteDust.Emit(5);
            }

            if (impactFleshSound != null)
            {
                AudioSource.PlayClipAtPoint(impactFleshSound, collision.contacts[0].point, 1.2f);
            }
        }

        private void PerformInstantKill(Collision collision, Creature creature, RagdollPart ragdollPart, bool isHeadshot)
        {
            killCount++;
            Debug.Log($"[JWA PENCIL] INSTANT KILL #{killCount} - {(isHeadshot ? "HEADSHOT" : "NECK")}");

            // Kill the creature instantly
            if (creature != null)
            {
                creature.Kill(collision.contacts[0].point, collision.relativeVelocity.normalized);
            }
            else if (ragdollPart != null)
            {
                var r = ragdollPart.ragdoll;
                if (r != null) r.Kill();
            }

            // Stick the pencil
            StickToTarget(collision, ragdollPart?.transform ?? creature?.transform);

            // Check for break
            if (killCount >= maxKillsBeforeBreak)
            {
                BreakPencil();
            }
        }

        private void StickToTarget(Collision collision, Transform target)
        {
            if (target == null) return;

            // Penetrate into target
            Vector3 penetrationDir = -collision.relativeVelocity.normalized;
            transform.position = collision.contacts[0].point + penetrationDir * penetrationDepth;

            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.SetParent(target);

            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        private void HandleHardImpact(Collision collision)
        {
            if (impactHardSound != null)
            {
                AudioSource.PlayClipAtPoint(impactHardSound, collision.contacts[0].point, 1f);
            }

            // Pencil might break on hard impact
            if (Random.value < 0.3f) // 30% chance to break on hard impact
            {
                BreakPencil();
            }
        }

        private void BreakPencil()
        {
            if (isBroken) return;

            isBroken = true;
            Debug.Log("[JWA PENCIL] The pencil has served its purpose. It breaks.");

            if (breakSound != null)
            {
                AudioSource.PlayClipAtPoint(breakSound, transform.position, 1f);
            }

            // Spawn broken pieces
            if (brokenPencilPrefab != null)
            {
                var broken = Instantiate(brokenPencilPrefab, transform.position, transform.rotation);
                var pieces = broken.GetComponentsInChildren<Rigidbody>();
                foreach (var piece in pieces)
                {
                    piece.AddExplosionForce(breakForce, transform.position, 0.5f);
                    piece.AddTorque(Random.insideUnitSphere * breakForce * 10f);
                }
            }

            // Destroy this pencil after a moment
            StartCoroutine(DestroyAfterBreak());
        }

        private IEnumerator DestroyAfterBreak()
        {
            yield return new WaitForSeconds(0.1f);

            // Remove from hand if held
            var holder = GetComponentInParent<RagdollHand>();
            if (holder != null)
            {
                holder.TryRelease();
            }

            Despawn();
        }

        public override bool OnGrab(RagdollHand hand)
        {
            // Can't grab broken pencil
            if (isBroken) return false;

            return base.OnGrab(hand);
        }

        protected override void OnDespawn()
        {
            base.OnDespawn();
            killCount = 0;
            isBroken = false;
        }

        private void ApplyPencilMaterials()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                var materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    string matName = materials[i].name.ToLower();
                    if (matName.Contains("body") || matName.Contains("shaft") || matName.Contains("wood"))
                    {
                        if (pencilBodyMaterial != null) materials[i] = pencilBodyMaterial;
                    }
                    else if (matName.Contains("tip") || matName.Contains("lead") || matName.Contains("graphite"))
                    {
                        if (pencilTipMaterial != null) materials[i] = pencilTipMaterial;
                    }
                    else if (matName.Contains("eraser") || matName.Contains("rubber"))
                    {
                        if (pencilEraserMaterial != null) materials[i] = pencilEraserMaterial;
                    }
                }
                renderer.materials = materials;
            }
        }

        // Public API
        public int KillCount => killCount;
        public int MaxKills => maxKillsBeforeBreak;
        public bool IsBroken => isBroken;
        public float DurabilityPercent => 1f - ((float)killCount / maxKillsBeforeBreak);

        // Static Easter egg tracker
        public static int TotalPencilKills { get; private set; } = 0;

        private void OnDestroy()
        {
            TotalPencilKills += killCount;
        }
    }
}