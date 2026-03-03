using System.Collections;
using UnityEngine;
using TrollGame.Core;

namespace TrollGame.Levels
{
    /// <summary>
    /// LEVEL 9: "The Rage Room"
    /// 
    /// CONCEPT: Pure, distilled rage platforming. Inspired by "Getting Over It"
    /// and "Jump King." Every death sends the player back to the start.
    /// BUT the game also dynamically reacts to the player's frustration:
    /// - Rage clicking makes platforms slippery
    /// - Pausing too long makes spikes grow
    /// - The game narrator becomes increasingly sarcastic
    /// - Death count is displayed PROMINENTLY
    /// 
    /// CLIP MOMENT: Every single death. The escalating narrator commentary.
    /// When the player finally beats it and the game says "That took you X deaths."
    /// 
    /// MEME INTEGRATION: Platforms are made of meme characters.
    /// Solo Penguin appears as a moving platform. Punch Monkey is a hazard.
    /// </summary>
    public class Level09_RagePlatformer : LevelBase
    {
        [Header("Rage Platformer")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float exitTriggerDistance = 2f;

        [Header("Death Display")]
        [SerializeField] private UnityEngine.UI.Text bigDeathCountText;
        [SerializeField] private UnityEngine.UI.Text narratorText;
        [SerializeField] private UnityEngine.UI.Text timerText;

        [Header("Dynamic Difficulty")]
        [SerializeField] private PhysicsMaterial slipperyMaterial;
        [SerializeField] private PhysicsMaterial normalMaterial;
        [SerializeField] private GameObject[] spikeGroups;
        [SerializeField] private float spikeGrowRate = 0.1f;

        [Header("Meme Platforms")]
        [SerializeField] private GameObject soloPenguinPlatform;
        [SerializeField] private GameObject punchMonkeyHazard;
        [SerializeField] private float penguinMoveSpeed = 2f;
        [SerializeField] private float penguinMoveRange = 5f;

        [Header("Settings")]
        [SerializeField] private float rageCooldownTime = 5f;

        private float levelTimer = 0f;
        private int deathsThisLevel = 0;
        private float lastDeathTime = 0f;
        private float rageLevel = 0f;
        private bool platformsSlippery = false;
        private float lastInputTime = 0f;
        private float idleTime = 0f;
        private Vector3 penguinStartPos;

        // Narrator commentary gets more savage with deaths
        private string[] narratorComments;

        protected override void Awake()
        {
            base.Awake();
            levelName = "The Rage Room";
            primaryTrollType = TrollType.RagePlatformer;
            trollTriggerDelay = 0f;
            autoStartTrolling = false;

            BuildNarratorComments();
        }

        protected override void Start()
        {
            base.Start();

            if (soloPenguinPlatform != null)
            {
                penguinStartPos = soloPenguinPlatform.transform.position;
            }
        }

        protected override void OnLevelStart()
        {
            UpdateDeathDisplay();
            ShowNarratorText("Welcome to the Rage Room. You will die. A lot.");

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SwitchToTrollMusic();
            }
        }

        protected override void OnTrollBegin() { }

        protected override void Update()
        {
            base.Update();

            levelTimer += Time.deltaTime;
            UpdateTimerDisplay();

            // Track player input for rage/idle detection
            if (Input.anyKey)
            {
                lastInputTime = Time.time;
                idleTime = 0f;
            }
            else
            {
                idleTime += Time.deltaTime;
            }

            // Rage detection
            DetectRage();

            // Dynamic difficulty adjustments
            ApplyDynamicDifficulty();

            // Animate meme platforms
            AnimateMemePlatforms();

            // Check for level completion
            if (playerTransform != null && endPoint != null)
            {
                float dist = Vector3.Distance(playerTransform.position, endPoint.position);
                if (dist < exitTriggerDistance)
                {
                    StartCoroutine(LevelCompleteSequence());
                }
            }
        }

        protected override bool CheckLevelComplete()
        {
            return levelComplete;
        }

