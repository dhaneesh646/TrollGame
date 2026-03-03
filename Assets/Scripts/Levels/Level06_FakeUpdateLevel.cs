using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TrollGame.Core;

namespace TrollGame.Levels
{
    /// <summary>
    /// LEVEL 6: "The Update"
    /// 
    /// CONCEPT: The game suddenly shows a "mandatory update" screen with a 
    /// progress bar. But the progress bar IS the level — the player must 
    /// platformer ALONG the progress bar to advance it. Standing still means
    /// the download pauses. Walking backward makes it lose progress.
    /// 
    /// CLIP MOMENT: When the player realizes they're INSIDE the progress bar.
    /// Double clip when the progress goes backward for the first time.
    /// "Wait, is the progress bar the LEVEL?!"
    /// 
    /// TWIST: At 99%, the "update" fails and restarts at 0%. But now the
    /// progress bar level has new obstacles.
    /// </summary>
    public class Level06_FakeUpdateLevel : LevelBase
    {
        [Header("Update UI")]
        [SerializeField] private GameObject updateOverlayPanel;
        [SerializeField] private Slider downloadProgressBar;
        [SerializeField] private Text downloadPercentText;
        [SerializeField] private Text downloadSpeedText;
        [SerializeField] private Text downloadSizeText;
        [SerializeField] private Text updateTitleText;
        [SerializeField] private Text changelogText;

        [Header("Progress Bar Level")]
        [SerializeField] private Transform progressBarStartPoint;
        [SerializeField] private Transform progressBarEndPoint;
        [SerializeField] private Transform playerTransform;

        [Header("Obstacle Waves")]
        [SerializeField] private GameObject[] wave1Obstacles;
        [SerializeField] private GameObject[] wave2Obstacles;
        [SerializeField] private GameObject[] wave3Obstacles;

        [Header("Settings")]
        [SerializeField] private float progressBarLength = 100f;
        [SerializeField] private float downloadSizeGB = 47.3f;

        private float currentProgress = 0f;
        private int attemptCount = 0;
        private bool downloadFailed = false;
        private float fakeDownloadSpeed = 0f;

        protected override void Awake()
        {
            base.Awake();
            levelName = "The Update";
            primaryTrollType = TrollType.FakeUpdate;
            trollTriggerDelay = 0f;
            autoStartTrolling = false;
        }

        protected override void OnLevelStart()
        {
            StartCoroutine(FakeUpdateSequence());
        }

        protected override void OnTrollBegin() { }

        protected override void Update()
        {
            base.Update();

            if (playerTransform == null || progressBarStartPoint == null || progressBarEndPoint == null)
                return;

            // Calculate player progress along the bar
            Vector3 startToEnd = progressBarEndPoint.position - progressBarStartPoint.position;
            Vector3 startToPlayer = playerTransform.position - progressBarStartPoint.position;
            float progress = Vector3.Dot(startToPlayer, startToEnd.normalized) / startToEnd.magnitude;
            progress = Mathf.Clamp01(progress);

            // Update the progress bar based on player position
            currentProgress = progress;
            UpdateDownloadUI();
        }

        protected override bool CheckLevelComplete()
        {
            return currentProgress >= 0.99f && attemptCount >= 1;
        }

        /// <summary>
        /// The initial fake update screen before the player realizes what's happening.
        /// </summary>
        private IEnumerator FakeUpdateSequence()
        {
            // Show fake update notification
            if (updateOverlayPanel != null) updateOverlayPanel.SetActive(true);

            if (updateTitleText != null)
            {
                updateTitleText.text = "MANDATORY UPDATE REQUIRED";
            }

            if (changelogText != null)
            {
                changelogText.text = "TrollGame v2.0.0 Changelog:\n\n" +
                    "- Added more trolling\n" +
                    "- Removed player enjoyment\n" +
                    "- Fixed bug where player was having fun\n" +
                    "- Increased suffering by 200%\n" +
                    "- Added 47.3 GB of 'essential' data\n" +
                    "- Optimized disappointment delivery\n" +
                    "- New feature: This update screen is actually a level";
            }

            if (downloadSizeText != null)
            {
                downloadSizeText.text = $"Download size: {downloadSizeGB} GB";
            }

            // Fake a slow start
            float fakeProgress = 0f;
            float fakeTime = 0f;

            while (fakeTime < 8f)
            {
                fakeTime += Time.deltaTime;

                // Erratic early progress
                if (fakeTime < 3f)
                {
                    fakeProgress = 0f; // Stuck at 0
                    fakeDownloadSpeed = 0f;
                    if (downloadSpeedText != null) downloadSpeedText.text = "Connecting...";
                }
                else if (fakeTime < 6f)
                {
                    fakeProgress = (fakeTime - 3f) / 30f; // Painfully slow
                    fakeDownloadSpeed = Random.Range(0.1f, 0.5f);
                    if (downloadSpeedText != null) downloadSpeedText.text = $"{fakeDownloadSpeed:F1} MB/s";
                }
                else
                {
                    fakeProgress = 0.1f;
                    fakeDownloadSpeed = Random.Range(0.01f, 0.1f);
                    if (downloadSpeedText != null) downloadSpeedText.text = $"{fakeDownloadSpeed:F2} MB/s (throttled)";
                }

                UpdateProgressBarUI(fakeProgress);
                yield return null;
            }

            // The reveal
            ShowTrollMessage("Wait... the download speed matches your walking speed...");

            yield return new WaitForSeconds(3f);

            ShowTrollMessage("THE PROGRESS BAR IS THE LEVEL! Walk forward to download!");

            // Hide the overlay but keep the progress bar visible
            // Player now controls the progress through movement
            attemptCount++;
            ActivateObstacleWave(1);
        }

