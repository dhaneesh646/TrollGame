using UnityEngine;
using UnityEngine.Events;
using TrollGame.Core;

namespace TrollGame.Effects
{
    /// <summary>
    /// Generic trigger zone that activates troll events when the player enters.
    /// Place these throughout levels to create surprise moments.
    /// Highly configurable — works with any troll type.
    /// </summary>
    public class TrollTriggerZone : MonoBehaviour
    {
        [Header("Trigger Settings")]
        [SerializeField] private bool triggerOnce = true;
        [SerializeField] private float triggerDelay = 0f;
        [SerializeField] private string requiredTag = "Player";

        [Header("Troll Type")]
        [SerializeField] private TrollType trollType = TrollType.None;
        [SerializeField] private string trollEventName = "";

        [Header("Dialogue")]
        [SerializeField] private bool showDialogue = false;
        [SerializeField] private string dialogueSpeaker = "???";
        [SerializeField] private string dialogueMessage = "";
        [SerializeField] private float dialogueDuration = 3f;

        [Header("Screen Effects")]
        [SerializeField] private bool triggerScreenShake = false;
        [SerializeField] private float shakeIntensity = 0.5f;
        [SerializeField] private float shakeDuration = 0.5f;
        [SerializeField] private bool triggerScreenFlash = false;
        [SerializeField] private Color flashColor = Color.white;

        [Header("Audio")]
        [SerializeField] private AudioClip triggerSound;
        [SerializeField] private bool playRandomTrollSound = false;

        [Header("Events")]
        [SerializeField] private UnityEvent onTrollTriggered;

        private bool hasTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered && triggerOnce) return;
            if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

            hasTriggered = true;

            if (triggerDelay > 0)
            {
                Invoke(nameof(ExecuteTroll), triggerDelay);
            }
            else
            {
                ExecuteTroll();
            }
        }

        /// <summary>
        /// Executes all configured troll effects.
        /// </summary>
        private void ExecuteTroll()
        {
            // Notify GameManager
            if (GameManager.Instance != null && !string.IsNullOrEmpty(trollEventName))
            {
                GameManager.Instance.TriggerTrollEvent(trollEventName);
            }

            // Show dialogue
            if (showDialogue && UIManager.Instance != null)
            {
                UIManager.Instance.ShowTrollDialogue(dialogueSpeaker, dialogueMessage, dialogueDuration);
            }

            // Screen effects
            if (triggerScreenShake && ScreenEffects.Instance != null)
            {
                ScreenEffects.Instance.Shake(shakeDuration, shakeIntensity);
            }

            if (triggerScreenFlash && UIManager.Instance != null)
            {
                UIManager.Instance.FlashScreen(flashColor, 0.2f);
            }

            // Audio
            if (triggerSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(triggerSound);
            }

            if (playRandomTrollSound && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayRandomTrollSound();
            }

            // Custom events
            onTrollTriggered?.Invoke();
        }

        /// <summary>
        /// Resets the trigger so it can fire again.
        /// </summary>
        public void ResetTrigger()
        {
            hasTriggered = false;
        }
    }
}
