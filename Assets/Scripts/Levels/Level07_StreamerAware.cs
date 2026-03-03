using System.Collections;
using System.Diagnostics;
using UnityEngine;
using TrollGame.Core;

namespace TrollGame.Levels
{
    /// <summary>
    /// LEVEL 7: "Streamer Mode"
    /// 
    /// CONCEPT: The game "detects" that the player is streaming (faked detection).
    /// It starts performing for the audience — enemies pose for screenshots,
    /// the narrator addresses chat directly, and the difficulty adjusts based
    /// on "viewer count" (fake). The game becomes self-aware about being content.
    /// 
    /// CLIP MOMENT: When the game says "Hello Twitch chat! Don't forget to subscribe!"
    /// Also when enemies stop mid-attack to T-pose for the camera.
    /// 
    /// TECHNICAL: Actual OBS/streaming detection is optional (checks for common
    /// process names). The fake detection works regardless.
    /// </summary>
    public class Level07_StreamerAware : LevelBase
    {
        [Header("Streamer UI")]
        [SerializeField] private GameObject fakeViewerCountPanel;
        [SerializeField] private UnityEngine.UI.Text viewerCountText;
        [SerializeField] private UnityEngine.UI.Text chatMessageText;
        [SerializeField] private GameObject fakeDonationPopup;
        [SerializeField] private UnityEngine.UI.Text donationText;
        [SerializeField] private GameObject fakeSubscriberAlert;

        [Header("Streamer Aware Enemies")]
        [SerializeField] private GameObject[] enemies;
        [SerializeField] private float enemyPoseChance = 0.3f;

        [Header("Fake Chat Messages")]
        [SerializeField] private float chatMessageInterval = 5f;

        [Header("Difficulty Scaling")]
        [SerializeField] private float baseSpeed = 5f;
        [SerializeField] private float maxSpeed = 15f;

        private int fakeViewerCount = 0;
        private bool streamDetected = false;
        private bool hasGreetedChat = false;

        private string[] fakeChatMessages = new string[]
        {
            "xX_G4merBoi_Xx: lol this game is trolling you",
            "TwitchChatter99: KEKW",
            "SubAlert: Just fell off! OMEGALUL",
            "ModeratorBot: !clip",
            "ProGamer420: skill issue tbh",
            "StreamFan_01: can you play something else",
            "Anonymous: THIS IS SO FUNNY LMAOOO",
            "BackseatGamer: go left... LEFT... YOUR OTHER LEFT",
            "TrollWatcher: the game is watching you watch it",
            "ClipChamp: CLIP THAT CLIP THAT",
            "DonationDave: donated $5 to see you suffer more",
            "LurkerLarry: been here 2 hours, worth every second",
            "MemeConnoisseur: this game was made specifically for content",
            "RageQuitPredictor: giving it 5 more minutes before ragequit",
            "FirstTimeViewer: what is even happening rn",
            "ChatSensei: have you tried not dying?",
            "EmoteSpammer: PogChamp PogChamp PogChamp",
            "SubscriberSam: I subbed for THIS?",
            "GiftedSub: someone gifted me a sub to watch this pain"
        };

        private string[] fakeDonationMessages = new string[]
        {
            "$5 from TrollFan: 'Die again!'",
            "$10 from ProGamer: 'You're so bad it's entertaining'",
            "$1 from CheapSkate: 'Worth every penny watching you suffer'",
            "$20 from BigSpender: 'Do a flip off the next cliff'",
            "$3 from Anonymous: 'The game is smarter than you'",
            "$50 from WhaleDonor: 'I paid $50 to tell you to git gud'",
            "$2 from MemeLord: 'This is peak content'",
            "$0.01 from MinimumEffort: 'lol'"
        };

        protected override void Awake()
        {
            base.Awake();
            levelName = "Streamer Mode";
            primaryTrollType = TrollType.StreamerAware;
            trollTriggerDelay = 3f;
        }

        protected override void OnLevelStart()
        {
            // Check for actual streaming software (safe, non-invasive check)
            streamDetected = CheckForStreamingSoftware();

            if (fakeViewerCountPanel != null) fakeViewerCountPanel.SetActive(false);
            if (fakeDonationPopup != null) fakeDonationPopup.SetActive(false);
            if (fakeSubscriberAlert != null) fakeSubscriberAlert.SetActive(false);

            ShowTrollMessage("Level 7: A totally normal platforming challenge...");
        }

