using System.Collections;
using System.IO;
using UnityEngine;
using TrollGame.Core;
using TrollGame.FourthWall;

namespace TrollGame.Levels
{
    /// <summary>
    /// LEVEL 10: "The End...?"
    /// 
    /// CONCEPT: The ultimate meta-troll. The game presents multiple fake endings,
    /// each more absurd than the last. When the player thinks it's TRULY over,
    /// the game does one final surprise. The credit sequence is itself a playable
    /// level where you dodge the names of the developers.
    /// 
    /// FAKE ENDING 1: Normal victory screen → game "crashes" → restarts from Level 1
    /// FAKE ENDING 2: Emotional goodbye from the narrator → psyche, more platforming
    /// FAKE ENDING 3: The game "uninstalls itself" (fake) → shows desktop → gotcha
    /// REAL ENDING: A genuine heartfelt message thanking the player, then one
    ///              final harmless troll (the game writes a thank-you file).
    /// 
    /// CLIP MOMENTS: Every single fake ending transition. The "uninstall" scare.
    /// The genuine emotional moment at the ACTUAL end.
    /// </summary>
    public class Level10_FinalTroll : LevelBase
    {
        [Header("Ending UI")]
        [SerializeField] private GameObject victoryScreenPanel;
        [SerializeField] private GameObject creditsPanel;
        [SerializeField] private GameObject emotionalPanel;
        [SerializeField] private GameObject uninstallPanel;
        [SerializeField] private UnityEngine.UI.Text mainText;
        [SerializeField] private UnityEngine.UI.Text subText;
        [SerializeField] private UnityEngine.UI.Slider uninstallProgressBar;
        [SerializeField] private UnityEngine.UI.Text uninstallProgressText;

        [Header("4th Wall References")]
        [SerializeField] private FakeBSODController bsodController;
        [SerializeField] private FakeDesktopController desktopController;
        [SerializeField] private WindowManipulator windowManipulator;

        [Header("Playable Credits")]
        [SerializeField] private Transform creditsStartPoint;
        [SerializeField] private Transform creditsEndPoint;
        [SerializeField] private GameObject creditTextPrefab;
        [SerializeField] private Transform playerTransform;

        [Header("Settings")]
        [SerializeField] private float fakeEndingPause = 5f;

        private int fakeEndingCount = 0;
        private bool realEndingReached = false;

        protected override void Awake()
        {
            base.Awake();
            levelName = "The End...?";
            primaryTrollType = TrollType.FakeEnding;
            trollTriggerDelay = 0f;
            autoStartTrolling = false;
        }

        protected override void OnLevelStart()
        {
            HideAllEndingPanels();
            StartCoroutine(FinalLevelSequence());
        }

        protected override void OnTrollBegin() { }

        protected override bool CheckLevelComplete()
        {
            return realEndingReached;
        }

        /// <summary>
        /// The main sequence of fake endings leading to the real one.
        /// </summary>
        private IEnumerator FinalLevelSequence()
        {
            ShowTrollMessage("You've made it to the final level. This is really the end.");

            yield return new WaitForSeconds(3f);

            ShowTrollMessage("...or IS it?");

            yield return new WaitForSeconds(2f);

            // Fake Ending 1: Normal Victory
            yield return StartCoroutine(FakeEnding1_NormalVictory());

            // Fake Ending 2: Emotional Goodbye
            yield return StartCoroutine(FakeEnding2_EmotionalGoodbye());

            // Fake Ending 3: Fake Uninstall
            yield return StartCoroutine(FakeEnding3_FakeUninstall());

            // The REAL ending
            yield return StartCoroutine(RealEnding());
        }

        /// <summary>
        /// Fake Ending 1: A standard victory screen that "crashes."
        /// </summary>
        private IEnumerator FakeEnding1_NormalVictory()
        {
            fakeEndingCount++;

            // Show victory screen
            if (victoryScreenPanel != null) victoryScreenPanel.SetActive(true);
            SetMainText("CONGRATULATIONS!");
            SetSubText("You have completed TrollGame!\n\nThank you for playing!");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayVictoryFanfare();
            }

            yield return new WaitForSeconds(fakeEndingPause);

            // "Crash"
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowGlitchEffect(0.5f);
            }

            yield return new WaitForSeconds(0.5f);

            if (victoryScreenPanel != null) victoryScreenPanel.SetActive(false);

