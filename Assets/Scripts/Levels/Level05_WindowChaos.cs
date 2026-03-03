using System.Collections;
using UnityEngine;
using TrollGame.Core;
using TrollGame.FourthWall;

namespace TrollGame.Levels
{
    /// <summary>
    /// LEVEL 5: "The Window"
    /// 
    /// CONCEPT: The game window itself becomes the obstacle. The window
    /// shakes, moves around, shrinks, and "runs away" from the player's
    /// mouse. The platforming level is inside the chaotic window, so the
    /// player must navigate while their viewport is literally moving.
    /// 
    /// CLIP MOMENT: When the window suddenly slides across the screen
    /// while the player is mid-jump. Streamers will audibly gasp.
    /// 
    /// TECHNICAL: Uses WindowManipulator for standalone builds.
    /// In-editor, simulates via camera/viewport manipulation.
    /// </summary>
    public class Level05_WindowChaos : LevelBase
    {
        [Header("Window References")]
        [SerializeField] private WindowManipulator windowManipulator;

        [Header("Camera Chaos (Editor Fallback)")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float cameraShakeIntensity = 0.3f;

        [Header("Level Progression")]
        [SerializeField] private Transform[] checkpoints;
        [SerializeField] private Transform exitPoint;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float exitTriggerDistance = 2f;

        [Header("Chaos Phases")]
        [SerializeField] private float phase1Duration = 20f;
        [SerializeField] private float phase2Duration = 30f;
        [SerializeField] private float phase3Duration = 40f;

        private int currentPhase = 0;
        private int checkpointReached = 0;
        private Vector3 originalCameraPos;
        private bool chaosActive = false;

        protected override void Awake()
        {
            base.Awake();
            levelName = "The Window";
            primaryTrollType = TrollType.WindowManipulation;
            trollTriggerDelay = 8f;
        }

        protected override void Start()
        {
            base.Start();

            if (mainCamera != null)
            {
                originalCameraPos = mainCamera.transform.localPosition;
            }
        }

        protected override void OnLevelStart()
        {
            ShowTrollMessage("A perfectly normal platforming level. Enjoy the stability while it lasts.");
        }

        protected override void OnTrollBegin()
        {
            chaosActive = true;
            StartCoroutine(WindowChaosSequence());
        }

        protected override void OnTrollUpdate()
        {
            if (!chaosActive) return;

            // Check for exit
            if (playerTransform != null && exitPoint != null)
            {
                float dist = Vector3.Distance(playerTransform.position, exitPoint.position);
                if (dist < exitTriggerDistance)
                {
                    StartCoroutine(ExitReachedSequence());
                }
            }
        }

        protected override bool CheckLevelComplete()
        {
            return levelComplete;
        }

        /// <summary>
        /// Main chaos sequence with escalating window manipulation.
        /// </summary>
        private IEnumerator WindowChaosSequence()
        {
            // Phase 1: Gentle shaking
            yield return StartCoroutine(Phase1_GentleShaking());

            // Phase 2: Window moves and resizes
            yield return StartCoroutine(Phase2_WindowMoves());

            // Phase 3: Full chaos - window bounces around like a screensaver
            yield return StartCoroutine(Phase3_FullChaos());
        }

        /// <summary>
        /// Phase 1: The window starts with subtle shaking.
        /// Player might not even notice at first.
        /// </summary>
        private IEnumerator Phase1_GentleShaking()
        {
            currentPhase = 1;
            ShowTrollMessage("Is it just me, or is something... shaking?");

            float elapsed = 0f;
            while (elapsed < phase1Duration && chaosActive)
            {
                elapsed += Time.deltaTime;

                // Subtle camera shake (works in editor too)
                if (mainCamera != null)
                {
                    float intensity = cameraShakeIntensity * (elapsed / phase1Duration);
                    Vector3 shakeOffset = new Vector3(
                        Random.Range(-intensity, intensity),
                        Random.Range(-intensity, intensity),
                        0
                    );
                    mainCamera.transform.localPosition = originalCameraPos + shakeOffset;
                }

                // Periodic window shake in builds
                if (elapsed % 5f < 0.1f && windowManipulator != null)
                {
                    windowManipulator.ShakeWindow(0.5f, 5f);
                }

                yield return null;
            }

            // Reset camera
            if (mainCamera != null) mainCamera.transform.localPosition = originalCameraPos;
        }

        /// <summary>
        /// Phase 2: The window starts moving on its own.
        /// </summary>
        private IEnumerator Phase2_WindowMoves()
        {
            currentPhase = 2;
            ShowTrollMessage("The window is... moving?! What's happening?!");

            float elapsed = 0f;
            while (elapsed < phase2Duration && chaosActive)
            {
                elapsed += Time.deltaTime;

                // Window moves periodically
                if (elapsed % 8f < 0.1f && windowManipulator != null)
                {
                    windowManipulator.WindowRunAway();
                }

                // Camera tilting effect (editor fallback)
                if (mainCamera != null)
                {
                    float tilt = Mathf.Sin(elapsed * 0.5f) * 3f;
                    mainCamera.transform.localRotation = Quaternion.Euler(0, 0, tilt);
                }

                yield return null;
            }

            // Reset camera rotation
            if (mainCamera != null) mainCamera.transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Phase 3: Full chaos - the window bounces around like a DVD screensaver.
        /// </summary>
        private IEnumerator Phase3_FullChaos()
        {
            currentPhase = 3;
            ShowTrollMessage("COMPLETE WINDOW CHAOS! Good luck reaching the exit!");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SwitchToTrollMusic();
            }

            // Start window drift (bouncing screensaver mode)
            if (windowManipulator != null)
            {
                windowManipulator.DriftWindow(100f, phase3Duration);
            }

            float elapsed = 0f;
            while (elapsed < phase3Duration && chaosActive)
            {
                elapsed += Time.deltaTime;

                // Chaotic camera effects
                if (mainCamera != null)
                {
                    float intensity = 0.5f;
                    Vector3 shakeOffset = new Vector3(
                        Random.Range(-intensity, intensity),
                        Random.Range(-intensity, intensity),
                        0
                    );
                    mainCamera.transform.localPosition = originalCameraPos + shakeOffset;

                    float tilt = Mathf.Sin(elapsed * 2f) * 5f;
                    mainCamera.transform.localRotation = Quaternion.Euler(0, 0, tilt);

                    // Random FOV changes
                    if (Random.value < 0.01f)
                    {
                        mainCamera.fieldOfView = Random.Range(40f, 100f);
                    }
                }

                // Random screen flashes
                if (Random.value < 0.005f && UIManager.Instance != null)
                {
                    Color flashColor = new Color(Random.value, Random.value, Random.value);
                    UIManager.Instance.FlashScreen(flashColor, 0.1f);
                }

                yield return null;
            }

            // Calm down
            ResetCameraEffects();
        }

        /// <summary>
        /// Called when the player reaches the exit during chaos.
        /// </summary>
        private IEnumerator ExitReachedSequence()
        {
            if (levelComplete) yield break;

            chaosActive = false;

            // Dramatic stop - everything freezes
            ResetCameraEffects();

            if (windowManipulator != null)
            {
                windowManipulator.RestoreWindowPosition();
            }

            // Brief silence
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopMusic(0.5f);
            }

            yield return new WaitForSeconds(1f);

            ShowTrollMessage("You... you actually made it through that?!");

            yield return new WaitForSeconds(2f);

            ShowTrollMessage("I'm honestly impressed. Fine, you can pass.");

            yield return new WaitForSeconds(2f);

            levelComplete = true;
            OnLevelComplete();
        }

