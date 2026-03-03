using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrollGame.Core
{
    /// <summary>
    /// Central event system for coordinating troll mechanics across the game.
    /// Manages troll queues, cooldowns, and chains for maximum comedic effect.
    /// </summary>
    public class TrollEventSystem : MonoBehaviour
    {
        public static TrollEventSystem Instance { get; private set; }

        [Header("Troll Queue Settings")]
        [SerializeField] private float minimumTrollCooldown = 3f;
        [SerializeField] private float maximumTrollCooldown = 15f;
        [SerializeField] private int maxQueuedTrolls = 5;

        private Queue<TrollEvent> trollQueue = new Queue<TrollEvent>();
        private List<TrollEvent> activeTrolls = new List<TrollEvent>();
        private float lastTrollTime = 0f;
        private bool trollingPaused = false;

        // Events
        public event Action<TrollEvent> OnTrollStarted;
        public event Action<TrollEvent> OnTrollCompleted;
        public event Action<TrollEvent> OnTrollFailed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            if (trollingPaused) return;

            // Process queued trolls
            if (trollQueue.Count > 0 && Time.time - lastTrollTime > minimumTrollCooldown)
            {
                TrollEvent nextTroll = trollQueue.Dequeue();
                ExecuteTroll(nextTroll);
            }

            // Clean up completed trolls
            activeTrolls.RemoveAll(t => t.IsComplete);
        }

        /// <summary>
        /// Queues a troll event for execution.
        /// </summary>
        public void QueueTroll(TrollEvent trollEvent)
        {
            if (trollQueue.Count >= maxQueuedTrolls) return;

            trollQueue.Enqueue(trollEvent);
        }

        /// <summary>
        /// Immediately executes a troll event, bypassing the queue.
        /// Use for critical story moments or player-triggered trolls.
        /// </summary>
        public void ExecuteTrollImmediate(TrollEvent trollEvent)
        {
            ExecuteTroll(trollEvent);
        }

        /// <summary>
        /// Executes a troll chain - a sequence of trolls that play in order.
        /// Perfect for multi-stage tricks like: fake crash -> fake BSOD -> "recovery" -> another crash.
        /// </summary>
        public void ExecuteTrollChain(List<TrollEvent> chain)
        {
            StartCoroutine(ProcessTrollChain(chain));
        }

        private IEnumerator ProcessTrollChain(List<TrollEvent> chain)
        {
            foreach (TrollEvent trollEvent in chain)
            {
                ExecuteTroll(trollEvent);

                // Wait for the troll to complete before moving to the next
                while (!trollEvent.IsComplete)
                {
                    yield return null;
                }

                // Brief pause between chain links for dramatic effect
                yield return new WaitForSeconds(trollEvent.ChainDelay);
            }
        }

        private void ExecuteTroll(TrollEvent trollEvent)
        {
            trollEvent.IsComplete = false;
            activeTrolls.Add(trollEvent);
            lastTrollTime = Time.time;

            OnTrollStarted?.Invoke(trollEvent);

            if (trollEvent.ExecuteAction != null)
            {
                trollEvent.ExecuteAction.Invoke();
            }

            // Auto-complete after duration if specified
            if (trollEvent.Duration > 0)
            {
                StartCoroutine(AutoCompleteTroll(trollEvent));
            }
        }

        private IEnumerator AutoCompleteTroll(TrollEvent trollEvent)
        {
            yield return new WaitForSeconds(trollEvent.Duration);

            if (!trollEvent.IsComplete)
            {
                CompleteTroll(trollEvent);
            }
        }

        /// <summary>
        /// Marks a troll event as completed.
        /// </summary>
        public void CompleteTroll(TrollEvent trollEvent)
        {
            trollEvent.IsComplete = true;
            OnTrollCompleted?.Invoke(trollEvent);
        }

        /// <summary>
        /// Pauses all trolling (used during cutscenes or important moments).
        /// </summary>
        public void PauseTrolling()
        {
            trollingPaused = true;
        }

        /// <summary>
        /// Resumes trolling.
        /// </summary>
        public void ResumeTrolling()
        {
            trollingPaused = false;
        }

        /// <summary>
        /// Clears all queued and active trolls.
        /// </summary>
        public void ClearAllTrolls()
        {
            trollQueue.Clear();
            foreach (TrollEvent troll in activeTrolls)
            {
                troll.IsComplete = true;
            }
            activeTrolls.Clear();
        }

        /// <summary>
        /// Creates and queues a simple timed troll.
        /// </summary>
        public void QuickTroll(string name, TrollType type, float duration, Action executeAction)
        {
            TrollEvent trollEvent = new TrollEvent
            {
                Name = name,
                Type = type,
                Duration = duration,
                ExecuteAction = executeAction
            };
            QueueTroll(trollEvent);
        }
    }

    /// <summary>
    /// Represents a single troll event with all its configuration.
    /// </summary>
    [Serializable]
    public class TrollEvent
    {
        public string Name;
        public TrollType Type;
        public float Duration;
        public float ChainDelay = 0.5f;
        public bool IsComplete;
        public Action ExecuteAction;
        public string DialogueText;
        public bool RequiresPlayerInput;
    }
}
