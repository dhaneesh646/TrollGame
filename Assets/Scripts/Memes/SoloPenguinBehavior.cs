using System.Collections;
using UnityEngine;
using TrollGame.Core;

namespace TrollGame.Memes
{
    /// <summary>
    /// Solo Penguin behavior — the viral lonely penguin that became a meme.
    /// In TrollGame, it serves as a moving platform that walks its own path.
    /// The penguin doesn't care about the player. It walks. Alone.
    /// 
    /// Special behaviors:
    /// - Walks at its own pace regardless of player needs
    /// - Sometimes stops to look at the camera (4th-wall moment)
    /// - If the player stands on it too long, it shakes them off
    /// - Has rare dialogue triggered by proximity
    /// </summary>
    public class SoloPenguinBehavior : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 2f;
        [SerializeField] private float walkRange = 10f;
        [SerializeField] private bool loopPath = true;
        [SerializeField] private Transform[] waypoints;

        [Header("Platform Behavior")]
        [SerializeField] private float shakeOffTime = 5f;
        [SerializeField] private float shakeOffForce = 8f;

        [Header("Fourth Wall")]
        [SerializeField] private float cameraLookChance = 0.05f;
        [SerializeField] private float cameraLookDuration = 2f;
        [SerializeField] private Transform cameraTransform;

        [Header("Dialogue")]
        [SerializeField] private float dialogueTriggerDistance = 5f;
        [SerializeField] private float dialogueCooldown = 30f;

        private int currentWaypointIndex = 0;
        private bool isLookingAtCamera = false;
        private float playerOnPenguinTime = 0f;
        private bool playerIsOnPenguin = false;
        private float lastDialogueTime = -999f;
        private Vector3 startPosition;

        private string[] penguinDialogue = new string[]
        {
            "The penguin looks at you... then looks away. It has places to be.",
            "Solo Penguin walks alone. Not because it has to. Because it wants to.",
            "You're standing on a penguin. Think about your life choices.",
            "The penguin neither approves nor disapproves. It simply waddles.",
            "Solo Penguin has seen things. Things you wouldn't understand.",
            "The penguin pauses. For a moment, you share a connection. Then it walks away.",
            "Solo Penguin doesn't need validation. Solo Penguin IS validation."
        };

        private void Start()
        {
            startPosition = transform.position;

            if (cameraTransform == null)
            {
                Camera mainCam = Camera.main;
                if (mainCam != null) cameraTransform = mainCam.transform;
            }
        }

        private void Update()
        {
            if (!isLookingAtCamera)
            {
                MoveAlongPath();
            }

            // Random camera look
            if (!isLookingAtCamera && Random.value < cameraLookChance * Time.deltaTime)
            {
                StartCoroutine(LookAtCamera());
            }

            // Track player standing on penguin
            if (playerIsOnPenguin)
            {
                playerOnPenguinTime += Time.deltaTime;

                if (playerOnPenguinTime > shakeOffTime)
                {
                    ShakePlayerOff();
                }
            }

            // Proximity dialogue
            CheckProximityDialogue();
        }

        /// <summary>
        /// Moves the penguin along its waypoint path.
        /// </summary>
        private void MoveAlongPath()
        {
            if (waypoints == null || waypoints.Length == 0)
            {
                // Simple back-and-forth if no waypoints
                float offset = Mathf.PingPong(Time.time * walkSpeed, walkRange) - walkRange / 2f;
                transform.position = startPosition + new Vector3(offset, 0, 0);
                return;
            }

            Transform targetWaypoint = waypoints[currentWaypointIndex];
            if (targetWaypoint == null) return;

            Vector3 direction = (targetWaypoint.position - transform.position).normalized;
            transform.position += direction * walkSpeed * Time.deltaTime;

            // Face movement direction
            if (direction.x != 0)
            {
                transform.localScale = new Vector3(
                    Mathf.Sign(direction.x) * Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z
                );
            }

            // Check if reached waypoint
            if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.5f)
            {
                currentWaypointIndex++;

                if (currentWaypointIndex >= waypoints.Length)
                {
                    if (loopPath)
                    {
                        currentWaypointIndex = 0;
                    }
                    else
                    {
                        currentWaypointIndex = waypoints.Length - 1;
                    }
                }
            }
        }

        /// <summary>
        /// The penguin stops and looks directly at the camera (4th-wall moment).
        /// </summary>
        private IEnumerator LookAtCamera()
        {
            isLookingAtCamera = true;

            // Turn toward camera
            if (cameraTransform != null)
            {
                Vector3 lookDir = (cameraTransform.position - transform.position).normalized;
                lookDir.y = 0;

                if (lookDir.x != 0)
                {
                    transform.localScale = new Vector3(
                        Mathf.Sign(lookDir.x) * Mathf.Abs(transform.localScale.x),
                        transform.localScale.y,
                        transform.localScale.z
                    );
                }
            }

            yield return new WaitForSeconds(cameraLookDuration);

            isLookingAtCamera = false;
        }

        /// <summary>
        /// Shakes the player off after standing on the penguin too long.
        /// </summary>
        private void ShakePlayerOff()
        {
            playerOnPenguinTime = 0f;

            // Find player on top
            Collider[] colliders = Physics.OverlapSphere(
                transform.position + Vector3.up * 1.5f, 1f);

            foreach (Collider col in colliders)
            {
                if (col.CompareTag("Player"))
                {
                    Rigidbody rb = col.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 launchDir = (Vector3.up + Random.insideUnitSphere * 0.3f).normalized;
                        rb.AddForce(launchDir * shakeOffForce, ForceMode.Impulse);
                    }

                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowTrollDialogue("Solo Penguin",
                            "The penguin shakes you off. It walks ALONE.", 2f);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Checks if the player is close enough to trigger dialogue.
        /// </summary>
        private void CheckProximityDialogue()
        {
            if (Time.time - lastDialogueTime < dialogueCooldown) return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance < dialogueTriggerDistance)
            {
                lastDialogueTime = Time.time;
                string dialogue = penguinDialogue[Random.Range(0, penguinDialogue.Length)];

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowTrollDialogue("Solo Penguin", dialogue, 3f);
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                // Check if player landed on top
                if (collision.contacts.Length > 0)
                {
                    Vector3 contactNormal = collision.contacts[0].normal;
                    if (Vector3.Dot(contactNormal, Vector3.down) > 0.5f)
                    {
                        playerIsOnPenguin = true;
                        playerOnPenguinTime = 0f;

                        // Parent player to penguin for platform behavior
                        collision.transform.SetParent(transform);
                    }
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                playerIsOnPenguin = false;
                playerOnPenguinTime = 0f;
                collision.transform.SetParent(null);
            }
        }
    }
}
