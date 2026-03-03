using UnityEngine;
using TrollGame.Core;

namespace TrollGame.Effects
{
    /// <summary>
    /// Kill zone that triggers player death on contact.
    /// Place these under platforms, around hazards, etc.
    /// Includes configurable death messages for troll flavor.
    /// </summary>
    public class DeathZone : MonoBehaviour
    {
        [Header("Death Settings")]
        [SerializeField] private string requiredTag = "Player";
        [SerializeField] private bool showDeathMessage = true;

        [Header("Custom Death Messages")]
        [SerializeField] private string[] deathMessages = new string[]
        {
            "You died. Shocking.",
            "The floor was lava. And you touched it.",
            "Gravity wins again.",
            "That was the worst jump I've ever seen.",
            "The void claimed you. It wasn't hungry, just bored.",
            "You fell. Into nothing. On purpose?",
            "Death #??? - We lost count.",
            "The spikes say hello. And goodbye.",
            "You walked into that one. Literally."
        };

        [Header("Respawn")]
        [SerializeField] private Transform respawnPoint;
        [SerializeField] private float respawnDelay = 1f;

        private void OnTriggerEnter(Collider other)
        {
            if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

            KillPlayer(other.gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!string.IsNullOrEmpty(requiredTag) && !collision.gameObject.CompareTag(requiredTag)) return;

            KillPlayer(collision.gameObject);
        }

        private void KillPlayer(GameObject player)
        {
            // Show death message
            if (showDeathMessage && deathMessages.Length > 0 && UIManager.Instance != null)
            {
                string msg = deathMessages[Random.Range(0, deathMessages.Length)];
                UIManager.Instance.ShowDeathScreen(msg);
            }

            // Play death sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayDeathSound();
            }

            // Screen effect
            if (ScreenEffects.Instance != null)
            {
                ScreenEffects.Instance.Shake(0.3f, 0.5f);
            }

            // Notify current level
            LevelBase currentLevel = FindFirstObjectByType<LevelBase>();
            if (currentLevel != null)
            {
                currentLevel.OnPlayerDeath();
            }

            // Respawn after delay
            if (respawnPoint != null)
            {
                StartCoroutine(RespawnRoutine(player));
            }
        }

        private System.Collections.IEnumerator RespawnRoutine(GameObject player)
        {
            // Brief disable
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            yield return new WaitForSeconds(respawnDelay);

            // Teleport to respawn
            player.transform.position = respawnPoint.position;

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }

            // Hide death screen
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideDeathScreen();
            }
        }
    }
}
