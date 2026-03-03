using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TrollGame.Core
{
    /// <summary>
    /// Manages all UI overlays including fake system dialogs, troll messages,
    /// fake BSOD screens, and HUD elements.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Panels")]
        [SerializeField] private GameObject trollDialoguePanel;
        [SerializeField] private GameObject fakeErrorPanel;
        [SerializeField] private GameObject fakeBSODPanel;
        [SerializeField] private GameObject fakeDesktopPanel;
        [SerializeField] private GameObject fakeLoadingPanel;
        [SerializeField] private GameObject fakeUpdatePanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject deathScreenPanel;
        [SerializeField] private GameObject pauseMenuPanel;

        [Header("Troll Dialogue")]
        [SerializeField] private Text trollDialogueText;
        [SerializeField] private Text trollDialogueSpeaker;

        [Header("Fake Error")]
        [SerializeField] private Text fakeErrorTitle;
        [SerializeField] private Text fakeErrorMessage;
        [SerializeField] private Button fakeErrorOkButton;
        [SerializeField] private Button fakeErrorCancelButton;

        [Header("Fake BSOD")]
        [SerializeField] private Text bsodErrorCode;
        [SerializeField] private Text bsodPercentage;

        [Header("Fake Loading")]
        [SerializeField] private Slider fakeLoadingBar;
        [SerializeField] private Text fakeLoadingText;

        [Header("Death Screen")]
        [SerializeField] private Text deathMessageText;
        [SerializeField] private Text deathCountText;

        [Header("Screen Effects")]
        [SerializeField] private Image screenFlashImage;
        [SerializeField] private Image screenFadeImage;
        [SerializeField] private Image glitchOverlayImage;

        // Events
        public event Action OnFakeErrorDismissed;
        public event Action OnFakeBSODComplete;
        public event Action OnFakeLoadingComplete;

        private Queue<DialogueEntry> dialogueQueue = new Queue<DialogueEntry>();
        private bool isShowingDialogue = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            HideAllPanels();
        }

        /// <summary>
        /// Hides all overlay panels.
        /// </summary>
        public void HideAllPanels()
        {
            SetPanelActive(trollDialoguePanel, false);
            SetPanelActive(fakeErrorPanel, false);
            SetPanelActive(fakeBSODPanel, false);
            SetPanelActive(fakeDesktopPanel, false);
            SetPanelActive(fakeLoadingPanel, false);
            SetPanelActive(fakeUpdatePanel, false);
            SetPanelActive(deathScreenPanel, false);
            SetPanelActive(pauseMenuPanel, false);
        }

        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null) panel.SetActive(active);
        }

        // ==========================================
        // TROLL DIALOGUE SYSTEM
        // ==========================================

        /// <summary>
        /// Shows a troll dialogue message with typewriter effect.
        /// </summary>
        public void ShowTrollDialogue(string message, float duration = 3f)
        {
            ShowTrollDialogue("???", message, duration);
        }

        /// <summary>
        /// Shows a troll dialogue with a named speaker.
        /// </summary>
        public void ShowTrollDialogue(string speaker, string message, float duration = 3f)
        {
            DialogueEntry entry = new DialogueEntry
            {
                Speaker = speaker,
                Message = message,
                Duration = duration
            };

            dialogueQueue.Enqueue(entry);

            if (!isShowingDialogue)
            {
                StartCoroutine(ProcessDialogueQueue());
            }
        }

        private IEnumerator ProcessDialogueQueue()
        {
            isShowingDialogue = true;

            while (dialogueQueue.Count > 0)
            {
                DialogueEntry entry = dialogueQueue.Dequeue();

                if (trollDialogueSpeaker != null) trollDialogueSpeaker.text = entry.Speaker;
                if (trollDialogueText != null) trollDialogueText.text = "";

                SetPanelActive(trollDialoguePanel, true);

                // Typewriter effect
                if (trollDialogueText != null)
                {
                    foreach (char c in entry.Message)
                    {
                        trollDialogueText.text += c;
                        yield return new WaitForSeconds(0.03f);
                    }
                }

                yield return new WaitForSeconds(entry.Duration);
                SetPanelActive(trollDialoguePanel, false);

                yield return new WaitForSeconds(0.5f);
            }

            isShowingDialogue = false;
        }

        // ==========================================
        // FAKE ERROR POPUP
        // ==========================================

        /// <summary>
        /// Shows a Windows-style error popup (rendered in-game, not a real system dialog).
        /// </summary>
        public void ShowFakeErrorPopup(string title, string message)
        {
            if (fakeErrorTitle != null) fakeErrorTitle.text = title;
            if (fakeErrorMessage != null) fakeErrorMessage.text = message;

            SetPanelActive(fakeErrorPanel, true);

            // Setup button callbacks
            if (fakeErrorOkButton != null)
            {
                fakeErrorOkButton.onClick.RemoveAllListeners();
                fakeErrorOkButton.onClick.AddListener(OnFakeErrorOkClicked);
            }
            if (fakeErrorCancelButton != null)
            {
                fakeErrorCancelButton.onClick.RemoveAllListeners();
                fakeErrorCancelButton.onClick.AddListener(OnFakeErrorCancelClicked);
            }
        }

        /// <summary>
        /// Shows a cascade of fake error popups (the classic troll move).
        /// </summary>
        public void ShowFakeErrorCascade(string title, string message, int count, float interval = 0.15f)
        {
            StartCoroutine(ErrorCascadeRoutine(title, message, count, interval));
        }

        private IEnumerator ErrorCascadeRoutine(string title, string message, int count, float interval)
        {
            for (int i = 0; i < count; i++)
            {
                ShowFakeErrorPopup(title, message + $" (Error #{i + 1})");
                yield return new WaitForSeconds(interval);
            }
        }

        private void OnFakeErrorOkClicked()
        {
            // OK button spawns MORE errors (classic troll)
            SetPanelActive(fakeErrorPanel, false);
            OnFakeErrorDismissed?.Invoke();
        }

        private void OnFakeErrorCancelClicked()
        {
            // Cancel actually closes it (reward for non-obvious choice)
            SetPanelActive(fakeErrorPanel, false);
            OnFakeErrorDismissed?.Invoke();
        }

        // ==========================================
        // FAKE BSOD (Blue Screen of Death)
        // ==========================================

        /// <summary>
        /// Shows a fake Blue Screen of Death. Looks terrifyingly real.
        /// </summary>
        public void ShowFakeBSOD(float duration = 8f, string errorCode = "TROLL_GAME_EXCEPTION")
        {
            SetPanelActive(fakeBSODPanel, true);
            if (bsodErrorCode != null) bsodErrorCode.text = errorCode;

            StartCoroutine(FakeBSODRoutine(duration));
        }

        private IEnumerator FakeBSODRoutine(float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                int percentage = Mathf.FloorToInt((elapsed / duration) * 100f);
                percentage = Mathf.Min(percentage, 99); // Never reaches 100% (extra troll)

                if (bsodPercentage != null)
                {
                    bsodPercentage.text = $"{percentage}% complete";
                }

                yield return null;
            }

            // "Reboot"
            StartCoroutine(FakeRebootSequence());
        }

        private IEnumerator FakeRebootSequence()
        {
            // Brief black screen
            SetPanelActive(fakeBSODPanel, false);

            if (screenFadeImage != null)
            {
                screenFadeImage.color = Color.black;
                screenFadeImage.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(2f);

            if (screenFadeImage != null)
            {
                screenFadeImage.gameObject.SetActive(false);
            }

            OnFakeBSODComplete?.Invoke();
        }

        // ==========================================
        // FAKE LOADING / UPDATE SCREEN
        // ==========================================

        /// <summary>
        /// Shows a fake loading screen that trolls the player.
        /// The loading bar behaves erratically to maximize frustration.
        /// </summary>
        public void ShowFakeLoading(string loadingText, float duration = 10f)
        {
            SetPanelActive(fakeLoadingPanel, true);
            if (fakeLoadingText != null) fakeLoadingText.text = loadingText;
            StartCoroutine(FakeLoadingRoutine(duration));
        }

        private IEnumerator FakeLoadingRoutine(float duration)
        {
            float elapsed = 0f;
            float displayProgress = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float realProgress = elapsed / duration;

                // Erratic loading bar behavior
                if (realProgress < 0.3f)
                {
                    // Fast start to give false hope
                    displayProgress = realProgress * 2f;
                }
                else if (realProgress < 0.7f)
                {
                    // Painfully slow middle section
                    displayProgress = 0.6f + (realProgress - 0.3f) * 0.1f;
                }
                else if (realProgress < 0.95f)
                {
                    // Speed up again
                    displayProgress = 0.64f + (realProgress - 0.7f) * 1.2f;
                }
                else
                {
                    // Stuck at 99% for maximum agony
                    displayProgress = 0.99f;
                }

                // Random jumps backward (maximum troll)
                if (UnityEngine.Random.value < 0.01f && displayProgress > 0.5f)
                {
                    displayProgress -= UnityEngine.Random.Range(0.1f, 0.3f);
                    if (fakeLoadingText != null)
                    {
                        fakeLoadingText.text = "Oops, re-downloading assets...";
                    }
                }

                if (fakeLoadingBar != null)
                {
                    fakeLoadingBar.value = Mathf.Clamp01(displayProgress);
                }

                yield return null;
            }

            SetPanelActive(fakeLoadingPanel, false);
            OnFakeLoadingComplete?.Invoke();
        }

        // ==========================================
        // SCREEN EFFECTS
        // ==========================================

        /// <summary>
        /// Flashes the screen a specific color (for jump scares, explosions, etc).
        /// </summary>
        public void FlashScreen(Color color, float duration = 0.1f)
        {
            StartCoroutine(ScreenFlashRoutine(color, duration));
        }

        private IEnumerator ScreenFlashRoutine(Color color, float duration)
        {
            if (screenFlashImage == null) yield break;

            screenFlashImage.color = color;
            screenFlashImage.gameObject.SetActive(true);

            yield return new WaitForSeconds(duration);

            // Fade out
            float fadeTime = duration * 2f;
            float elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                screenFlashImage.color = new Color(color.r, color.g, color.b, alpha);
                yield return null;
            }

            screenFlashImage.gameObject.SetActive(false);
        }

        /// <summary>
        /// Fades screen to black and back.
        /// </summary>
        public void FadeToBlack(float fadeOutTime = 1f, float holdTime = 1f, float fadeInTime = 1f)
        {
            StartCoroutine(FadeRoutine(fadeOutTime, holdTime, fadeInTime));
        }

        private IEnumerator FadeRoutine(float fadeOutTime, float holdTime, float fadeInTime)
        {
            if (screenFadeImage == null) yield break;

            screenFadeImage.gameObject.SetActive(true);

            // Fade out
            float elapsed = 0f;
            while (elapsed < fadeOutTime)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeOutTime);
                screenFadeImage.color = new Color(0, 0, 0, alpha);
                yield return null;
            }

            yield return new WaitForSeconds(holdTime);

            // Fade in
            elapsed = 0f;
            while (elapsed < fadeInTime)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeInTime);
                screenFadeImage.color = new Color(0, 0, 0, alpha);
                yield return null;
            }

            screenFadeImage.gameObject.SetActive(false);
        }

        // ==========================================
        // DEATH SCREEN
        // ==========================================

        /// <summary>
        /// Shows the death screen with a troll message.
        /// </summary>
        public void ShowDeathScreen(string message)
        {
            SetPanelActive(deathScreenPanel, true);

            if (deathMessageText != null) deathMessageText.text = message;

            if (deathCountText != null && GameManager.Instance != null)
            {
                deathCountText.text = $"Deaths: {GameManager.Instance.PlayerDeathCount}";
            }
        }

        /// <summary>
        /// Hides the death screen.
        /// </summary>
        public void HideDeathScreen()
        {
            SetPanelActive(deathScreenPanel, false);
        }

        /// <summary>
        /// Shows glitch overlay effect.
        /// </summary>
        public void ShowGlitchEffect(float duration = 0.5f)
        {
            StartCoroutine(GlitchRoutine(duration));
        }

        private IEnumerator GlitchRoutine(float duration)
        {
            if (glitchOverlayImage == null) yield break;

            glitchOverlayImage.gameObject.SetActive(true);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = UnityEngine.Random.Range(0.3f, 0.8f);
                glitchOverlayImage.color = new Color(
                    UnityEngine.Random.value,
                    UnityEngine.Random.value,
                    UnityEngine.Random.value,
                    alpha
                );
                yield return new WaitForSeconds(0.05f);
            }

            glitchOverlayImage.gameObject.SetActive(false);
        }

        private struct DialogueEntry
        {
            public string Speaker;
            public string Message;
            public float Duration;
        }
    }
}
