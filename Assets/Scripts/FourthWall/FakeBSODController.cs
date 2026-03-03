using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TrollGame.Core;

namespace TrollGame.FourthWall
{
    /// <summary>
    /// Controls the fake Blue Screen of Death effect.
    /// Renders a pixel-perfect Windows BSOD inside the game window.
    /// This is 100% in-engine UI — no actual system calls, zero AV risk.
    /// </summary>
    public class FakeBSODController : MonoBehaviour
    {
        [Header("BSOD UI Elements")]
        [SerializeField] private Canvas bsodCanvas;
        [SerializeField] private Image bsodBackground;
        [SerializeField] private Text sadFaceText;
        [SerializeField] private Text mainErrorText;
        [SerializeField] private Text percentageText;
        [SerializeField] private Text errorCodeText;
        [SerializeField] private Text qrCodePlaceholder;

        [Header("BSOD Settings")]
        [SerializeField] private Color bsodBlueColor = new Color(0f, 0.47f, 0.84f, 1f);
        [SerializeField] private float collectDuration = 8f;
        [SerializeField] private float restartDelay = 3f;
        [SerializeField] private bool pauseGameDuringBSOD = true;

        [Header("Troll Error Messages")]
        [SerializeField] private string[] trollErrorCodes = new string[]
        {
            "TROLL_GAME_EXCEPTION",
            "SKILL_ISSUE_DETECTED",
            "GIT_GUD_FAILURE",
            "RAGE_QUIT_OVERFLOW",
            "MEME_BUFFER_OVERFLOW",
            "PLAYER_PATIENCE_UNDERFLOW",
            "KEYBOARD_SMASH_DETECTED",
            "BRAINROT_CORRUPTION",
            "NOOB_STATUS_CONFIRMED",
            "CTRL_ALT_DELETE_WONT_HELP"
        };

        private bool isBSODActive = false;

        /// <summary>
        /// Triggers the fake BSOD with a random troll error code.
        /// </summary>
        public void TriggerBSOD()
        {
            string errorCode = trollErrorCodes[Random.Range(0, trollErrorCodes.Length)];
            TriggerBSOD(errorCode);
        }

        /// <summary>
        /// Triggers the fake BSOD with a specific error code.
        /// </summary>
        public void TriggerBSOD(string errorCode)
        {
            if (isBSODActive) return;

            isBSODActive = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopMusic(0.1f);
                AudioManager.Instance.PlayBSODSound();
            }

            StartCoroutine(BSODSequence(errorCode));
        }

        private IEnumerator BSODSequence(string errorCode)
        {
            // Pause game
            if (pauseGameDuringBSOD)
            {
                Time.timeScale = 0f;
            }

            // Setup BSOD visuals
            if (bsodCanvas != null) bsodCanvas.gameObject.SetActive(true);
            if (bsodBackground != null) bsodBackground.color = bsodBlueColor;
            if (sadFaceText != null) sadFaceText.text = ":(";
            if (mainErrorText != null)
            {
                mainErrorText.text = "Your PC ran into a problem and needs to restart. " +
                    "We're just collecting some error info, and then we'll restart for you.";
            }
            if (errorCodeText != null) errorCodeText.text = $"Stop code: {errorCode}";
            if (qrCodePlaceholder != null) qrCodePlaceholder.text = "[QR Code]\nFor more info visit:\nhttps://www.youjustgottrolled.com";

            // Fake collection progress
            float elapsed = 0f;
            int lastPercentage = -1;

            while (elapsed < collectDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                int percentage = Mathf.FloorToInt((elapsed / collectDuration) * 100f);

                // Troll: percentage gets stuck at certain numbers
                if (percentage >= 37 && percentage <= 42)
                {
                    percentage = 37; // Stuck!
                }
                else if (percentage >= 78 && percentage <= 85)
                {
                    percentage = 78; // Stuck again!
                }
                else if (percentage >= 99)
                {
                    percentage = 99; // NEVER reaches 100
                }

                if (percentage != lastPercentage)
                {
                    if (percentageText != null)
                    {
                        percentageText.text = $"{percentage}% complete";
                    }
                    lastPercentage = percentage;
                }

                yield return null;
            }

            // Fake restart sequence
            yield return StartCoroutine(FakeRestartSequence());

            // Cleanup
            isBSODActive = false;
            if (pauseGameDuringBSOD)
            {
                Time.timeScale = 1f;
            }
        }

        private IEnumerator FakeRestartSequence()
        {
            // Black screen
            if (bsodBackground != null)
            {
                bsodBackground.color = Color.black;
            }
            if (sadFaceText != null) sadFaceText.text = "";
            if (mainErrorText != null) mainErrorText.text = "";
            if (percentageText != null) percentageText.text = "";
            if (errorCodeText != null) errorCodeText.text = "";
            if (qrCodePlaceholder != null) qrCodePlaceholder.text = "";

            yield return new WaitForSecondsRealtime(1.5f);

            // Fake spinning dots "restarting" animation
            if (mainErrorText != null)
            {
                mainErrorText.text = "Restarting";
                for (int i = 0; i < 6; i++)
                {
                    mainErrorText.text += ".";
                    yield return new WaitForSecondsRealtime(0.5f);
                }
            }

            yield return new WaitForSecondsRealtime(restartDelay);

            // Fake boot screen
            if (mainErrorText != null)
            {
                mainErrorText.text = "Just kidding! Did we scare you? :)";
            }

            yield return new WaitForSecondsRealtime(2f);

            if (bsodCanvas != null) bsodCanvas.gameObject.SetActive(false);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayStartupSound();
            }
        }

        /// <summary>
        /// Creates a quick BSOD flash (brief flash of blue screen for unsettling effect).
        /// </summary>
        public void FlashBSOD(float duration = 0.15f)
        {
            StartCoroutine(BSODFlashRoutine(duration));
        }

        private IEnumerator BSODFlashRoutine(float duration)
        {
            if (bsodCanvas != null) bsodCanvas.gameObject.SetActive(true);
            if (bsodBackground != null) bsodBackground.color = bsodBlueColor;
            if (sadFaceText != null) sadFaceText.text = ":(";
            if (mainErrorText != null) mainErrorText.text = "";

            yield return new WaitForSecondsRealtime(duration);

            if (bsodCanvas != null) bsodCanvas.gameObject.SetActive(false);
        }
    }
}
