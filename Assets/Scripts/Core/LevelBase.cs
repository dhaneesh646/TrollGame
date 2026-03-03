using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrollGame.Core
{
    /// <summary>
    /// Abstract base class for all troll levels.
    /// Each level implements its own unique troll mechanics while sharing common infrastructure.
    /// </summary>
    public abstract class LevelBase : MonoBehaviour
    {
        [Header("Level Info")]
        [SerializeField] protected string levelName = "Unnamed Level";
        [SerializeField] protected string levelDescription = "";
        [SerializeField] protected float levelDifficulty = 1.0f;

        [Header("Troll Configuration")]
        [SerializeField] protected TrollType primaryTrollType = TrollType.None;
        [SerializeField] protected float trollTriggerDelay = 2f;
        [SerializeField] protected bool autoStartTrolling = true;

        [Header("Completion")]
        [SerializeField] protected bool levelComplete = false;
        [SerializeField] protected float minimumPlayTime = 10f;

        protected GameManager gameManager;
        protected TrollEventSystem trollSystem;
        protected float levelStartTime;
        protected bool trollingStarted = false;
        protected int playerDeathsThisLevel = 0;

        /// <summary>
        /// Called when the level first loads. Override to set up level-specific elements.
        /// </summary>
        protected virtual void Awake()
        {
            gameManager = GameManager.Instance;
            trollSystem = TrollEventSystem.Instance;
        }

        /// <summary>
        /// Called at start. Override to begin level-specific logic.
        /// </summary>
        protected virtual void Start()
        {
            levelStartTime = Time.time;

            if (autoStartTrolling)
            {
                StartCoroutine(DelayedTrollStart());
            }

            OnLevelStart();
        }

        /// <summary>
        /// Called every frame. Override for level-specific update logic.
        /// </summary>
        protected virtual void Update()
        {
            if (trollingStarted)
            {
                OnTrollUpdate();
            }
        }

        /// <summary>
        /// Override this to define what happens when the level starts.
        /// This is where you set up the initial "normal" experience before trolling begins.
        /// </summary>
        protected abstract void OnLevelStart();

        /// <summary>
        /// Override this to define the main troll mechanic for this level.
        /// Called once after the troll trigger delay.
        /// </summary>
        protected abstract void OnTrollBegin();

        /// <summary>
        /// Override this for continuous troll behavior (called every frame after trolling starts).
        /// </summary>
        protected virtual void OnTrollUpdate() { }

        /// <summary>
        /// Override this to define the level completion condition.
        /// </summary>
        protected abstract bool CheckLevelComplete();

        /// <summary>
        /// Override this to define what happens when the player "completes" the level.
        /// Could be a real completion or a fake one (another troll!).
        /// </summary>
        protected virtual void OnLevelComplete()
        {
            levelComplete = true;

            if (gameManager != null)
            {
                gameManager.NextLevel();
            }
        }

        /// <summary>
        /// Called when the player dies in this level.
        /// </summary>
        public virtual void OnPlayerDeath()
        {
            playerDeathsThisLevel++;

            if (gameManager != null)
            {
                gameManager.RegisterPlayerDeath();
            }

            OnDeathTroll();
        }

        /// <summary>
        /// Override to add special trolling on player death.
        /// </summary>
        protected virtual void OnDeathTroll()
        {
            // Base: nothing special. Override in levels for death-specific trolling.
        }

        /// <summary>
        /// Delays the start of trolling to let the player get comfortable first.
        /// The best trolls come when the player least expects them.
        /// </summary>
        private IEnumerator DelayedTrollStart()
        {
            yield return new WaitForSeconds(trollTriggerDelay);
            trollingStarted = true;
            OnTrollBegin();
        }

        /// <summary>
        /// Starts trolling immediately (for levels that don't need a delay).
        /// </summary>
        protected void StartTrollingNow()
        {
            trollingStarted = true;
            OnTrollBegin();
        }

        /// <summary>
        /// Utility: Gets the time the player has spent in this level.
        /// </summary>
        protected float GetLevelPlayTime()
        {
            return Time.time - levelStartTime;
        }

        /// <summary>
        /// Utility: Checks if the player has been in the level long enough.
        /// </summary>
        protected bool HasMinimumPlayTimePassed()
        {
            return GetLevelPlayTime() >= minimumPlayTime;
        }

        /// <summary>
        /// Shows a troll dialogue message to the player.
        /// </summary>
        protected void ShowTrollMessage(string message, float duration = 3f)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowTrollDialogue(message, duration);
            }
        }

        /// <summary>
        /// Shows a fake error popup.
        /// </summary>
        protected void ShowFakeError(string title, string message)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFakeErrorPopup(title, message);
            }
        }
    }
}
