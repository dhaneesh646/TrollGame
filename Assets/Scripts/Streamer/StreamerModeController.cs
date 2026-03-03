using System.Collections;
using UnityEngine;
using TrollGame.Core;

namespace TrollGame.Streamer
{
    /// <summary>
    /// Global streamer mode controller that enhances troll frequency and
    /// adds camera-aware behaviors when streaming is detected or toggled.
    /// 
    /// Can be activated through:
    /// 1. Pause menu toggle ("Streamer Mode" button)
    /// 2. Auto-detection of streaming software (optional, safe check)
    /// 3. GameManager command
    /// 
    /// Effects when active:
    /// - Troll frequency increased by 50%
    /// - More dramatic reactions from NPCs/enemies
    /// - Fake "viewer count" overlay
    /// - Occasional "chat messages" in the corner
    /// - Boss fights become more theatrical
    /// </summary>
    public class StreamerModeController : MonoBehaviour
    {
        public static StreamerModeController Instance { get; private set; }

        [Header("Streamer Mode UI")]
        [SerializeField] private GameObject streamerOverlay;
        [SerializeField] private UnityEngine.UI.Text viewerCountText;
        [SerializeField] private UnityEngine.UI.Text chatText;
        [SerializeField] private GameObject liveIndicator;

        [Header("Settings")]
        [SerializeField] private bool isActive = false;
        [SerializeField] private float trollFrequencyMultiplier = 1.5f;
        [SerializeField] private float chatMessageInterval = 8f;

        private int fakeViewerCount = 0;
        private Coroutine chatCoroutine;

        private string[] genericChatMessages = new string[]
        {
            "Viewer_42: is this game trolling you?",
            "xXProGamerXx: skill issue",
            "LurkerLarry: ...",
            "ModBot: !timeout Player 600s",
            "StreamFan: KEKW KEKW KEKW",
            "BackseatAndy: go right GO RIGHT",
            "DonorDave: donated to see you suffer",
            "NewViewer: what game is this??",
            "ClipChimp: CLIP IT CLIP IT",
            "SubAlert: Someone gifted 5 subs!",
            "Chatter99: OMEGALUL",
            "TrollBot: the game is self aware btw",
            "VIPViewer: I've been here for 3 hours",
            "FirstTimer: this is the best stream"
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (streamerOverlay != null) streamerOverlay.SetActive(false);
        }

        /// <summary>
        /// Toggles streamer mode on/off.
        /// </summary>
        public void ToggleStreamerMode()
        {
            isActive = !isActive;

            if (isActive)
            {
                EnableStreamerMode();
            }
            else
            {
                DisableStreamerMode();
            }
        }

        /// <summary>
        /// Enables streamer mode.
        /// </summary>
        public void EnableStreamerMode()
        {
            isActive = true;
            fakeViewerCount = Random.Range(50, 500);

            if (streamerOverlay != null) streamerOverlay.SetActive(true);
            if (liveIndicator != null) liveIndicator.SetActive(true);

            UpdateViewerCount();

            if (chatCoroutine != null) StopCoroutine(chatCoroutine);
            chatCoroutine = StartCoroutine(ChatMessageLoop());

            // Notify GameManager
            if (GameManager.Instance != null && !GameManager.Instance.IsStreamerMode)
            {
                GameManager.Instance.ToggleStreamerMode();
            }
        }

        /// <summary>
        /// Disables streamer mode.
        /// </summary>
        public void DisableStreamerMode()
        {
            isActive = false;

            if (streamerOverlay != null) streamerOverlay.SetActive(false);
            if (liveIndicator != null) liveIndicator.SetActive(false);

            if (chatCoroutine != null)
            {
                StopCoroutine(chatCoroutine);
                chatCoroutine = null;
            }

            if (GameManager.Instance != null && GameManager.Instance.IsStreamerMode)
            {
                GameManager.Instance.ToggleStreamerMode();
            }
        }

        /// <summary>
        /// Adds fake viewers (call on deaths, funny moments, etc).
        /// </summary>
        public void AddFakeViewers(int count)
        {
            if (!isActive) return;
            fakeViewerCount += count;
            UpdateViewerCount();
        }

        /// <summary>
        /// Shows a specific chat message.
        /// </summary>
        public void ShowChatMessage(string message)
        {
            if (!isActive || chatText == null) return;
            chatText.text = message;
        }

        private void UpdateViewerCount()
        {
            if (viewerCountText == null) return;

            int display = fakeViewerCount + Random.Range(-3, 4);
            viewerCountText.text = $"{Mathf.Max(0, display)}";
        }

        private IEnumerator ChatMessageLoop()
        {
            while (isActive)
            {
                yield return new WaitForSeconds(chatMessageInterval + Random.Range(-2f, 3f));

                if (chatText != null)
                {
                    string msg = genericChatMessages[Random.Range(0, genericChatMessages.Length)];
                    chatText.text = msg;
                }

                // Fluctuate viewer count
                fakeViewerCount += Random.Range(-5, 10);
                fakeViewerCount = Mathf.Max(10, fakeViewerCount);
                UpdateViewerCount();
            }
        }

        /// <summary>
        /// Call this when the player dies to simulate chat reaction.
        /// </summary>
        public void OnPlayerDeathChatReaction()
        {
            if (!isActive) return;

            string[] deathReactions = new string[]
            {
                "Chat: F F F F F F F",
                "Chat: OMEGALUL THAT WAS SO BAD",
                "Chat: SKILL ISSUE SKILL ISSUE",
                "Chat: widepeepoSad",
                "Chat: RIPBOZO RIPBOZO",
                "Chat: !clip"
            };

            ShowChatMessage(deathReactions[Random.Range(0, deathReactions.Length)]);
            AddFakeViewers(Random.Range(10, 50));
        }

        public bool IsActive => isActive;
    }
}
