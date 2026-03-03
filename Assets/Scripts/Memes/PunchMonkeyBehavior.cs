using System.Collections;
using UnityEngine;
using TrollGame.Core;

namespace TrollGame.Memes
{
    /// <summary>
    /// Punch Monkey behavior — the viral rage-bait monkey.
    /// In TrollGame, it serves as a hazard that punches the player 
    /// at the worst possible moments.
    /// 
    /// Special behaviors:
    /// - Waits until the player is mid-jump to attack (maximum frustration)
    /// - Gets faster the more it successfully hits the player
    /// - Has a "wind-up" animation to give a split-second warning
    /// - Taunts the player after each hit
    /// - Can appear out of nowhere (off-screen spawn)
    /// </summary>
    public class PunchMonkeyBehavior : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private float detectionRange = 15f;
        [SerializeField] private float attackRange = 3f;
        [SerializeField] private LayerMask playerLayer;

        [Header("Attack")]
        [SerializeField] private float punchForce = 15f;
        [SerializeField] private float punchCooldown = 3f;
        [SerializeField] private float windUpDuration = 0.5f;
        [SerializeField] private float punchDamage = 1f;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float chaseSpeedMultiplier = 1.5f;
        [SerializeField] private float patrolRange = 8f;

        [Header("Troll Settings")]
        [SerializeField] private bool waitForJump = true;
        [SerializeField] private bool getsFasterWithHits = true;
        [SerializeField] private float speedIncreasePerHit = 0.5f;

        [Header("Spawn Settings")]
        [SerializeField] private bool canSpawnOffscreen = true;
        [SerializeField] private float offscreenSpawnDistance = 20f;

        private Transform playerTransform;
        private Rigidbody playerRb;
        private float lastPunchTime = -999f;
        private int successfulHits = 0;
        private bool isWindingUp = false;
        private bool isChasing = false;
        private Vector3 patrolCenter;
        private MonkeyState currentState = MonkeyState.Patrol;

        private string[] tauntLines = new string[]
        {
            "PUNCH! The monkey got you!",
            "Punch Monkey strikes again! Did you even see that coming?",
            "The monkey is laughing at you. It has one move and it WORKS.",
            "BONK! That's what you get for standing there.",
            "Punch Monkey doesn't miss. Punch Monkey never misses.",
            "The monkey winds up... WHAM! Another victim.",
            "You just got monkeyed. That's a word now.",
            "The monkey respects no one. Especially not you."
        };

        private enum MonkeyState
        {
            Patrol,
            Chase,
            WindUp,
            Punch,
            Taunt,
            Cooldown
        }

        private void Start()
        {
            patrolCenter = transform.position;
            FindPlayer();
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                FindPlayer();
                return;
            }

            float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            switch (currentState)
            {
                case MonkeyState.Patrol:
                    Patrol();
                    if (distToPlayer < detectionRange)
                    {
                        currentState = MonkeyState.Chase;
                    }
                    break;

                case MonkeyState.Chase:
                    ChasePlayer();
                    if (distToPlayer < attackRange && CanPunch())
                    {
                        if (waitForJump && IsPlayerInAir())
                        {
                            StartCoroutine(PunchSequence());
                        }
                        else if (!waitForJump)
                        {
                            StartCoroutine(PunchSequence());
                        }
                    }
                    if (distToPlayer > detectionRange * 1.5f)
                    {
                        currentState = MonkeyState.Patrol;
                    }
                    break;

                case MonkeyState.Cooldown:
                    if (Time.time - lastPunchTime > punchCooldown)
                    {
                        currentState = MonkeyState.Chase;
                    }
                    break;
            }

