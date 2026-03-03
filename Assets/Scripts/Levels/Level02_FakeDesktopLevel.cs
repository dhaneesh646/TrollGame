using System.Collections;
using UnityEngine;
using TrollGame.Core;
using TrollGame.FourthWall;

namespace TrollGame.Levels
{
    /// <summary>
    /// LEVEL 2: "The Desktop"
    /// 
    /// CONCEPT: After completing the tutorial, the game appears to "crash" and
    /// minimize. The player sees their "desktop" — but it's actually a fake
    /// desktop rendered entirely inside the game. The level IS the desktop.
    /// Players must interact with fake desktop icons to progress.
    /// 
    /// CLIP MOMENT: The instant the player realizes the "desktop" is the game.
    /// Double clip when they click "totally_not_a_virus.exe" and errors cascade.
    /// 
    /// INSPIRATION: Pony Island, There Is No Game
    /// </summary>
    public class Level02_FakeDesktopLevel : LevelBase
    {
        [Header("Desktop References")]
        [SerializeField] private FakeDesktopController fakeDesktop;
        [SerializeField] private FakeBSODController fakeBSOD;
        [SerializeField] private FakeErrorPopupController fakeErrors;

        [Header("Level Progression")]
        [SerializeField] private int requiredIconClicks = 3;
        [SerializeField] private float revealDelay = 30f;

        private int correctIconsClicked = 0;
        private bool desktopRevealed = false;
        private bool transitionStarted = false;

        protected override void Awake()
        {
            base.Awake();
            levelName = "The Desktop";
            primaryTrollType = TrollType.FakeDesktop;
            trollTriggerDelay = 0f; // Starts immediately
            autoStartTrolling = false; // We handle the sequence manually
        }

        protected override void OnLevelStart()
        {
            StartCoroutine(DesktopLevelSequence());
        }

        protected override void OnTrollBegin()
        {
            // Trolling is built into the desktop interaction itself
        }

        protected override bool CheckLevelComplete()
        {
            return correctIconsClicked >= requiredIconClicks;
        }

        /// <summary>
        /// Main sequence for the fake desktop level.
        /// </summary>
        private IEnumerator DesktopLevelSequence()
        {
            // Step 1: Fake crash
            yield return StartCoroutine(FakeCrashSequence());

            // Step 2: Show fake desktop
            if (fakeDesktop != null)
            {
                fakeDesktop.ShowFakeDesktop();
            }

            // Step 3: Wait for player to explore
            yield return new WaitForSeconds(5f);

            // Step 4: Subtle hint that something is wrong
            ShowTrollMessage("Huh, the game crashed... or did it?");

            // Step 5: Wait for player to interact with icons
            yield return new WaitUntil(() => correctIconsClicked >= 1);

            ShowTrollMessage("Wait... this desktop looks a little... off.");

            yield return new WaitUntil(() => correctIconsClicked >= 2);

            ShowTrollMessage("You're still playing the game, you know.");

            yield return new WaitUntil(() => correctIconsClicked >= requiredIconClicks || GetLevelPlayTime() > revealDelay);

            // Step 6: The reveal
            yield return StartCoroutine(DesktopRevealSequence());
        }

        /// <summary>
        /// Simulates the game crashing.
        /// </summary>
        private IEnumerator FakeCrashSequence()
        {
            // Screen glitch
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowGlitchEffect(0.5f);
            }

            yield return new WaitForSeconds(0.5f);

            // Screen goes black
            if (UIManager.Instance != null)
            {
                UIManager.Instance.FadeToBlack(0.3f, 2f, 0f);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.VinylScratch();
            }

            yield return new WaitForSeconds(2.5f);

            // Brief BSOD flash
            if (fakeBSOD != null)
            {
                fakeBSOD.FlashBSOD(0.3f);
            }

            yield return new WaitForSeconds(0.5f);
        }

        /// <summary>
        /// The dramatic reveal that the desktop is actually the game.
        /// </summary>
        private IEnumerator DesktopRevealSequence()
        {
            if (desktopRevealed) yield break;
            desktopRevealed = true;

            // Glitch effects
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowGlitchEffect(1f);
            }

            yield return new WaitForSeconds(1f);

            // The desktop starts "breaking apart"
            ShowTrollMessage("Did you really think you escaped the game?");

            yield return new WaitForSeconds(2f);

            ShowTrollMessage("The desktop was the game all along!");

            yield return new WaitForSeconds(2f);

            // Transition back to "normal" game
            if (fakeDesktop != null)
            {
                fakeDesktop.HideFakeDesktop();
            }

            // Flash and transition
            if (UIManager.Instance != null)
            {
                UIManager.Instance.FlashScreen(Color.white, 0.3f);
            }

            yield return new WaitForSeconds(1f);

            // Level complete
            OnLevelComplete();
        }

        /// <summary>
        /// Called by the fake desktop when the player clicks specific icons.
        /// </summary>
        public void OnCorrectIconClicked()
        {
            correctIconsClicked++;

            if (CheckLevelComplete() && !transitionStarted)
            {
                transitionStarted = true;
                StartCoroutine(DesktopRevealSequence());
            }
        }

        /// <summary>
        /// Called when the player tries to "close" the game from the desktop.
        /// </summary>
        public void OnPlayerTriedToCloseFromDesktop()
        {
            if (fakeErrors != null)
            {
                fakeErrors.SpawnError(
                    "Nice Try",
                    "You can't close a game that IS the desktop.",
                    ErrorStyle.Error
                );
            }
        }

        /// <summary>
        /// Called when the player right-clicks on the desktop.
        /// </summary>
        public void OnDesktopRightClick()
        {
            ShowTrollMessage("Right-clicking won't help you here.");
        }

        protected override void OnDeathTroll()
        {
            // You can't really die on the desktop level, but just in case
            ShowTrollMessage("How did you even die on a desktop?");
        }
    }
}
