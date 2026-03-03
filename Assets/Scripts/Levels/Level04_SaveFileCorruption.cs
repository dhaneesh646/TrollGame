using System.Collections;
using System.IO;
using UnityEngine;
using TrollGame.Core;

namespace TrollGame.Levels
{
    /// <summary>
    /// LEVEL 4: "The Save File"
    /// 
    /// CONCEPT: The game pretends to delete/corrupt the player's save file.
    /// A dramatic progress bar shows "Deleting save data..." while the player
    /// panics. Then the level transforms into a platformer INSIDE the "corrupted"
    /// save file — the player literally navigates through floating data fragments.
    /// 
    /// CLIP MOMENT: The fake "deleting save file" progress bar with the player
    /// frantically trying to close it. Triple clip when the game creates a real
    /// (harmless) text file on their desktop that says "Your save is fine, relax."
    /// 
    /// SAFETY: Only creates files in Application.persistentDataPath (the game's
    /// own save directory). Never touches actual user data.
    /// </summary>
    public class Level04_SaveFileCorruption : LevelBase
    {
        [Header("Save File UI")]
        [SerializeField] private GameObject deletionWarningPanel;
        [SerializeField] private UnityEngine.UI.Text warningHeaderText;
        [SerializeField] private UnityEngine.UI.Text warningDetailText;
        [SerializeField] private UnityEngine.UI.Slider deletionProgressBar;
        [SerializeField] private UnityEngine.UI.Text progressPercentText;
        [SerializeField] private UnityEngine.UI.Button cancelButton;
        [SerializeField] private UnityEngine.UI.Button confirmButton;

        [Header("Corrupted World")]
        [SerializeField] private GameObject normalWorldContainer;
        [SerializeField] private GameObject corruptedWorldContainer;
        [SerializeField] private Transform corruptedSpawnPoint;

        [Header("Data Fragment Collectibles")]
        [SerializeField] private int totalFragments = 10;
        [SerializeField] private GameObject dataFragmentPrefab;

        [Header("Settings")]
        [SerializeField] private float deletionDuration = 15f;
        [SerializeField] private float panicBuildupTime = 5f;

        private int fragmentsCollected = 0;
        private bool deletionStarted = false;
        private bool deletionComplete = false;
        private bool playerTriedToCancel = false;
        private int cancelAttempts = 0;

        protected override void Awake()
        {
            base.Awake();
            levelName = "The Save File";
            primaryTrollType = TrollType.SaveCorruption;
            trollTriggerDelay = 10f;
        }

        protected override void OnLevelStart()
        {
            // Start with a normal-looking level
            if (normalWorldContainer != null) normalWorldContainer.SetActive(true);
            if (corruptedWorldContainer != null) corruptedWorldContainer.SetActive(false);
            if (deletionWarningPanel != null) deletionWarningPanel.SetActive(false);

            ShowTrollMessage("This level has an auto-save feature. Don't worry about it.");
        }

        protected override void OnTrollBegin()
        {
            StartCoroutine(SaveCorruptionSequence());
        }

        protected override bool CheckLevelComplete()
        {
            return fragmentsCollected >= totalFragments;
        }

        /// <summary>
        /// The main save corruption sequence.
        /// </summary>
        private IEnumerator SaveCorruptionSequence()
        {
            // Phase 1: Subtle warnings
            yield return StartCoroutine(SubtleWarningPhase());

            // Phase 2: The deletion warning popup
            yield return StartCoroutine(DeletionWarningPhase());

            // Phase 3: "Corruption" of the world
            yield return StartCoroutine(WorldCorruptionPhase());

            // Phase 4: Player must collect data fragments to "repair" the save
            ShowTrollMessage("Collect the data fragments to repair your save file!");
        }

        /// <summary>
        /// Phase 1: Subtle hints that something is wrong with the save file.
        /// </summary>
        private IEnumerator SubtleWarningPhase()
        {
            yield return new WaitForSeconds(2f);
            ShowTrollMessage("Auto-saving...");

            yield return new WaitForSeconds(3f);
            ShowTrollMessage("Auto-save failed. Retrying...");

            yield return new WaitForSeconds(2f);
            ShowTrollMessage("Auto-save failed again. Save file may be corrupted.");

            yield return new WaitForSeconds(panicBuildupTime);
            ShowTrollMessage("WARNING: Critical save file error detected!");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCriticalError();
            }
        }

