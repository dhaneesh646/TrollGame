using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TrollGame.Core;

namespace TrollGame.FourthWall
{
    /// <summary>
    /// Renders a fake Windows desktop inside the game window.
    /// When the game "minimizes," the player sees a desktop with fake icons,
    /// a taskbar, and interactive elements — but it's all rendered in Unity.
    /// 
    /// This approach (used by Pony Island, There Is No Game) is 100% safe
    /// and doesn't trigger any antivirus software.
    /// </summary>
    public class FakeDesktopController : MonoBehaviour
    {
        [Header("Desktop UI")]
        [SerializeField] private Canvas desktopCanvas;
        [SerializeField] private Image desktopWallpaper;
        [SerializeField] private RectTransform iconContainer;
        [SerializeField] private RectTransform taskbarPanel;
        [SerializeField] private Text clockText;
        [SerializeField] private Text startButtonText;
        [SerializeField] private Image startMenuPanel;

        [Header("Desktop Settings")]
        [SerializeField] private Color desktopColor = new Color(0f, 0.47f, 0.84f);
        [SerializeField] private Sprite[] desktopWallpapers;
        [SerializeField] private Sprite folderIcon;
        [SerializeField] private Sprite fileIcon;
        [SerializeField] private Sprite recyclebinIcon;
        [SerializeField] private Sprite trollGameIcon;

        [Header("Fake Desktop Icons")]
        [SerializeField] private List<FakeDesktopIcon> desktopIcons = new List<FakeDesktopIcon>();

        [Header("Troll Elements")]
        [SerializeField] private Text trollNotepadText;
        [SerializeField] private GameObject trollNotepadWindow;
        [SerializeField] private GameObject trollBrowserWindow;
        [SerializeField] private Text trollBrowserContent;

        private bool isDesktopActive = false;
        private bool startMenuOpen = false;

        /// <summary>
        /// Shows the fake desktop (game "minimizes" to this).
        /// </summary>
        public void ShowFakeDesktop()
        {
            isDesktopActive = true;

            if (desktopCanvas != null) desktopCanvas.gameObject.SetActive(true);

            // Set wallpaper
            if (desktopWallpaper != null && desktopWallpapers != null && desktopWallpapers.Length > 0)
            {
                desktopWallpaper.sprite = desktopWallpapers[UnityEngine.Random.Range(0, desktopWallpapers.Length)];
            }
            else if (desktopWallpaper != null)
            {
                desktopWallpaper.color = desktopColor;
            }

            PopulateDesktopIcons();
            UpdateClock();
            StartCoroutine(ClockUpdateRoutine());

            // Unlock cursor for desktop interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>
        /// Hides the fake desktop and returns to the game.
        /// </summary>
        public void HideFakeDesktop()
        {
            isDesktopActive = false;

            if (desktopCanvas != null) desktopCanvas.gameObject.SetActive(false);

            // Re-lock cursor for game
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>
        /// Populates the desktop with fake icons.
        /// </summary>
        private void PopulateDesktopIcons()
        {
            // Default icons if none configured
            if (desktopIcons.Count == 0)
            {
                desktopIcons.Add(new FakeDesktopIcon { Name = "totally_not_a_virus.exe", IconType = IconType.File });
                desktopIcons.Add(new FakeDesktopIcon { Name = "Recycle Bin", IconType = IconType.RecycleBin });
                desktopIcons.Add(new FakeDesktopIcon { Name = "My Documents", IconType = IconType.Folder });
                desktopIcons.Add(new FakeDesktopIcon { Name = "DONT_OPEN_THIS", IconType = IconType.Folder });
                desktopIcons.Add(new FakeDesktopIcon { Name = "secret_plans.txt", IconType = IconType.File });
                desktopIcons.Add(new FakeDesktopIcon { Name = "TrollGame.exe", IconType = IconType.Game });
                desktopIcons.Add(new FakeDesktopIcon { Name = "player_data_backup.zip", IconType = IconType.File });
                desktopIcons.Add(new FakeDesktopIcon { Name = "README_IMPORTANT.txt", IconType = IconType.File });
            }
        }

        /// <summary>
        /// Called when a desktop icon is clicked.
        /// Each icon has its own troll behavior.
        /// </summary>
        public void OnDesktopIconClicked(string iconName)
        {
            switch (iconName)
            {
                case "totally_not_a_virus.exe":
                    OnVirusExeClicked();
                    break;
                case "Recycle Bin":
                    OnRecycleBinClicked();
                    break;
                case "DONT_OPEN_THIS":
                    OnDontOpenThisClicked();
                    break;
                case "secret_plans.txt":
                    OnSecretPlansClicked();
                    break;
                case "TrollGame.exe":
                    OnTrollGameExeClicked();
                    break;
                case "README_IMPORTANT.txt":
                    OnReadmeClicked();
                    break;
                case "player_data_backup.zip":
                    OnBackupClicked();
                    break;
                default:
                    OnGenericIconClicked(iconName);
                    break;
            }
        }

        private void OnVirusExeClicked()
        {
            // Clicking the "virus" triggers a cascade of fake error popups
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFakeErrorCascade(
                    "CRITICAL ERROR",
                    "Your system has been compromised!",
                    5, 0.3f
                );
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCriticalError();
            }
        }

        private void OnRecycleBinClicked()
        {
            // Recycle bin contains "deleted" game files
            ShowNotepad("Recycle Bin Contents:\n\n" +
                "- player_dignity.exe (Deleted)\n" +
                "- skill.dll (Not Found)\n" +
                "- patience.sys (Corrupted)\n" +
                "- will_to_live.bat (Empty)\n" +
                "- good_reflexes.zip (0 bytes)\n\n" +
                "All items were deleted by: TrollGame.exe");
        }

        private void OnDontOpenThisClicked()
        {
            // Opens a folder that leads back to the game
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowTrollDialogue("The Game",
                    "I told you not to open that. Now you're back in the game.", 3f);
            }
            HideFakeDesktop();
        }