        /// <summary>
        /// Resets all camera effects to default.
        /// </summary>
        private void ResetCameraEffects()
        {
            if (mainCamera != null)
            {
                mainCamera.transform.localPosition = originalCameraPos;
                mainCamera.transform.localRotation = Quaternion.identity;
                mainCamera.fieldOfView = 60f;
            }
        }

        /// <summary>
        /// Called when the player reaches a checkpoint.
        /// </summary>
        public void OnCheckpointReached(int checkpointIndex)
        {
            checkpointReached = checkpointIndex;

            string[] checkpointMessages = new string[]
            {
                "Checkpoint! The chaos won't get easier though.",
                "Saved! Not that saving will help you here.",
                "Checkpoint reached! The window disagrees.",
                "Progress! The window is plotting its revenge."
            };

            ShowTrollMessage(checkpointMessages[Random.Range(0, checkpointMessages.Length)]);
        }

        protected override void OnDeathTroll()
        {
            string[] messages = currentPhase switch
            {
                1 => new[] { "Killed by a gentle shake? Weak.", "The shaking was barely noticeable!" },
                2 => new[] { "The window outsmarted you.", "You can't even dodge a WINDOW?" },
                3 => new[] { "Full chaos claimed another victim.", "DVD screensaver of death." },
                _ => new[] { "How?" }
            };

            ShowTrollMessage(messages[Random.Range(0, messages.Length)]);
        }
    }
}