        /// <summary>
        /// Phase 2: The dramatic deletion warning with fake progress bar.
        /// </summary>
        private IEnumerator DeletionWarningPhase()
        {
            // Show the deletion warning
            if (deletionWarningPanel != null) deletionWarningPanel.SetActive(true);

            string playerName = "Player";
            if (gameManager != null)
            {
                playerName = gameManager.PlayerName;
            }

            if (warningHeaderText != null)
            {
                warningHeaderText.text = "CRITICAL ERROR: SAVE FILE CORRUPTED";
            }
            if (warningDetailText != null)
            {
                warningDetailText.text = $"Save data for '{playerName}' is corrupted.\n" +
                    "All progress will be deleted to prevent system damage.\n\n" +
                    "This action cannot be undone.";
            }

            // Setup cancel button (which doesn't actually work)
            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveAllListeners();
                cancelButton.onClick.AddListener(OnCancelClicked);
            }
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(OnConfirmClicked);
            }

            // Wait a moment for panic to set in
            yield return new WaitForSeconds(3f);

            // Start the "deletion" (auto-starts even without clicking confirm)
            StartCoroutine(FakeDeletionProgress());

            ShowTrollMessage("Deletion starting in 3... 2... 1...");

            yield return new WaitForSeconds(deletionDuration + 3f);
        }

        /// <summary>
        /// The fake deletion progress bar with dramatic slowdowns.
        /// </summary>
        private IEnumerator FakeDeletionProgress()
        {
            deletionStarted = true;
            float elapsed = 0f;

            if (warningDetailText != null)
            {
                warningDetailText.text = "Deleting save data...";
            }

            while (elapsed < deletionDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / deletionDuration;

                // Dramatic progress manipulation
                float displayProgress;
                if (progress < 0.3f)
                {
                    displayProgress = progress * 1.5f; // Fast start
                }
                else if (progress < 0.6f)
                {
                    displayProgress = 0.45f + (progress - 0.3f) * 0.3f; // Slow middle
                }
                else if (progress < 0.9f)
                {
                    displayProgress = 0.54f + (progress - 0.6f) * 1.3f; // Speed up
                }
                else
                {
                    displayProgress = 0.93f + (progress - 0.9f) * 0.7f; // Agonizingly slow end
                }

                if (deletionProgressBar != null)
                {
                    deletionProgressBar.value = Mathf.Clamp01(displayProgress);
                }
                if (progressPercentText != null)
                {
                    int pct = Mathf.FloorToInt(displayProgress * 100f);
                    progressPercentText.text = $"{pct}%";
                }

                // Update deletion details with fake file names
                if (warningDetailText != null && Random.value < 0.05f)
                {
                    string[] fakeFiles = new string[]
                    {
                        "Deleting: player_progress.sav",
                        "Deleting: achievements.dat",
                        "Deleting: high_scores.db",
                        "Deleting: unlocked_levels.cfg",
                        "Deleting: player_dignity.exe",
                        "Deleting: skill_points.json",
                        "Deleting: good_memories.bak",
                        "Deleting: hours_of_progress.tmp",
                        $"Deleting: {playerName_cached}_secrets.txt"
                    };
                    warningDetailText.text = fakeFiles[Random.Range(0, fakeFiles.Length)];
                }

                yield return null;
            }

            deletionComplete = true;

            // "Deletion complete"
            if (warningDetailText != null)
            {
                warningDetailText.text = "Deletion complete!\n\n...Just kidding. Your save is fine.\nBut the level isn't.";
            }
            if (progressPercentText != null)
            {
                progressPercentText.text = "100% (trolled)";
            }

            yield return new WaitForSeconds(3f);

            // Hide the panel
            if (deletionWarningPanel != null) deletionWarningPanel.SetActive(false);
        }

        private string playerName_cached
        {
            get
            {
                if (gameManager != null) return gameManager.PlayerName;
                return "Player";
            }
        }

        /// <summary>
        /// Phase 3: The world transforms into a "corrupted" version.
        /// </summary>
        private IEnumerator WorldCorruptionPhase()
        {
            // Glitch transition
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowGlitchEffect(1.5f);
            }

            yield return new WaitForSeconds(1.5f);

            // Swap world layouts
            if (normalWorldContainer != null) normalWorldContainer.SetActive(false);
            if (corruptedWorldContainer != null) corruptedWorldContainer.SetActive(true);

            // Teleport player to corrupted world
            if (corruptedSpawnPoint != null)
            {
                Transform player = FindPlayerTransform();
                if (player != null)
                {
                    player.position = corruptedSpawnPoint.position;
                }
            }

            // Create the save file Easter egg
            CreateSaveFileEasterEgg();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SwitchToCreepyMusic();
            }

            ShowTrollMessage("Welcome to your corrupted save file. Collect the data fragments to fix it.");
        }

        /// <summary>
        /// Creates a harmless text file in the game's save directory as an Easter egg.
        /// </summary>
        private void CreateSaveFileEasterEgg()
        {
            try
            {
                string savePath = Application.persistentDataPath;
                string filePath = Path.Combine(savePath, "your_save_is_fine_relax.txt");

                string content = "=== TROLL GAME SAVE FILE STATUS ===\n\n" +
                    "Dear Player,\n\n" +
                    "Your save file was NEVER in danger.\n" +
                    "That whole deletion thing? Totally fake.\n\n" +
                    "But we got you, didn't we? :)\n\n" +
                    "P.S. If you're reading this file on your actual computer,\n" +
                    "that means the 4th wall is well and truly broken.\n\n" +
                    "- The TrollGame Development Team\n" +
                    "(Who are also probably watching you play right now)";

                File.WriteAllText(filePath, content);
            }
            catch (System.Exception)
            {
                // Silently fail — this is just an Easter egg, not critical
            }
        }

        /// <summary>
        /// Called when a data fragment is collected.
        /// </summary>
        public void OnDataFragmentCollected()
        {
            fragmentsCollected++;

            string[] messages = new string[]
            {
                $"Fragment {fragmentsCollected}/{totalFragments} collected!",
                $"Save file integrity: {(fragmentsCollected * 100 / totalFragments)}%",
                "This data fragment contains your... dignity? Nah, that was never saved.",
                "Fragment recovered: skill.dll (file was empty)",
                $"Only {totalFragments - fragmentsCollected} more to go!"
            };

            ShowTrollMessage(messages[Random.Range(0, messages.Length)]);

            if (CheckLevelComplete())
            {
                StartCoroutine(SaveRestoredSequence());
            }
        }

        /// <summary>
        /// Sequence when the save is "restored."
        /// </summary>
        private IEnumerator SaveRestoredSequence()
        {
            ShowTrollMessage("Save file restored! All data recovered!");

            yield return new WaitForSeconds(2f);

            ShowTrollMessage("...Except for one file: your_patience.exe. That one's gone forever.");

            yield return new WaitForSeconds(3f);

            OnLevelComplete();
        }

        private void OnCancelClicked()
        {
            cancelAttempts++;

            string[] cancelResponses = new string[]
            {
                "Cancel is not available during deletion.",
                "Nice try. Deletion continues.",
                "The cancel button is purely decorative.",
                "Pressing cancel harder won't make it work.",
                "ERROR: Cancel.exe has been deleted.",
                $"That's {cancelAttempts} cancel attempts. None of them worked."
            };

            int index = Mathf.Min(cancelAttempts - 1, cancelResponses.Length - 1);
            ShowTrollMessage(cancelResponses[index]);
        }

        private void OnConfirmClicked()
        {
            ShowTrollMessage("You... you CONFIRMED the deletion?! Who does that?!");
        }

        private Transform FindPlayerTransform()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            return player != null ? player.transform : null;
        }

        protected override void OnDeathTroll()
        {
            if (deletionComplete)
            {
                ShowTrollMessage("Death data corrupted. You died in a way we didn't program. Impressive.");
            }
            else
            {
                ShowTrollMessage("Can't save your death. The save file is being deleted, remember?");
            }
        }
    }
}