        protected override void OnTrollBegin()
        {
            StartCoroutine(StreamerAwareSequence());
        }

        protected override void OnTrollUpdate()
        {
            // Gradually increase fake viewer count based on deaths
            if (gameManager != null)
            {
                fakeViewerCount = 100 + (gameManager.PlayerDeathCount * 50) +
                    (int)(GetLevelPlayTime() * 2f);
            }

            UpdateViewerCount();
        }

        protected override bool CheckLevelComplete()
        {
            return levelComplete;
        }

        /// <summary>
        /// Main streamer-aware sequence.
        /// </summary>
        private IEnumerator StreamerAwareSequence()
        {
            // Phase 1: "Detection"
            yield return StartCoroutine(StreamDetectionPhase());

            // Phase 2: Greet the audience
            yield return StartCoroutine(GreetAudiencePhase());

            // Phase 3: Start fake chat and interactions
            StartCoroutine(FakeChatLoop());
            StartCoroutine(FakeDonationLoop());

            // Phase 4: Enemies become camera-aware
            yield return StartCoroutine(EnemyAwarenessPhase());

            // Phase 5: "Content mode" difficulty
            yield return StartCoroutine(ContentModeDifficulty());
        }

        /// <summary>
        /// Phase 1: The game "detects" streaming software.
        /// </summary>
        private IEnumerator StreamDetectionPhase()
        {
            ShowTrollMessage("Scanning system...");

            yield return new WaitForSeconds(2f);

            if (streamDetected)
            {
                ShowTrollMessage("STREAM DETECTED! OBS is running! Hello, viewers!");
            }
            else
            {
                ShowTrollMessage("Stream detected! (We're lying, but go with it)");
            }

            yield return new WaitForSeconds(1f);

            // Enable viewer count
            if (fakeViewerCountPanel != null)
            {
                fakeViewerCountPanel.SetActive(true);
            }

            fakeViewerCount = UnityEngine.Random.Range(50, 500);
        }

        /// <summary>
        /// Phase 2: The game greets "the audience."
        /// </summary>
        private IEnumerator GreetAudiencePhase()
        {
            if (hasGreetedChat) yield break;
            hasGreetedChat = true;

            ShowTrollMessage("Hello Twitch chat! Don't forget to like and subscribe!");

            yield return new WaitForSeconds(3f);

            ShowTrollMessage("Chat, should I make this harder? ...Chat says yes. Always yes.");

            yield return new WaitForSeconds(3f);

            string playerName = "Streamer";
            if (gameManager != null) playerName = gameManager.PlayerName;

            ShowTrollMessage($"Let's see how {playerName} handles this next part. Place your bets, chat!");

            yield return new WaitForSeconds(2f);

            // Toggle streamer mode in GameManager
            if (gameManager != null && !gameManager.IsStreamerMode)
            {
                gameManager.ToggleStreamerMode();
            }
        }

        /// <summary>
        /// Fake chat messages appearing periodically.
        /// </summary>
        private IEnumerator FakeChatLoop()
        {
            while (!levelComplete)
            {
                yield return new WaitForSeconds(chatMessageInterval + UnityEngine.Random.Range(-2f, 2f));

                if (chatMessageText != null)
                {
                    string msg = fakeChatMessages[UnityEngine.Random.Range(0, fakeChatMessages.Length)];
                    chatMessageText.text = msg;

                    // Fade out after a few seconds
                    yield return new WaitForSeconds(3f);
                    chatMessageText.text = "";
                }
            }
        }

        /// <summary>
        /// Fake donation popups appearing occasionally.
        /// </summary>
        private IEnumerator FakeDonationLoop()
        {
            while (!levelComplete)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(15f, 30f));

                if (fakeDonationPopup != null && donationText != null)
                {
                    string donation = fakeDonationMessages[UnityEngine.Random.Range(0, fakeDonationMessages.Length)];
                    donationText.text = donation;
                    fakeDonationPopup.SetActive(true);

                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayNotification();
                    }