        private void OnSecretPlansClicked()
        {
            string playerName = "Player";
            if (GameManager.Instance != null)
            {
                playerName = GameManager.Instance.PlayerName;
            }

            ShowNotepad($"=== TOP SECRET GAME DESIGN DOCUMENT ===\n\n" +
                $"Target: {playerName}\n" +
                $"Objective: Maximum trolling\n\n" +
                $"Level 1: Gain trust\n" +
                $"Level 2: Betray trust\n" +
                $"Level 3: ???\n" +
                $"Level 4: Profit\n\n" +
                $"NOTE: If {playerName} finds this file,\n" +
                $"execute Plan B (more trolling).\n\n" +
                $"P.S. Yes, {playerName}, we know you're reading this.");
        }

        private void OnTrollGameExeClicked()
        {
            // "Re-launches" the game (actually just exits fake desktop)
            ShowFakeLoadingScreen();
        }

        private void OnReadmeClicked()
        {
            ShowNotepad("=== README ===\n\n" +
                "Thank you for purchasing TrollGame!\n\n" +
                "Controls:\n" +
                "- WASD: Maybe moves\n" +
                "- Space: Might jump\n" +
                "- Escape: LOL nice try\n" +
                "- Alt+F4: Definitely don't try this\n" +
                "- Ctrl+Z: Can't undo your purchase\n\n" +
                "Known Issues:\n" +
                "- Everything is working as intended\n" +
                "- Your suffering is a feature, not a bug\n\n" +
                "Refund Policy:\n" +
                "- HAHAHAHA");
        }

        private void OnBackupClicked()
        {
            // Fake extracting backup
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFakeLoading("Restoring backup...", 5f);
                UIManager.Instance.OnFakeLoadingComplete += () =>
                {
                    UIManager.Instance.ShowFakeErrorPopup("Backup Error",
                        "Error: Backup file is just a picture of a troll face.");
                };
            }
        }

        private void OnGenericIconClicked(string iconName)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFakeErrorPopup("Access Denied",
                    $"'{iconName}' cannot be opened because the game says no.");
            }
        }

        /// <summary>
        /// Shows a fake Notepad window with text content.
        /// </summary>
        private void ShowNotepad(string content)
        {
            if (trollNotepadWindow != null)
            {
                trollNotepadWindow.SetActive(true);
                if (trollNotepadText != null) trollNotepadText.text = content;
            }
        }

        /// <summary>
        /// Shows a fake loading screen before "re-launching" the game.
        /// </summary>
        private void ShowFakeLoadingScreen()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFakeLoading("Loading TrollGame.exe...", 5f);
                UIManager.Instance.OnFakeLoadingComplete += OnGameRelaunchComplete;
            }
        }

        private void OnGameRelaunchComplete()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.OnFakeLoadingComplete -= OnGameRelaunchComplete;
            }
            HideFakeDesktop();
        }

        /// <summary>
        /// Toggles the fake Start Menu.
        /// </summary>
        public void ToggleStartMenu()
        {
            startMenuOpen = !startMenuOpen;
            if (startMenuPanel != null)
            {
                startMenuPanel.gameObject.SetActive(startMenuOpen);
            }
        }

        /// <summary>
        /// Updates the fake taskbar clock.
        /// </summary>
        private void UpdateClock()
        {
            if (clockText != null)
            {
                clockText.text = DateTime.Now.ToString("h:mm tt\nM/d/yyyy");
            }
        }

        private IEnumerator ClockUpdateRoutine()
        {
            while (isDesktopActive)
            {
                UpdateClock();
                yield return new WaitForSeconds(30f);
            }
        }
    }

    [Serializable]
    public class FakeDesktopIcon
    {
        public string Name;
        public IconType IconType;
        public Sprite CustomIcon;
        public string TooltipText;
    }

    public enum IconType
    {
        Folder,
        File,
        Game,
        RecycleBin,
        Browser,
        Terminal,
        Custom
    }
}
