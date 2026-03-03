using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TrollGame.Core;

namespace TrollGame.FourthWall
{
    /// <summary>
    /// Controls fake Windows-style error popup dialogs.
    /// All rendered as in-game UI — looks authentic but is 100% safe.
    /// 
    /// Features:
    /// - Multiple error styles (Warning, Error, Critical, Info)
    /// - Draggable popups
    /// - Cascade spawning (the classic "infinite error" troll)
    /// - Button trolling (OK spawns more, Cancel doesn't work, etc.)
    /// </summary>
    public class FakeErrorPopupController : MonoBehaviour
    {
        [Header("Popup Prefab")]
        [SerializeField] private GameObject errorPopupPrefab;
        [SerializeField] private Canvas popupCanvas;
        [SerializeField] private int maxPopups = 30;

        [Header("Error Styles")]
        [SerializeField] private Sprite warningIcon;
        [SerializeField] private Sprite errorIcon;
        [SerializeField] private Sprite criticalIcon;
        [SerializeField] private Sprite infoIcon;

        [Header("Troll Error Messages")]
        [SerializeField] private string[] trollTitles = new string[]
        {
            "Fatal Error",
            "Warning",
            "System Alert",
            "TrollGame.exe",
            "CRITICAL FAILURE",
            "Skill Issue Detected",
            "Windows Security",
            "Your PC",
            "Error 404",
            "Blue Shell Incoming"
        };

        [SerializeField] private string[] trollMessages = new string[]
        {
            "An unexpected error occurred: You are bad at this game.",
            "Warning: Your skill level is dangerously low.",
            "System resources depleted by excessive rage clicking.",
            "Cannot find file: 'your_patience.dll'",
            "Error: Task 'Git Gud' failed with exit code -1.",
            "The operation completed unsuccessfully. Blame yourself.",
            "This program has performed an illegal operation: Being too easy on you.",
            "Out of memory. Player brain buffer exceeded.",
            "Runtime Error: divide by skill (skill = 0)",
            "Unhandled Exception: PlayerRageOverflowException",
            "Access Denied: You don't have permission to win.",
            "The file 'victory.exe' could not be found on your system.",
            "Error 418: I'm a teapot. And you're bad at games.",
            "Stack Overflow: Too many excuses piled up.",
            "Connection timed out: Server refusing your skill level."
        };

        private List<GameObject> activePopups = new List<GameObject>();

        /// <summary>
        /// Spawns a single fake error popup at a random position.
        /// </summary>
        public void SpawnError()
        {
            string title = trollTitles[Random.Range(0, trollTitles.Length)];
            string message = trollMessages[Random.Range(0, trollMessages.Length)];
            SpawnError(title, message, ErrorStyle.Warning);
        }

        /// <summary>
        /// Spawns a fake error popup with specific content.
        /// </summary>
        public void SpawnError(string title, string message, ErrorStyle style)
        {
            if (activePopups.Count >= maxPopups) return;
            if (errorPopupPrefab == null || popupCanvas == null) return;

            GameObject popup = Instantiate(errorPopupPrefab, popupCanvas.transform);

            // Random position within screen bounds
            RectTransform rt = popup.GetComponent<RectTransform>();
            if (rt != null)
            {
                float x = Random.Range(-300f, 300f);
                float y = Random.Range(-200f, 200f);
                rt.anchoredPosition = new Vector2(x, y);
            }

            // Setup popup content
            FakeErrorPopup popupComponent = popup.GetComponent<FakeErrorPopup>();
            if (popupComponent != null)
            {
                Sprite icon = GetIconForStyle(style);
                popupComponent.Setup(title, message, icon, this);
            }

            activePopups.Add(popup);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayWindowsError();
            }
        }

        /// <summary>
        /// Spawns a cascade of error popups (the classic troll move).
        /// Each one slightly offset from the last.
        /// </summary>
        public void SpawnErrorCascade(int count, float interval = 0.1f)
        {
            StartCoroutine(ErrorCascadeRoutine(count, interval));
        }

        private IEnumerator ErrorCascadeRoutine(int count, float interval)
        {
            for (int i = 0; i < count; i++)
            {
                if (activePopups.Count >= maxPopups) break;

                SpawnError();
                yield return new WaitForSeconds(interval);
            }
        }

        /// <summary>
        /// Spawns errors that multiply when closed (the ultimate troll).
        /// Close one, two more appear!
        /// </summary>
        public void SpawnHydraErrors(int initialCount = 3)
        {
            for (int i = 0; i < initialCount; i++)
            {
                string title = "Hydra Error";
                string message = "Close me. I dare you. (Two more will appear)";
                SpawnError(title, message, ErrorStyle.Critical);
            }
        }

        /// <summary>
        /// Called when a popup is closed. Used for hydra-mode (close one, spawn two).
        /// </summary>
        public void OnPopupClosed(GameObject popup, bool isHydraMode)
        {
            activePopups.Remove(popup);
            Destroy(popup);

            if (isHydraMode && activePopups.Count < maxPopups)
            {
                // Spawn two more (hydra!)
                SpawnError("Hydra Error", "I told you! Here's two more!", ErrorStyle.Critical);
                SpawnError("Hydra Error", "You can't stop us!", ErrorStyle.Critical);
            }
        }

        /// <summary>
        /// Clears all active popups.
        /// </summary>
        public void ClearAllPopups()
        {
            foreach (GameObject popup in activePopups)
            {
                if (popup != null) Destroy(popup);
            }
            activePopups.Clear();
        }

        /// <summary>
        /// Spawns a popup where the OK button runs away from the cursor.
        /// </summary>
        public void SpawnUnclosableError(string title, string message)
        {
            SpawnError(title, message, ErrorStyle.Error);
            // The FakeErrorPopup component handles the runaway button behavior
        }

        private Sprite GetIconForStyle(ErrorStyle style)
        {
            switch (style)
            {
                case ErrorStyle.Warning: return warningIcon;
                case ErrorStyle.Error: return errorIcon;
                case ErrorStyle.Critical: return criticalIcon;
                case ErrorStyle.Info: return infoIcon;
                default: return warningIcon;
            }
        }
    }

    /// <summary>
    /// Individual fake error popup behavior.
    /// </summary>
    public class FakeErrorPopup : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text messageText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button okButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button closeButton;

        private FakeErrorPopupController controller;
        private bool isHydraMode = false;
        private bool isRunawayMode = false;

        public void Setup(string title, string message, Sprite icon, FakeErrorPopupController ctrl)
        {
            controller = ctrl;
            if (titleText != null) titleText.text = title;
            if (messageText != null) messageText.text = message;
            if (iconImage != null && icon != null) iconImage.sprite = icon;

            isHydraMode = title.Contains("Hydra");

            // Wire up buttons
            if (okButton != null)
            {
                okButton.onClick.AddListener(OnOkClicked);
            }
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelClicked);
            }
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }
        }

        private void OnOkClicked()
        {
            // OK button might spawn more errors
            if (controller != null)
            {
                controller.OnPopupClosed(gameObject, isHydraMode);
            }
        }

        private void OnCancelClicked()
        {
            // Cancel actually works (reward for the non-obvious choice)
            if (controller != null)
            {
                controller.OnPopupClosed(gameObject, false);
            }
        }

        private void OnCloseClicked()
        {
            if (controller != null)
            {
                controller.OnPopupClosed(gameObject, isHydraMode);
            }
        }
    }

    public enum ErrorStyle
    {
        Warning,
        Error,
        Critical,
        Info
    }
}