                    yield return new WaitForSeconds(5f);
                    fakeDonationPopup.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Phase 4: Enemies become "camera aware" — they pose, wave, etc.
        /// </summary>
        private IEnumerator EnemyAwarenessPhase()
        {
            ShowTrollMessage("The enemies know they're being watched...");

            yield return new WaitForSeconds(2f);

            // Enemies occasionally stop to "pose"
            if (enemies != null)
            {
                foreach (GameObject enemy in enemies)
                {
                    if (enemy == null) continue;

                    // Enemies would have a component that makes them occasionally T-pose
                    // or wave at the camera. The behavior is handled by EnemyStreamerAware component.
                }
            }
        }

        /// <summary>
        /// Phase 5: Difficulty scales with "viewer count" for content.
        /// </summary>
        private IEnumerator ContentModeDifficulty()
        {
            ShowTrollMessage("Difficulty now scales with viewer count! More viewers = harder game!");

            while (!levelComplete)
            {
                yield return new WaitForSeconds(5f);

                // Increase "viewer count" when the player dies
                // The "audience" loves watching failure
                if (playerDeathsThisLevel > 0)
                {
                    fakeViewerCount += playerDeathsThisLevel * 10;

                    if (playerDeathsThisLevel > 3)
                    {
                        ShowFakeChatMessage("ClipThis: THIS IS THE BEST STREAM EVER LMAOOO");
                    }
                }
            }
        }

        /// <summary>
        /// Checks if streaming software is running (safe, non-invasive).
        /// Only checks process names — does NOT interact with the processes.
        /// </summary>
        private bool CheckForStreamingSoftware()
        {
            try
            {
                string[] streamingProcesses = new string[]
                {
                    "obs64", "obs32", "obs",
                    "streamlabs",
                    "xsplit",
                    "twitch"
                };

                Process[] processes = Process.GetProcesses();
                foreach (Process process in processes)
                {
                    try
                    {
                        string processName = process.ProcessName.ToLowerInvariant();
                        foreach (string streamProcess in streamingProcesses)
                        {
                            if (processName.Contains(streamProcess))
                            {
                                return true;
                            }
                        }
                    }
                    catch
                    {
                        // Skip processes we can't access (normal behavior)
                    }
                }
            }
            catch
            {
                // If process enumeration fails, just fake it
            }

            return false;
        }

        /// <summary>
        /// Updates the viewer count display.
        /// </summary>
        private void UpdateViewerCount()
        {
            if (viewerCountText != null)
            {
                // Add some random fluctuation
                int displayCount = fakeViewerCount + UnityEngine.Random.Range(-5, 5);
                viewerCountText.text = $"LIVE {Mathf.Max(0, displayCount)} viewers";
            }
        }

        /// <summary>
        /// Shows a fake chat message.
        /// </summary>
        private void ShowFakeChatMessage(string message)
        {
            if (chatMessageText != null)
            {
                chatMessageText.text = message;
            }
        }

        /// <summary>
        /// Call this to mark the level as complete (triggered by reaching the exit).
        /// </summary>
        public void CompleteLevel()
        {
            StartCoroutine(LevelCompleteSequence());
        }

        private IEnumerator LevelCompleteSequence()
        {
            ShowTrollMessage("GG chat! That's the end of Streamer Mode!");

            yield return new WaitForSeconds(2f);

            ShowFakeChatMessage("PogChamp PogChamp PogChamp");
            fakeViewerCount += 1000; // "Viewer spike" on completion

            yield return new WaitForSeconds(2f);

            ShowTrollMessage("Thanks for watching! Don't forget to follow!");

            yield return new WaitForSeconds(2f);

            levelComplete = true;
            OnLevelComplete();
        }

        protected override void OnDeathTroll()
        {
            // Deaths increase "viewer count" (the audience loves pain)
            fakeViewerCount += 50;

            string[] deathMessages = new string[]
            {
                $"{fakeViewerCount} people just watched you die. Content!",
                "That death will make a great clip.",
                "Chat is typing 'F' right now.",
                "Your viewer count just went UP because of that death.",
                "Somebody definitely clipped that."
            };

            ShowTrollMessage(deathMessages[UnityEngine.Random.Range(0, deathMessages.Length)]);
        }
    }
}