            if (bsodController != null)
            {
                bsodController.TriggerBSOD("ENDING_NOT_FOUND_EXCEPTION");
            }
            else if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFakeBSOD(6f, "ENDING_NOT_FOUND_EXCEPTION");
            }

            yield return new WaitForSecondsRealtime(10f);

            ShowTrollMessage("Did you really think it would be THAT easy?");

            yield return new WaitForSeconds(3f);

            ShowTrollMessage("Fake Ending #1 complete. How many more can there be?");

            yield return new WaitForSeconds(3f);
        }

        /// <summary>
        /// Fake Ending 2: An emotional, heartfelt goodbye that gets ruined.
        /// </summary>
        private IEnumerator FakeEnding2_EmotionalGoodbye()
        {
            fakeEndingCount++;

            if (emotionalPanel != null) emotionalPanel.SetActive(true);

            string playerName = gameManager != null ? gameManager.PlayerName : "Player";
            int totalDeaths = gameManager != null ? gameManager.PlayerDeathCount : 0;

            // Start emotional
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopMusic(1f);
            }

            yield return new WaitForSeconds(2f);

            SetMainText($"Dear {playerName},");
            SetSubText("");

            yield return new WaitForSeconds(2f);

            SetSubText("Thank you for playing TrollGame.");

            yield return new WaitForSeconds(3f);

            SetSubText($"You died {totalDeaths} times. And yet you persevered.");

            yield return new WaitForSeconds(3f);

            SetSubText("Through lying tutorials, fake crashes, and impossible platforms...");

            yield return new WaitForSeconds(3f);

            SetSubText("You kept going. That takes real determination.");

            yield return new WaitForSeconds(3f);

            SetSubText("And for that, we want to say...");

            yield return new WaitForSeconds(3f);

            // THE TWIST
            SetMainText("PSYCHE!");
            SetSubText("YOU THOUGHT WE'D BE NICE?!\nThere's still more game! HA!");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayTrollLaugh();
                AudioManager.Instance.SwitchToTrollMusic();
            }

            yield return new WaitForSeconds(4f);

            if (emotionalPanel != null) emotionalPanel.SetActive(false);

            ShowTrollMessage("Fake Ending #2. Getting warmer...");

            yield return new WaitForSeconds(3f);
        }

        /// <summary>
        /// Fake Ending 3: The game pretends to uninstall itself.
        /// </summary>
        private IEnumerator FakeEnding3_FakeUninstall()
        {
            fakeEndingCount++;

            if (uninstallPanel != null) uninstallPanel.SetActive(true);

            SetMainText("CRITICAL ERROR");
            SetSubText("TrollGame has become self-aware and has decided to uninstall itself.\n" +
                "This action cannot be stopped.");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCriticalError();
                AudioManager.Instance.StopMusic(0.5f);
            }

            yield return new WaitForSeconds(3f);

            // Fake uninstall progress
            SetMainText("Uninstalling TrollGame...");

            float duration = 12f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / duration;

                if (uninstallProgressBar != null) uninstallProgressBar.value = progress;

                // Fake file names being "deleted"
                string[] fakeFiles = new string[]
                {
                    "Removing: trolling_engine.dll",
                    "Removing: player_suffering.dat",
                    "Removing: fake_endings.pkg",
                    "Removing: meme_database.db",
                    "Removing: rage_tracking.sys",
                    "Removing: hope.exe (not found)",
                    "Removing: fun.dll (was never installed)",
                    "Removing: your_sanity.bak",
                    "Removing: streamer_clips.mp4",
                    "Removing: final_boss_tears.wav"
                };

                int fileIndex = Mathf.FloorToInt(progress * fakeFiles.Length);
                fileIndex = Mathf.Min(fileIndex, fakeFiles.Length - 1);

                SetSubText(fakeFiles[fileIndex]);

                if (uninstallProgressText != null)
                {
                    int pct = Mathf.FloorToInt(progress * 100f);
                    uninstallProgressText.text = $"{pct}%";
                }

                yield return null;
            }

            // "Uninstall complete"
            SetMainText("Uninstall Complete");
            SetSubText("TrollGame has been removed from your system.\n\nGoodbye forever.");

            // Window manipulation for extra drama
            if (windowManipulator != null)
            {
                windowManipulator.FakeCloseWindow();
            }

            yield return new WaitForSecondsRealtime(3f);

            // SURPRISE! Still here!
            if (uninstallPanel != null) uninstallPanel.SetActive(false);

            if (windowManipulator != null)
            {
                windowManipulator.RestoreWindowPosition();
            }

            ShowTrollMessage("...Just kidding. You can't uninstall me. I'm part of you now.");

            yield return new WaitForSeconds(3f);

            ShowTrollMessage("Fake Ending #3. Okay, the REAL ending is next. For real this time.");

            yield return new WaitForSeconds(3f);
        }

        /// <summary>
        /// The REAL ending — genuine appreciation with one final harmless troll.
        /// </summary>
        private IEnumerator RealEnding()
        {
            realEndingReached = true;

            HideAllEndingPanels();
            if (emotionalPanel != null) emotionalPanel.SetActive(true);

            string playerName = gameManager != null ? gameManager.PlayerName : "Player";
            int totalDeaths = gameManager != null ? gameManager.PlayerDeathCount : 0;
            float totalTime = gameManager != null ? gameManager.TotalPlayTime : 0f;
            int minutes = Mathf.FloorToInt(totalTime / 60f);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopMusic(1f);
            }

            yield return new WaitForSeconds(2f);

            // This time it's real
            SetMainText("The Real Ending");
            SetSubText("No tricks this time. Promise.");

            yield return new WaitForSeconds(4f);

            SetSubText($"{playerName}, you played for {minutes} minutes and died {totalDeaths} times.");

            yield return new WaitForSeconds(4f);

            SetSubText("And through all the trolling, the fake crashes, the broken tutorials,\n" +
                "the lying progress bars, and the window chaos...");

            yield return new WaitForSeconds(4f);

            SetSubText("You never gave up.");

            yield return new WaitForSeconds(3f);

            SetSubText("This game was built to test your patience.\nAnd you passed.");

            yield return new WaitForSeconds(4f);

            SetMainText("Thank You");
            SetSubText("Seriously. Thank you for playing.\n\n" +
                "Now go leave us a Steam review.\n" +
                "(A real one this time. The boss isn't writing it for you.)");

            yield return new WaitForSeconds(5f);

            // One final harmless troll — create a thank you file
            CreateThankYouFile(playerName, totalDeaths, minutes);

            SetSubText("P.S. Check your game folder. We left you something. :)");

            yield return new WaitForSeconds(3f);

            // Playable credits
            SetMainText("CREDITS");
            SetSubText("Dodge the credits to unlock the secret ending!\n\n(There is no secret ending. Or is there?)");

            yield return new WaitForSeconds(5f);

            // Show final stats
            SetMainText("FINAL STATS");
            SetSubText($"Player: {playerName}\n" +
                $"Total Deaths: {totalDeaths}\n" +
                $"Play Time: {minutes} minutes\n" +
                $"Fake Endings Survived: {fakeEndingCount}\n" +
                $"Times Alt-Tabbed: (We were counting)\n" +
                $"Rage Level: Maximum\n" +
                $"Overall Rating: You're a legend.\n\n" +
                $"THE END\n" +
                $"(For real this time)");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayVictoryFanfare();
            }

            yield return new WaitForSeconds(10f);

            OnLevelComplete();
        }

        /// <summary>
        /// Creates a harmless thank-you text file in the game's save directory.
        /// </summary>
        private void CreateThankYouFile(string playerName, int totalDeaths, int minutes)
        {
            try
            {
                string savePath = Application.persistentDataPath;
                string filePath = Path.Combine(savePath, "thank_you_for_playing.txt");

                string content = $"╔══════════════════════════════════════╗\n" +
                    $"║     TROLLGAME - CERTIFICATE OF       ║\n" +
                    $"║        COMPLETION                    ║\n" +
                    $"╠══════════════════════════════════════╣\n" +
                    $"║                                      ║\n" +
                    $"║  This certifies that:                ║\n" +
                    $"║  {playerName,-36} ║\n" +
                    $"║                                      ║\n" +
                    $"║  Has survived TrollGame              ║\n" +
                    $"║  with {totalDeaths} deaths in {minutes} minutes.     ║\n" +
                    $"║                                      ║\n" +
                    $"║  This is a real achievement.         ║\n" +
                    $"║  Frame this. Put it on your wall.    ║\n" +
                    $"║  Tell your grandchildren.            ║\n" +
                    $"║                                      ║\n" +
                    $"║  Thank you for playing.              ║\n" +
                    $"║  We hope we trolled you well.        ║\n" +
                    $"║                                      ║\n" +
                    $"║  - The TrollGame Team                ║\n" +
                    $"╚══════════════════════════════════════╝\n\n" +
                    $"P.S. Your save file was always safe. We promise.\n" +
                    $"P.P.S. Or WAS it? (It was. We're not monsters.)\n";

                File.WriteAllText(filePath, content);
            }
            catch (System.Exception)
            {
                // Easter egg creation is non-critical
            }
        }

        private void HideAllEndingPanels()
        {
            if (victoryScreenPanel != null) victoryScreenPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
            if (emotionalPanel != null) emotionalPanel.SetActive(false);
            if (uninstallPanel != null) uninstallPanel.SetActive(false);
        }

        private void SetMainText(string text)
        {
            if (mainText != null) mainText.text = text;
        }

        private void SetSubText(string text)
        {
            if (subText != null) subText.text = text;
        }

        protected override void OnDeathTroll()
        {
            ShowTrollMessage("Dying during the finale? The credits will remember this.");
        }
    }
}
