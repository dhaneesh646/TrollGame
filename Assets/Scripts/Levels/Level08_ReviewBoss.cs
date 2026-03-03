using System.Collections;
using UnityEngine;
using TrollGame.Core;

namespace TrollGame.Levels
{
    /// <summary>
    /// LEVEL 8: "The Review"
    /// 
    /// CONCEPT: The game itself becomes a boss that BEGS the player not to
    /// leave a bad Steam review. The "boss" is a giant floating Steam review
    /// box that attacks with negative review text, star ratings as projectiles,
    /// and "helpful" vote buttons as shields.
    /// 
    /// CLIP MOMENT: When the boss literally says "Please don't give me 1 star,
    /// I have a family!" while firing thumbs-down projectiles.
    /// The game negotiates IN REAL TIME during the fight.
    /// 
    /// TWIST: If the player "loses," the game writes a fake positive review
    /// on screen. If the player wins, it threatens to make the next level
    /// impossible (and follows through).
    /// </summary>
    public class Level08_ReviewBoss : LevelBase
    {
        [Header("Boss UI")]
        [SerializeField] private GameObject reviewBossPanel;
        [SerializeField] private UnityEngine.UI.Text bossDialogueText;
        [SerializeField] private UnityEngine.UI.Slider bossHealthBar;
        [SerializeField] private UnityEngine.UI.Text bossNameText;
        [SerializeField] private GameObject starProjectilePrefab;
        [SerializeField] private GameObject thumbsDownPrefab;

        [Header("Boss Settings")]
        [SerializeField] private float bossMaxHealth = 100f;
        [SerializeField] private float bossDamagePerHit = 10f;
        [SerializeField] private Transform bossTransform;
        [SerializeField] private Transform playerTransform;

        [Header("Negotiation Phases")]
        [SerializeField] private float negotiationInterval = 15f;

        private float bossHealth;
        private int negotiationPhase = 0;
        private bool bossDefeated = false;
        private bool playerLost = false;
        private int playerHitsOnBoss = 0;

        private string[] bossDialoguePhases = new string[]
        {
            "Welcome to... wait, are you going to review me? Please be gentle.",
            "OW! That hurt! Come on, I'm just a game! Don't be mean!",
            "Okay okay, I'll stop trolling! Just give me 5 stars!",
            "I'LL GIVE YOU A DISCOUNT! 50% OFF! Just don't hit me!",
            "Please! I have a family! Little DLC babies to feed!",
            "Fine! I'll actually be a good game! No more trolling! ...Starting next level.",
            "YOU MONSTER! I gave you HOURS of entertainment and THIS is how you repay me?!",
            "I'm calling my lawyer. My DIGITAL lawyer.",
            "Okay I'm getting desperate. What if I gave you the final boss code?",
            "FINE! Hit me! See if I care! *crying in binary*"
        };

        private string[] negotiationOffers = new string[]
        {
            "DEAL: Stop hitting me and I'll reduce difficulty by 50%!",
            "DEAL: I'll skip the next level entirely! Just stop!",
            "DEAL: Free DLC! (It's just more trolling but still!)",
            "DEAL: I'll tell you the ending! (The ending is more trolling)",
            "DEAL: I'll actually make a good level! One! Just ONE!",
            "FINAL OFFER: I'll stop breaking the 4th wall for 5 minutes!"
        };

        protected override void Awake()
        {
            base.Awake();
            levelName = "The Review";
            primaryTrollType = TrollType.FakeEnding;
            trollTriggerDelay = 0f;
            autoStartTrolling = false;
        }

        protected override void OnLevelStart()
        {
            bossHealth = bossMaxHealth;

            if (bossHealthBar != null)
            {
                bossHealthBar.maxValue = bossMaxHealth;
                bossHealthBar.value = bossHealth;
            }

            if (bossNameText != null)
            {
                bossNameText.text = "BOSS: TrollGame.exe (Wants 5 Stars)";
            }

            StartCoroutine(BossIntroSequence());
        }

        protected override void OnTrollBegin() { }

        protected override bool CheckLevelComplete()
        {
            return bossDefeated || playerLost;
        }

        /// <summary>
        /// Boss introduction sequence.
        /// </summary>
        private IEnumerator BossIntroSequence()
        {
            ShowTrollMessage("FINAL BOSS APPROACHING...");

            yield return new WaitForSeconds(2f);

            if (reviewBossPanel != null) reviewBossPanel.SetActive(true);

            SetBossDialogue("Oh... oh no. You made it this far?");

            yield return new WaitForSeconds(3f);

            SetBossDialogue("Look, before we fight... have you considered giving me a NICE review?");

            yield return new WaitForSeconds(3f);

            SetBossDialogue("No? Fine. Let's do this. But I'm going to NEGOTIATE during the fight!");

            yield return new WaitForSeconds(2f);

            // Start the boss fight
            StartCoroutine(BossAttackLoop());
            StartCoroutine(NegotiationLoop());
        }