        /// <summary>
        /// Updates the download UI based on player position.
        /// </summary>
        private void UpdateDownloadUI()
        {
            UpdateProgressBarUI(currentProgress);

            // Calculate fake download speed based on player velocity
            if (playerTransform != null)
            {
                Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    fakeDownloadSpeed = rb.linearVelocity.magnitude * 10f;
                }
            }

            if (downloadSpeedText != null)
            {
                if (fakeDownloadSpeed < 0.1f)
                {
                    downloadSpeedText.text = "Download paused (MOVE!)";
                }
                else
                {
                    downloadSpeedText.text = $"{fakeDownloadSpeed:F1} MB/s";
                }
            }

            // Check for the 99% troll on first attempt
            if (currentProgress >= 0.99f && attemptCount == 1 && !downloadFailed)
            {
                StartCoroutine(DownloadFailedTroll());
            }

            // Check for completion on second attempt
            if (currentProgress >= 0.99f && attemptCount >= 2)
            {
                StartCoroutine(DownloadCompleteSequence());
            }
        }

        private void UpdateProgressBarUI(float progress)
        {
            if (downloadProgressBar != null)
            {
                downloadProgressBar.value = progress;
            }
            if (downloadPercentText != null)
            {
                int pct = Mathf.FloorToInt(progress * 100f);
                downloadPercentText.text = $"{pct}%";
            }
        }

        /// <summary>
        /// The dreaded 99% failure.
        /// </summary>
        private IEnumerator DownloadFailedTroll()
        {
            downloadFailed = true;

            // Everything stops
            ShowTrollMessage("99%... almost there...");

            yield return new WaitForSeconds(2f);

            // ERROR
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCriticalError();
            }

            ShowTrollMessage("DOWNLOAD FAILED: Connection interrupted. Restarting...");

            yield return new WaitForSeconds(3f);

            // Reset progress (teleport player back)
            if (playerTransform != null && progressBarStartPoint != null)
            {
                playerTransform.position = progressBarStartPoint.position;
            }

            currentProgress = 0f;
            downloadFailed = false;
            attemptCount++;

            // Activate harder obstacles
            ActivateObstacleWave(2);

            ShowTrollMessage("Download restarted from 0%. Oh, and there are more obstacles now. Enjoy!");
        }

        /// <summary>
        /// The download actually completes on the second try.
        /// </summary>
        private IEnumerator DownloadCompleteSequence()
        {
            if (levelComplete) yield break;

            ShowTrollMessage("100%! Update downloaded successfully!");

            yield return new WaitForSeconds(2f);

            ShowTrollMessage("Installing update... Please do not turn off your—\nAh forget it, level complete.");

            yield return new WaitForSeconds(2f);

            levelComplete = true;
            OnLevelComplete();
        }

        /// <summary>
        /// Activates obstacle waves based on attempt number.
        /// </summary>
        private void ActivateObstacleWave(int wave)
        {
            SetObstacleWaveActive(wave1Obstacles, wave >= 1);
            SetObstacleWaveActive(wave2Obstacles, wave >= 2);
            SetObstacleWaveActive(wave3Obstacles, wave >= 3);
        }

        private void SetObstacleWaveActive(GameObject[] obstacles, bool active)
        {
            if (obstacles == null) return;
            foreach (GameObject obj in obstacles)
            {
                if (obj != null) obj.SetActive(active);
            }
        }

        protected override void OnDeathTroll()
        {
            float pct = currentProgress * 100f;
            ShowTrollMessage($"You died at {pct:F0}% downloaded. The internet is laughing at you.");
        }
    }
}
