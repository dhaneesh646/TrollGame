using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TrollGame.Core
{
    /// <summary>
    /// Central game manager - handles level progression, game state, and global troll coordination.
    /// Singleton pattern ensures only one instance exists across scenes.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Level Configuration")]
        [SerializeField] private List<LevelData> levels = new List<LevelData>();
        [SerializeField] private int currentLevelIndex = 0;

        [Header("Troll Settings")]
        [SerializeField] private float globalTrollIntensity = 1.0f;
        [SerializeField] private bool streamerModeActive = false;
        [SerializeField] private int playerRageLevel = 0;
        [SerializeField] private int playerDeathCount = 0;

        [Header("Meta Tracking")]
        [SerializeField] private float totalPlayTime = 0f;
        [SerializeField] private int totalClicks = 0;
        [SerializeField] private int timesAltTabbed = 0;

        // Events for other systems to react to
        public event Action<int> OnLevelChanged;
        public event Action<int> OnRageLevelChanged;
        public event Action<int> OnPlayerDied;
        public event Action OnStreamerModeToggled;
        public event Action<string> OnTrollTriggered;

        // Player awareness tracking
        public bool PlayerKnowsGameIsTrolling { get; set; } = false;
        public bool PlayerHasTriedToQuit { get; set; } = false;
        public bool PlayerHasAltTabbed { get; set; } = false;
        public string PlayerName { get; private set; } = "Player";

        public int CurrentLevelIndex => currentLevelIndex;
        public int PlayerDeathCount => playerDeathCount;
        public int PlayerRageLevel => playerRageLevel;
        public float GlobalTrollIntensity => globalTrollIntensity;
        public bool IsStreamerMode => streamerModeActive;
        public float TotalPlayTime => totalPlayTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadPlayerName();
            LoadMetaData();
        }

        private void Update()
        {
            totalPlayTime += Time.deltaTime;

            // Track mouse clicks for rage detection
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                totalClicks++;
                DetectRageClicking();
            }

            // Detect alt-tab behavior
            if (!Application.isFocused && !PlayerHasAltTabbed)
            {
                PlayerHasAltTabbed = true;
                timesAltTabbed++;
                OnTrollTriggered?.Invoke("player_alt_tabbed");
            }

            // Detect escape key (player trying to quit)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PlayerHasTriedToQuit = true;
                OnTrollTriggered?.Invoke("player_tried_to_quit");
            }
        }

        /// <summary>
        /// Attempts to load the player's name from system environment or Steam.
        /// Falls back to OS username for that personal troll touch.
        /// </summary>
        private void LoadPlayerName()
        {
            // Try to get the OS username for 4th-wall breaking
            string systemUser = Environment.UserName;
            if (!string.IsNullOrEmpty(systemUser))
            {
                PlayerName = systemUser;
            }
        }

        /// <summary>
        /// Loads persistent meta-data that survives between play sessions.
        /// The game "remembers" the player even after reinstall (via PlayerPrefs).
        /// </summary>
        private void LoadMetaData()
        {
            playerDeathCount = PlayerPrefs.GetInt("TrollGame_TotalDeaths", 0);
            totalPlayTime = PlayerPrefs.GetFloat("TrollGame_TotalPlayTime", 0f);
            timesAltTabbed = PlayerPrefs.GetInt("TrollGame_TimesAltTabbed", 0);
            int previousLaunchCount = PlayerPrefs.GetInt("TrollGame_LaunchCount", 0);
            PlayerPrefs.SetInt("TrollGame_LaunchCount", previousLaunchCount + 1);

            // If the player has launched before, the game "remembers"
            if (previousLaunchCount > 0)
            {
                PlayerKnowsGameIsTrolling = true;
            }

            PlayerPrefs.Save();
        }

        /// <summary>
        /// Saves persistent meta-data.
        /// </summary>
        public void SaveMetaData()
        {
            PlayerPrefs.SetInt("TrollGame_TotalDeaths", playerDeathCount);
            PlayerPrefs.SetFloat("TrollGame_TotalPlayTime", totalPlayTime);
            PlayerPrefs.SetInt("TrollGame_TimesAltTabbed", timesAltTabbed);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Advances to the next level with optional troll transition.
        /// </summary>
        public void NextLevel()
        {
            currentLevelIndex++;

            if (currentLevelIndex >= levels.Count)
            {
                // Game "complete" - but is it really?
                OnTrollTriggered?.Invoke("fake_ending");
                return;
            }

            OnLevelChanged?.Invoke(currentLevelIndex);
            LoadLevel(currentLevelIndex);
        }

        /// <summary>
        /// Loads a specific level by index.
        /// </summary>
        public void LoadLevel(int index)
        {
            if (index < 0 || index >= levels.Count) return;

            currentLevelIndex = index;
            string sceneName = levels[index].SceneName;

            if (!string.IsNullOrEmpty(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }

            OnLevelChanged?.Invoke(currentLevelIndex);
        }

        /// <summary>
        /// Called when the player dies. Tracks deaths and adjusts troll intensity.
        /// </summary>
        public void RegisterPlayerDeath()
        {
            playerDeathCount++;
            OnPlayerDied?.Invoke(playerDeathCount);

            // Increase troll intensity based on death count
            if (playerDeathCount > 20)
            {
                globalTrollIntensity = 2.0f; // Maximum trolling
            }
            else if (playerDeathCount > 10)
            {
                globalTrollIntensity = 1.5f;
            }

            // Every 5 deaths, increase rage level
            if (playerDeathCount % 5 == 0)
            {
                IncreaseRageLevel();
            }

            SaveMetaData();
        }

        /// <summary>
        /// Increases the player's rage level, which unlocks more aggressive trolling.
        /// </summary>
        public void IncreaseRageLevel()
        {
            playerRageLevel = Mathf.Min(playerRageLevel + 1, 10);
            OnRageLevelChanged?.Invoke(playerRageLevel);
        }

        /// <summary>
        /// Toggles streamer mode - increases troll frequency and adds camera-aware behavior.
        /// </summary>
        public void ToggleStreamerMode()
        {
            streamerModeActive = !streamerModeActive;

            if (streamerModeActive)
            {
                globalTrollIntensity *= 1.5f;
            }
            else
            {
                globalTrollIntensity /= 1.5f;
            }

            OnStreamerModeToggled?.Invoke();
        }

        /// <summary>
        /// Detects rapid clicking as a sign of rage/frustration.
        /// </summary>
        private float lastClickTime = 0f;
        private int rapidClickCount = 0;

        private void DetectRageClicking()
        {
            float timeSinceLastClick = Time.time - lastClickTime;

            if (timeSinceLastClick < 0.3f)
            {
                rapidClickCount++;
                if (rapidClickCount > 5)
                {
                    IncreaseRageLevel();
                    OnTrollTriggered?.Invoke("rage_clicking_detected");
                    rapidClickCount = 0;
                }
            }
            else
            {
                rapidClickCount = 0;
            }

            lastClickTime = Time.time;
        }

        /// <summary>
        /// Triggers a named troll event that other systems can react to.
        /// </summary>
        public void TriggerTrollEvent(string eventName)
        {
            OnTrollTriggered?.Invoke(eventName);
        }

        /// <summary>
        /// Resets all "fake" progress (used for the fake-reset troll).
        /// </summary>
        public void FakeReset()
        {
            // Pretend to reset but actually don't
            OnTrollTriggered?.Invoke("fake_reset");
        }

        /// <summary>
        /// Actually resets all persistent data (hidden option).
        /// </summary>
        public void ActualReset()
        {
            PlayerPrefs.DeleteAll();
            playerDeathCount = 0;
            playerRageLevel = 0;
            totalPlayTime = 0f;
            totalClicks = 0;
            timesAltTabbed = 0;
            currentLevelIndex = 0;
            PlayerKnowsGameIsTrolling = false;
            PlayerHasTriedToQuit = false;
        }

        private void OnApplicationQuit()
        {
            SaveMetaData();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveMetaData();
            }
        }
    }

    /// <summary>
    /// Data container for level configuration.
    /// </summary>
    [Serializable]
    public class LevelData
    {
        public string LevelName;
        public string SceneName;
        public string Description;
        public float DifficultyMultiplier = 1.0f;
        public bool HasFourthWallBreak = false;
        public TrollType PrimaryTrollType = TrollType.None;
    }

    /// <summary>
    /// Types of trolling mechanics available in the game.
    /// </summary>
    public enum TrollType
    {
        None,
        FakeCrash,
        FakeBSOD,
        FakeDesktop,
        ControlSwap,
        GravityFlip,
        FakeProgress,
        FakeEnding,
        WindowManipulation,
        SaveCorruption,
        FakeUpdate,
        StreamerAware,
        MemeReference,
        RagePlatformer
    }
}