        /// <summary>
        /// Boss attack loop — fires star ratings and thumbs-down as projectiles.
        /// </summary>
        private IEnumerator BossAttackLoop()
        {
            while (!bossDefeated && !playerLost)
            {
                yield return new WaitForSeconds(2f);

                // Attack patterns based on health
                float healthPercent = bossHealth / bossMaxHealth;

                if (healthPercent > 0.7f)
                {
                    // Gentle attacks — single star projectiles
                    FireStarProjectile(1);
                    SetBossDialogue("Hey! That's 1 star! Like the review you should NOT give me!");
                }
                else if (healthPercent > 0.4f)
                {
                    // Medium attacks — multiple projectiles
                    for (int i = 0; i < 3; i++)
                    {
                        FireStarProjectile(Random.Range(1, 4));
                        yield return new WaitForSeconds(0.3f);
                    }
                    SetBossDialogue("TAKE THESE NEGATIVE REVIEWS! See how YOU like it!");
                }
                else
                {
                    // Desperate attacks — rapid fire + thumbs down
                    for (int i = 0; i < 5; i++)
                    {
                        FireStarProjectile(1);
                        FireThumbsDown();
                        yield return new WaitForSeconds(0.15f);
                    }
                    SetBossDialogue("THIS IS WHAT 1-STAR REVIEWS FEEL LIKE!!!");
                }

                yield return null;
            }
        }

        /// <summary>
        /// Negotiation loop — the boss tries to bargain with the player during combat.
        /// </summary>
        private IEnumerator NegotiationLoop()
        {
            while (!bossDefeated && !playerLost)
            {
                yield return new WaitForSeconds(negotiationInterval);

                if (negotiationPhase < negotiationOffers.Length)
                {
                    string offer = negotiationOffers[negotiationPhase];
                    ShowTrollMessage(offer);
                    negotiationPhase++;

                    // Brief ceasefire during negotiation
                    yield return new WaitForSeconds(3f);
                }
            }
        }

        /// <summary>
        /// Called when the player hits the boss.
        /// </summary>
        public void OnBossHit(float damage)
        {
            if (bossDefeated) return;

            bossHealth -= damage;
            playerHitsOnBoss++;

            if (bossHealthBar != null)
            {
                bossHealthBar.value = bossHealth;
            }

            // Boss reacts to being hit
            int dialogueIndex = Mathf.Min(playerHitsOnBoss - 1, bossDialoguePhases.Length - 1);
            if (dialogueIndex >= 0)
            {
                SetBossDialogue(bossDialoguePhases[dialogueIndex]);
            }

            // Check defeat
            if (bossHealth <= 0)
            {
                StartCoroutine(BossDefeatSequence());
            }
        }

        /// <summary>
        /// Boss defeat sequence.
        /// </summary>
        private IEnumerator BossDefeatSequence()
        {
            bossDefeated = true;

            SetBossDialogue("NOOOOO! You actually beat me?!");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayVictoryFanfare();
            }

            yield return new WaitForSeconds(3f);

            SetBossDialogue("Fine... you win... but I'm making the next level IMPOSSIBLE.");

            yield return new WaitForSeconds(3f);

            SetBossDialogue("Also, I'm STILL going to ask for 5 stars in the Steam overlay. Watch me.");

            yield return new WaitForSeconds(3f);

            // Fake achievement
            ShowTrollMessage("ACHIEVEMENT UNLOCKED: 'Bullied A Game Until It Cried'");

            yield return new WaitForSeconds(2f);

            if (reviewBossPanel != null) reviewBossPanel.SetActive(false);

            OnLevelComplete();
        }

        /// <summary>
        /// Called if the player loses the fight.
        /// </summary>
        public void OnPlayerDefeatedByBoss()
        {
            playerLost = true;
            StartCoroutine(PlayerLossSequence());
        }

        private IEnumerator PlayerLossSequence()
        {
            SetBossDialogue("HA! I WIN! Now watch this...");

            yield return new WaitForSeconds(2f);

            // The game "writes" a fake review
            SetBossDialogue("Writing review on your behalf...\n\n" +
                "★★★★★ 'Best game ever. 10/10 would get trolled again.'\n" +
                "- " + (gameManager != null ? gameManager.PlayerName : "Player"));

            yield return new WaitForSeconds(4f);

            SetBossDialogue("There. That's YOUR review now. You're welcome.");

            yield return new WaitForSeconds(3f);

            // Still complete the level (it's a troll, not a wall)
            OnLevelComplete();
        }

        /// <summary>
        /// Fires a star rating projectile at the player.
        /// </summary>
        private void FireStarProjectile(int starCount)
        {
            if (starProjectilePrefab == null || bossTransform == null || playerTransform == null) return;

            GameObject star = Instantiate(starProjectilePrefab, bossTransform.position, Quaternion.identity);

            // Set projectile direction toward player
            Vector3 direction = (playerTransform.position - bossTransform.position).normalized;
            Rigidbody rb = star.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * (10f + starCount * 2f);
            }

            // Destroy after 5 seconds
            Destroy(star, 5f);
        }

        /// <summary>
        /// Fires a thumbs-down projectile.
        /// </summary>
        private void FireThumbsDown()
        {
            if (thumbsDownPrefab == null || bossTransform == null || playerTransform == null) return;

            GameObject thumb = Instantiate(thumbsDownPrefab, bossTransform.position, Quaternion.identity);

            Vector3 direction = (playerTransform.position - bossTransform.position).normalized;
            direction += new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);

            Rigidbody rb = thumb.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction.normalized * 15f;
            }

            Destroy(thumb, 5f);
        }

        private void SetBossDialogue(string text)
        {
            if (bossDialogueText != null)
            {
                bossDialogueText.text = text;
            }
        }

        protected override void OnDeathTroll()
        {
            float healthPct = (bossHealth / bossMaxHealth) * 100f;

            string[] messages = new string[]
            {
                $"Boss HP: {healthPct:F0}%. The review box is winning.",
                "Killed by a Steam review. Put THAT on your resume.",
                "The boss gives your death a 1-star rating.",
                "Review: 'Player was not helpful. 0/10 would not recommend.'"
            };

            ShowTrollMessage(messages[Random.Range(0, messages.Length)]);
        }
    }
}
