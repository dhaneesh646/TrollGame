using System.Collections;
using UnityEngine;
using TrollGame.Core;

namespace TrollGame.Levels
{
    /// <summary>
    /// LEVEL 1: "The Tutorial That Lies"
    /// 
    /// CONCEPT: The game starts with a seemingly normal tutorial that teaches
    /// the player completely wrong controls. Jump is actually crouch.
    /// WASD is remapped randomly. The tutorial text actively gaslights the player.
    /// 
    /// CLIP MOMENT: The exact frame where the player realizes they've been lied to.
    /// Streamers will scream "WAIT WHAT?!" guaranteed.
    /// 
    /// DESIGN PHILOSOPHY: Start normal, build trust, then shatter it.
    /// The first 30 seconds look like any generic platformer tutorial.
    /// </summary>
    public class Level01_LyingTutorial : LevelBase
    {
        [Header("Tutorial UI")]
        [SerializeField] private GameObject tutorialPromptUI;
        [SerializeField] private UnityEngine.UI.Text tutorialText;
        [SerializeField] private UnityEngine.UI.Text subtitleText;

        [Header("Tutorial Phases")]
        [SerializeField] private float phaseDelay = 5f;

        [Header("Player Reference")]
        [SerializeField] private FirstPersonController playerController;
        [SerializeField] private Rigidbody playerRb;

        private int currentPhase = 0;
        private bool controlsSwapped = false;
        private bool gravityFlipped = false;
        private float originalWalkSpeed;
        private float originalJumpPower;
        private float originalMouseSensitivity;

        // Store original control mappings
        private KeyCode originalJumpKey;
        private KeyCode originalCrouchKey;

        protected override void Awake()
        {
            base.Awake();
            levelName = "The Tutorial";
            primaryTrollType = TrollType.ControlSwap;
            trollTriggerDelay = 15f; // Give them 15 seconds of normal gameplay first
        }

        protected override void Start()
        {
            base.Start();

            // Save original settings
            if (playerController != null)
            {
                originalWalkSpeed = playerController.walkSpeed;
                originalJumpPower = playerController.jumpPower;
                originalMouseSensitivity = playerController.mouseSensitivity;
                originalJumpKey = playerController.jumpKey;
                originalCrouchKey = playerController.crouchKey;
            }
        }

        protected override void OnLevelStart()
        {
            // Phase 0: Completely normal tutorial
            ShowTutorialPrompt("Welcome to the Tutorial!", "Use WASD to move around.");
            StartCoroutine(TutorialSequence());
        }

        protected override void OnTrollBegin()
        {
            // Phase 3+: Start lying
            StartCoroutine(LyingPhase());
        }

        protected override void OnTrollUpdate()
        {
            // Continuously mess with controls based on rage level
            if (gameManager != null && gameManager.PlayerRageLevel > 3 && !gravityFlipped)
            {
                StartCoroutine(GravityTroll());
            }
        }

        protected override bool CheckLevelComplete()
        {
            // Level completes when player reaches the exit despite the trolling
            return levelComplete;
        }

        /// <summary>
        /// The normal tutorial sequence before the trolling starts.
        /// </summary>
        private IEnumerator TutorialSequence()
        {
            // Phase 1: Movement (honest)
            yield return new WaitForSeconds(phaseDelay);
            ShowTutorialPrompt("Great job!", "Now try jumping with SPACE.");
            currentPhase = 1;

            // Phase 2: Jumping (still honest)
            yield return new WaitForSeconds(phaseDelay);
            ShowTutorialPrompt("Perfect!", "Use LEFT SHIFT to sprint.");
            currentPhase = 2;

            // Phase 3: The twist begins...
            yield return new WaitForSeconds(phaseDelay);
            ShowTutorialPrompt("Now for advanced controls...",
                "Press SPACE to activate your shield!");
            currentPhase = 3;
            // (There is no shield. SPACE still jumps. The lie begins.)
        }