            // Face the player when chasing
            if (currentState == MonkeyState.Chase && playerTransform != null)
            {
                Vector3 dir = (playerTransform.position - transform.position).normalized;
                if (dir.x != 0)
                {
                    transform.localScale = new Vector3(
                        Mathf.Sign(dir.x) * Mathf.Abs(transform.localScale.x),
                        transform.localScale.y,
                        transform.localScale.z
                    );
                }
            }
        }

        /// <summary>
        /// Simple patrol behavior around the spawn point.
        /// </summary>
        private void Patrol()
        {
            float offset = Mathf.Sin(Time.time * moveSpeed * 0.3f) * patrolRange;
            Vector3 targetPos = patrolCenter + new Vector3(offset, 0, 0);

            transform.position = Vector3.MoveTowards(
                transform.position, targetPos, moveSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Chases the player aggressively.
        /// </summary>
        private void ChasePlayer()
        {
            if (playerTransform == null) return;

            float currentSpeed = moveSpeed * chaseSpeedMultiplier;
            if (getsFasterWithHits)
            {
                currentSpeed += successfulHits * speedIncreasePerHit;
            }

            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.position += direction * currentSpeed * Time.deltaTime;
        }

        /// <summary>
        /// The punch attack sequence with wind-up and follow-through.
        /// </summary>
        private IEnumerator PunchSequence()
        {
            currentState = MonkeyState.WindUp;
            isWindingUp = true;

            // Wind-up (brief warning for the player)
            // Scale up slightly to indicate incoming attack
            Vector3 originalScale = transform.localScale;
            float elapsed = 0f;
            while (elapsed < windUpDuration)
            {
                elapsed += Time.deltaTime;
                float scale = 1f + (elapsed / windUpDuration) * 0.3f;
                transform.localScale = new Vector3(
                    originalScale.x * scale,
                    originalScale.y * scale,
                    originalScale.z
                );
                yield return null;
            }

            // PUNCH!
            currentState = MonkeyState.Punch;
            isWindingUp = false;

            if (playerTransform != null)
            {
                float dist = Vector3.Distance(transform.position, playerTransform.position);
                if (dist < attackRange * 1.5f)
                {
                    // Apply force to player
                    if (playerRb != null)
                    {
                        Vector3 punchDir = (playerTransform.position - transform.position).normalized;
                        punchDir.y = 0.5f; // Slight upward component
                        punchDir = punchDir.normalized;

                        float actualForce = punchForce + (successfulHits * 2f);
                        playerRb.AddForce(punchDir * actualForce, ForceMode.Impulse);
                    }

                    successfulHits++;

                    // Play sound
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFX(null); // Punch sound would go here
                    }

                    // Taunt
                    StartCoroutine(TauntAfterPunch());
                }
            }

            // Return to original scale
            transform.localScale = originalScale;

            lastPunchTime = Time.time;
            currentState = MonkeyState.Cooldown;
        }

        /// <summary>
        /// Taunts the player after a successful punch.
        /// </summary>
        private IEnumerator TauntAfterPunch()
        {
            yield return new WaitForSeconds(0.5f);

            string taunt = tauntLines[Random.Range(0, tauntLines.Length)];
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowTrollDialogue("Punch Monkey", taunt, 2f);
            }
        }

        /// <summary>
        /// Checks if the player is in the air (for maximum frustration timing).
        /// </summary>
        private bool IsPlayerInAir()
        {
            if (playerRb == null) return false;

            // Check if there's ground below the player
            Vector3 origin = playerTransform.position;
            return !Physics.Raycast(origin, Vector3.down, 1.5f);
        }

        private bool CanPunch()
        {
            return Time.time - lastPunchTime > punchCooldown && !isWindingUp;
        }

        private void FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerRb = player.GetComponent<Rigidbody>();
            }
        }

        /// <summary>
        /// Spawns the monkey from off-screen at a position near the player.
        /// </summary>
        public static PunchMonkeyBehavior SpawnOffscreen(GameObject prefab, Vector3 playerPosition)
        {
            if (prefab == null) return null;

            // Spawn off-screen
            Vector3 spawnOffset = Random.insideUnitSphere.normalized * 20f;
            spawnOffset.y = 0;
            Vector3 spawnPos = playerPosition + spawnOffset;

            GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity);
            PunchMonkeyBehavior monkey = instance.GetComponent<PunchMonkeyBehavior>();

            return monkey;
        }
    }
}