        /// <summary>
        /// Detects player rage through input patterns.
        /// </summary>
        private void DetectRage()
        {
            // Rapid deaths = rage
            float timeSinceLastDeath = Time.time - lastDeathTime;
            if (timeSinceLastDeath < 3f)
            {
                rageLevel += Time.deltaTime * 2f;
            }
            else
            {
                rageLevel = Mathf.Max(0, rageLevel - Time.deltaTime * 0.5f);
            }

            rageLevel = Mathf.Clamp(rageLevel, 0f, 10f);
        }

        /// <summary>
        /// Applies dynamic difficulty based on player behavior.
        /// The game gets HARDER when you're frustrated (the ultimate troll).
        /// </summary>
        private void ApplyDynamicDifficulty()
        {
            // Rage clicking → slippery platforms
            if (rageLevel > 5f && !platformsSlippery)
            {
                MakePlatformsSlippery(true);
                ShowNarratorText("Your rage is making the platforms slippery. Maybe calm down?");
            }
            else if (rageLevel < 2f && platformsSlippery)
            {
                MakePlatformsSlippery(false);
            }

            // Idle too long → spikes grow
            if (idleTime > 10f)
            {
                GrowSpikes();

                if (idleTime > 10f && idleTime < 10.5f)
                {
                    ShowNarratorText("Standing still? The spikes are getting bored. And bigger.");
                }
            }
        }

        /// <summary>
        /// Makes all platforms slippery or normal.
        /// </summary>
        private void MakePlatformsSlippery(bool slippery)
        {
            platformsSlippery = slippery;
            // In a full implementation, iterate through platforms and swap materials
        }

        /// <summary>
        /// Grows spikes when the player is idle.
        /// </summary>
        private void GrowSpikes()
        {
            if (spikeGroups == null) return;

            foreach (GameObject spikeGroup in spikeGroups)
            {
                if (spikeGroup == null) continue;
                Vector3 scale = spikeGroup.transform.localScale;
                scale.y += spikeGrowRate * Time.deltaTime;
                scale.y = Mathf.Min(scale.y, 3f); // Cap growth
                spikeGroup.transform.localScale = scale;
            }
        }

        /// <summary>
        /// Animates the meme-themed platforms.
        /// </summary>
        private void AnimateMemePlatforms()
        {
            // Solo Penguin platform moves back and forth
            if (soloPenguinPlatform != null)
            {
                float offset = Mathf.Sin(Time.time * penguinMoveSpeed) * penguinMoveRange;
                soloPenguinPlatform.transform.position = penguinStartPos + new Vector3(offset, 0, 0);
            }

            // Punch Monkey spins and moves erratically
            if (punchMonkeyHazard != null)
            {
                punchMonkeyHazard.transform.Rotate(Vector3.up, 90f * Time.deltaTime);
                float yOffset = Mathf.Sin(Time.time * 3f) * 0.5f;
                Vector3 pos = punchMonkeyHazard.transform.position;
                pos.y += yOffset * Time.deltaTime;
                punchMonkeyHazard.transform.position = pos;
            }
        }

        /// <summary>
        /// Handles player death with increasing sarcasm.
        /// </summary>
        public override void OnPlayerDeath()
        {
            deathsThisLevel++;
            lastDeathTime = Time.time;

            base.OnPlayerDeath();

            UpdateDeathDisplay();

            // Respawn at start
            if (playerTransform != null && startPoint != null)
            {
                playerTransform.position = startPoint.position;

                Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }

            // Narrator commentary escalates
            ShowNarratorComment();
        }

        /// <summary>
        /// Updates the big death counter display.
        /// </summary>
        private void UpdateDeathDisplay()
        {
            if (bigDeathCountText != null)
            {
                bigDeathCountText.text = deathsThisLevel.ToString();

                // Death count gets bigger with more deaths (visual joke)
                float fontSize = 48f + (deathsThisLevel * 2f);
                bigDeathCountText.fontSize = Mathf.Min((int)fontSize, 200);
            }
        }

        /// <summary>
        /// Updates the timer display.
        /// </summary>
        private void UpdateTimerDisplay()
        {
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(levelTimer / 60f);
                int seconds = Mathf.FloorToInt(levelTimer % 60f);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }
        }