        /// <summary>
        /// The lying phase - tutorial starts giving wrong information.
        /// </summary>
        private IEnumerator LyingPhase()
        {
            // Lie 1: Swap jump and crouch
            ShowTutorialPrompt("IMPORTANT UPDATE",
                "Controls have been optimized!\nSPACE = Crouch\nCTRL = Jump");
            SwapJumpAndCrouch();
            yield return new WaitForSeconds(phaseDelay);

            // Lie 2: Invert mouse
            ShowTutorialPrompt("Mouse Calibration",
                "Adjusting mouse sensitivity for optimal gameplay...");
            InvertMouse();
            yield return new WaitForSeconds(phaseDelay);

            // Lie 3: Speed manipulation
            ShowTutorialPrompt("Performance Mode Activated",
                "Game speed optimized! (You're welcome)");
            RandomizeSpeed();
            yield return new WaitForSeconds(phaseDelay);

            // Lie 4: Gaslight the player
            ShowTutorialPrompt("Hmm...",
                "Are you sure you're pressing the right buttons?\nThe controls haven't changed.");
            yield return new WaitForSeconds(phaseDelay);

            // Lie 5: Pretend to fix it
            ShowTutorialPrompt("Okay, we fixed the controls.",
                "Everything should be normal now. Trust us.");
            // (Don't actually fix anything)
            yield return new WaitForSeconds(phaseDelay);

            // Lie 6: Make it worse
            ShowTutorialPrompt("Actually...",
                "We lied. Here's your REAL controls:");
            MakeControlsChaotic();
            yield return new WaitForSeconds(3f);

            // Final message
            ShowTutorialPrompt("Tutorial Complete!",
                "Congratulations! You've learned nothing.\nGood luck!");
            yield return new WaitForSeconds(3f);

            // Actually restore controls (or do we?)
            if (Random.value > 0.5f)
            {
                RestoreControls();
                ShowTutorialPrompt("", "Fine, here are your real controls back. You're welcome.");
            }
            else
            {
                ShowTutorialPrompt("", "No, we're keeping the broken controls. Deal with it.");
            }
        }

        /// <summary>
        /// Swaps the jump and crouch keys.
        /// </summary>
        private void SwapJumpAndCrouch()
        {
            if (playerController == null) return;

            playerController.jumpKey = KeyCode.LeftControl;
            playerController.crouchKey = KeyCode.Space;
            controlsSwapped = true;
        }

        /// <summary>
        /// Inverts the mouse controls.
        /// </summary>
        private void InvertMouse()
        {
            if (playerController == null) return;
            playerController.invertCamera = !playerController.invertCamera;
        }

        /// <summary>
        /// Randomizes the player's movement speed.
        /// </summary>
        private void RandomizeSpeed()
        {
            if (playerController == null) return;

            // Randomly make them super fast or painfully slow
            if (Random.value > 0.5f)
            {
                playerController.walkSpeed = originalWalkSpeed * 3f; // Sonic speed
            }
            else
            {
                playerController.walkSpeed = originalWalkSpeed * 0.2f; // Snail mode
            }
        }

        /// <summary>
        /// Makes controls completely chaotic.
        /// </summary>
        private void MakeControlsChaotic()
        {
            if (playerController == null) return;

            // Random mouse sensitivity
            playerController.mouseSensitivity = Random.Range(0.1f, 10f);

            // Random speed
            playerController.walkSpeed = Random.Range(1f, 15f);

            // Random jump power
            playerController.jumpPower = Random.Range(1f, 20f);
        }

        /// <summary>
        /// Actually restores controls to normal.
        /// </summary>
        private void RestoreControls()
        {
            if (playerController == null) return;

            playerController.walkSpeed = originalWalkSpeed;
            playerController.jumpPower = originalJumpPower;
            playerController.mouseSensitivity = originalMouseSensitivity;
            playerController.jumpKey = originalJumpKey;
            playerController.crouchKey = originalCrouchKey;
            playerController.invertCamera = false;
            controlsSwapped = false;
        }

        /// <summary>
        /// Flips gravity for the player (the ultimate confusion).
        /// </summary>
        private IEnumerator GravityTroll()
        {
            gravityFlipped = true;
            ShowTutorialPrompt("GRAVITY MALFUNCTION", "Oops. We broke physics.");

            Physics.gravity = new Vector3(0, 9.81f, 0); // Reversed gravity

            yield return new WaitForSeconds(5f);

            Physics.gravity = new Vector3(0, -9.81f, 0); // Back to normal
            gravityFlipped = false;

            ShowTutorialPrompt("", "Fixed it. Probably.");
        }

        /// <summary>
        /// Shows a tutorial prompt on screen.
        /// </summary>
        private void ShowTutorialPrompt(string title, string content)
        {
            if (tutorialPromptUI != null)
            {
                tutorialPromptUI.SetActive(!string.IsNullOrEmpty(title) || !string.IsNullOrEmpty(content));
            }
            if (tutorialText != null) tutorialText.text = title;
            if (subtitleText != null) subtitleText.text = content;
        }

        protected override void OnDeathTroll()
        {
            string[] deathMessages = new string[]
            {
                "The tutorial warned you... oh wait, it lied.",
                "Maybe try pressing the CORRECT button next time.\n(We won't tell you which one)",
                "Deaths in tutorial: " + playerDeathsThisLevel + "\nThat's a new record! (It's not)",
                "Have you considered that maybe the floor is the enemy?",
                "Pro tip: Don't die. (This tip is actually honest)"
            };

            ShowTrollMessage(deathMessages[Random.Range(0, deathMessages.Length)]);
        }
    }
}
