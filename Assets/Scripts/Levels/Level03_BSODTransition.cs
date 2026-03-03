using System.Collections;
using UnityEngine;
using TrollGame.Core;
using TrollGame.FourthWall;

namespace TrollGame.Levels
{
    /// <summary>
    /// LEVEL 3: "The Crash"
    /// 
    /// CONCEPT: A seemingly normal platforming level that is suddenly
    /// interrupted by a fake BSOD. The player thinks their PC crashed.
    /// After the fake "reboot," the level has CHANGED — platforms moved,
    /// enemies appeared, and the rules are different.
    /// 
    /// CLIP MOMENT: The BSOD itself (instant streamer panic), followed by
    /// the relief/confusion when it "reboots" into a transformed level.
    /// 
    /// DOUBLE TROLL: The level crashes AGAIN when the player is about to
    /// reach the exit. This time the "reboot" puts them back at the start
    /// but everything is mirrored/upside down.
    /// </summary>
    public class Level03_BSODTransition : LevelBase
    {
        [Header("BSOD Controller")]
        [SerializeField] private FakeBSODController bsodController;

        [Header("Level Variants")]
        [SerializeField] private GameObject normalLevelLayout;
        [SerializeField] private GameObject corruptedLevelLayout;
        [SerializeField] private GameObject mirroredLevelLayout;

        [Header("Crash Triggers")]
        [SerializeField] private Transform firstCrashTriggerPoint;
        [SerializeField] private Transform secondCrashTriggerPoint;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float triggerDistance = 3f;

        [Header("Settings")]
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private float preCrashGlitchDuration = 2f;

        private int crashCount = 0;
        private bool isTransitioning = false;

        protected override void Awake()
        {
            base.Awake();
            levelName = "The Crash";
            primaryTrollType = TrollType.FakeCrash;
            trollTriggerDelay = 0f;
            autoStartTrolling = false;
        }

        protected override void OnLevelStart()
        {
            // Start with normal layout
            SetActiveLayout(normalLevelLayout);

            ShowTrollMessage("Finally, a normal level. Nothing weird here. Promise.");
        }

        protected override void OnTrollBegin() { }

        protected override void Update()
        {
            base.Update();

            if (isTransitioning || playerTransform == null) return;

            // Check crash trigger points
            if (crashCount == 0 && firstCrashTriggerPoint != null)
            {
                float dist = Vector3.Distance(playerTransform.position, firstCrashTriggerPoint.position);
                if (dist < triggerDistance)
                {
                    StartCoroutine(TriggerCrash(1));
                }
            }
            else if (crashCount == 1 && secondCrashTriggerPoint != null)
            {
                float dist = Vector3.Distance(playerTransform.position, secondCrashTriggerPoint.position);
                if (dist < triggerDistance)
                {
                    StartCoroutine(TriggerCrash(2));
                }
            }
        }

        protected override bool CheckLevelComplete()
        {
            return crashCount >= 2 && levelComplete;
        }

        /// <summary>
        /// Triggers a fake crash and level transformation.
        /// </summary>
        private IEnumerator TriggerCrash(int crashNumber)
        {
            isTransitioning = true;
            crashCount = crashNumber;

            // Pre-crash warning signs (subtle, then escalating)
            yield return StartCoroutine(PreCrashGlitches());

            // THE CRASH
            if (bsodController != null)
            {
                string errorCode = crashNumber == 1
                    ? "LEVEL_INTEGRITY_FAILURE"
                    : "REALITY_CORRUPTION_DETECTED";

                bsodController.TriggerBSOD(errorCode);
            }
            else if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFakeBSOD(8f, "LEVEL_CORRUPTION_ERROR");
            }

            // Wait for BSOD to finish
            yield return new WaitForSecondsRealtime(12f);

            // Transform the level
            if (crashNumber == 1)
            {
                // First crash: level becomes "corrupted"
                SetActiveLayout(corruptedLevelLayout);
                ShowTrollMessage("Hmm, the level looks... different after that crash.");

                // Change the mood
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.SwitchToCreepyMusic();
                }
            }
            else
            {
                // Second crash: level is mirrored/inverted
                SetActiveLayout(mirroredLevelLayout);
                ShowTrollMessage("Not AGAIN! And everything is... backwards?!");

                // Invert some controls for extra chaos
                Physics.gravity = new Vector3(0, -4.9f, 0); // Half gravity
            }

            // Respawn player
            if (playerTransform != null && playerSpawnPoint != null)
            {
                playerTransform.position = playerSpawnPoint.position;
            }

            isTransitioning = false;

            // After second crash, mark level as completable
            if (crashNumber >= 2)
            {
                yield return new WaitForSeconds(15f);
                ShowTrollMessage("Okay, if you make it to the exit THIS time, I'll let you through.");
                levelComplete = true;
            }
        }

        /// <summary>
        /// Pre-crash visual glitches that hint something is about to go wrong.
        /// Builds tension before the BSOD.
        /// </summary>
        private IEnumerator PreCrashGlitches()
        {
            float elapsed = 0f;

            while (elapsed < preCrashGlitchDuration)
            {
                elapsed += Time.deltaTime;
                float intensity = elapsed / preCrashGlitchDuration;

                // Increasing glitch frequency
                if (Random.value < intensity * 0.3f)
                {
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowGlitchEffect(Random.Range(0.05f, 0.2f));
                    }
                }

                // Audio distortion
                if (AudioManager.Instance != null && intensity > 0.5f)
                {
                    AudioManager.Instance.DistortMusic(1f - intensity * 0.5f, 0.1f);
                }

                // Screen flash near the end
                if (intensity > 0.8f && Random.value < 0.1f)
                {
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.FlashScreen(Color.white, 0.05f);
                    }
                }

                yield return null;
            }

            // Final dramatic flash
            if (UIManager.Instance != null)
            {
                UIManager.Instance.FlashScreen(Color.white, 0.2f);
            }
        }

        /// <summary>
        /// Activates a specific level layout and deactivates others.
        /// </summary>
        private void SetActiveLayout(GameObject targetLayout)
        {
            if (normalLevelLayout != null) normalLevelLayout.SetActive(normalLevelLayout == targetLayout);
            if (corruptedLevelLayout != null) corruptedLevelLayout.SetActive(corruptedLevelLayout == targetLayout);
            if (mirroredLevelLayout != null) mirroredLevelLayout.SetActive(mirroredLevelLayout == targetLayout);
        }

        protected override void OnDeathTroll()
        {
            string[] messages = crashCount switch
            {
                0 => new[] { "Dying in the normal part? Really?", "It hasn't even started yet..." },
                1 => new[] { "The corruption got you!", "Maybe the crash damaged your skills too." },
                _ => new[] { "Everything is backwards INCLUDING your skill.", "Mirror world, mirror deaths." }
            };

            ShowTrollMessage(messages[Random.Range(0, messages.Length)]);
        }
    }
}