        /// <summary>
        /// Shows an increasingly sarcastic narrator comment based on death count.
        /// </summary>
        private void ShowNarratorComment()
        {
            if (narratorComments == null || narratorComments.Length == 0) return;

            int index;
            if (deathsThisLevel <= narratorComments.Length)
            {
                index = deathsThisLevel - 1;
            }
            else
            {
                // Cycle through the last few comments
                index = narratorComments.Length - 1 - (deathsThisLevel % 5);
                index = Mathf.Max(0, index);
            }

            ShowNarratorText(narratorComments[index]);
        }

        /// <summary>
        /// Shows narrator text.
        /// </summary>
        private void ShowNarratorText(string text)
        {
            if (narratorText != null) narratorText.text = text;
        }

        /// <summary>
        /// Level complete sequence with death count summary.
        /// </summary>
        private IEnumerator LevelCompleteSequence()
        {
            if (levelComplete) yield break;
            levelComplete = true;

            // Dramatic pause
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopMusic(0.5f);
            }

            yield return new WaitForSeconds(1f);

            ShowNarratorText($"You... actually did it.");

            yield return new WaitForSeconds(2f);

            int minutes = Mathf.FloorToInt(levelTimer / 60f);
            int seconds = Mathf.FloorToInt(levelTimer % 60f);

            ShowNarratorText($"Deaths: {deathsThisLevel}\nTime: {minutes}:{seconds:00}\n\n" +
                $"That's... that's a lot of deaths, {(gameManager != null ? gameManager.PlayerName : "Player")}.");

            yield return new WaitForSeconds(3f);

            if (deathsThisLevel < 5)
            {
                ShowNarratorText("Wait, only " + deathsThisLevel + " deaths? Are you a speedrunner?!");
            }
            else if (deathsThisLevel < 20)
            {
                ShowNarratorText("Not bad. Not good either. But not bad.");
            }
            else if (deathsThisLevel < 50)
            {
                ShowNarratorText("The Solo Penguin is disappointed in you. " + deathsThisLevel + " deaths? Really?");
            }
            else
            {
                ShowNarratorText(deathsThisLevel + " deaths. We should put that on the Steam store page as a warning.");
            }

            yield return new WaitForSeconds(3f);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayVictoryFanfare();
            }

            OnLevelComplete();
        }

        /// <summary>
        /// Builds the narrator commentary array with escalating sarcasm.
        /// </summary>
        private void BuildNarratorComments()
        {
            narratorComments = new string[]
            {
                "Death #1. Everyone gets one free death. That was yours.",
                "Death #2. Okay, still warming up.",
                "Death #3. The game isn't even trying yet.",
                "Death #4. Are you playing with your eyes closed?",
                "Death #5. Milestone! You've died more times than you've blinked.",
                "Death #6. The Punch Monkey is laughing at you.",
                "Death #7. Seven is supposed to be lucky. Not for you.",
                "Death #8. I've seen potatoes with better reflexes.",
                "Death #9. At this point, dying IS your gameplay.",
                "Death #10. DOUBLE DIGITS! Should we celebrate?",
                "Death #11. The Solo Penguin completed this in 0 deaths. Just saying.",
                "Death #12. Your keyboard is begging for mercy.",
                "Death #13. Unlucky 13. Or just regular unlucky. Like you.",
                "Death #14. I'm running out of sarcastic comments.",
                "Death #15. No wait, I have plenty more.",
                "Death #16. The platforms feel sorry for you at this point.",
                "Death #17. Have you tried... being better?",
                "Death #18. The spikes are filing a complaint about overwork.",
                "Death #19. One more and it's 20. Let that sink in.",
                "Death #20. I'm genuinely impressed by your persistence. And concerned.",
                "Okay I stopped counting but you're STILL dying.",
                "Are you doing this on purpose now?",
                "I've seen entire speedruns shorter than your death count.",
                "At this rate, the 'completion time' stat is going to overflow.",
                "The game files are getting heavy with all these death records.",
                "Fine. I respect the grind. Keep dying, champion.",
                "You know what? I believe in you. (I'm lying. I don't.)",
                "...",
                "Seriously though, the exit is RIGHT THERE.",
                "I'm going to take a nap. Wake me when you're done dying."
            };
        }

        protected override void OnDeathTroll()
        {
            // Death troll is handled by ShowNarratorComment
        }
    }
}
