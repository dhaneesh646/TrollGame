using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TrollGame.Core;

namespace TrollGame.Memes
{
    /// <summary>
    /// Central meme content manager. Handles integration of real-world 2025-2026
    /// viral incidents and meme references throughout the game.
    /// 
    /// Memes are treated as modular content packs that can be injected into any level.
    /// Each meme has associated dialogue, visual references, and gameplay mechanics.
    /// 
    /// Current meme library (2025-2026):
    /// - Solo Penguin: The viral penguin walking alone — used as a moving platform
    /// - Punch Monkey: The rage-bait monkey — used as a hazard/enemy
    /// - Italian Brainrot: Aesthetic corruption — used as visual glitch effects
    /// - Skibidi: Audio distortion meme — used in audio trolling
    /// - Ohio Meme: "Only in Ohio" — used for surreal level elements
    /// </summary>
    public class MemeManager : MonoBehaviour
    {
        public static MemeManager Instance { get; private set; }

        [Header("Meme Configuration")]
        [SerializeField] private List<MemeData> memeLibrary = new List<MemeData>();
        [SerializeField] private float memeReferenceInterval = 30f;
        [SerializeField] private bool autoInjectMemes = true;

        [Header("Meme Prefabs")]
        [SerializeField] private GameObject soloPenguinPrefab;
        [SerializeField] private GameObject punchMonkeyPrefab;
        [SerializeField] private GameObject brainrotEffectPrefab;

        [Header("Meme Audio")]
        [SerializeField] private AudioClip soloPenguinTheme;
        [SerializeField] private AudioClip punchMonkeySound;
        [SerializeField] private AudioClip brainrotAudio;
        [SerializeField] private AudioClip vineBoomeSound;
        [SerializeField] private AudioClip ohioAmbience;

        private Queue<MemeReference> pendingMemeReferences = new Queue<MemeReference>();
        private float lastMemeTime = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (memeLibrary.Count == 0)
            {
                PopulateDefaultMemeLibrary();
            }
        }

        private void Update()
        {
            if (!autoInjectMemes) return;

            // Periodically inject meme references
            if (Time.time - lastMemeTime > memeReferenceInterval && pendingMemeReferences.Count > 0)
            {
                MemeReference memeRef = pendingMemeReferences.Dequeue();
                ExecuteMemeReference(memeRef);
                lastMemeTime = Time.time;
            }
        }

        /// <summary>
        /// Populates the default meme library with 2025-2026 viral content.
        /// </summary>
        private void PopulateDefaultMemeLibrary()
        {
            memeLibrary.Add(new MemeData
            {
                MemeName = "Solo Penguin",
                MemeType = MemeType.Character,
                Description = "The viral penguin who walks alone. A symbol of independence and loneliness.",
                DialogueLines = new string[]
                {
                    "A wild Solo Penguin appears! It walks... alone.",
                    "The Solo Penguin judges you silently.",
                    "Solo Penguin doesn't need friends. Solo Penguin IS the friend.",
                    "The penguin waddles away. You feel strangely emotional."
                },
                GameplayEffect = MemeGameplayEffect.MovingPlatform,
                TrollPotential = 3
            });

            memeLibrary.Add(new MemeData
            {
                MemeName = "Punch Monkey",
                MemeType = MemeType.Hazard,
                Description = "The rage-inducing monkey. Punches the player at the worst possible moments.",
                DialogueLines = new string[]
                {
                    "PUNCH MONKEY approaches! It looks angry!",
                    "The monkey has ONE move. Guess what it is.",
                    "You've been PUNCHED by the monkey! Classic.",
                    "Punch Monkey doesn't discriminate. Everyone gets punched."
                },
                GameplayEffect = MemeGameplayEffect.DamageHazard,
                TrollPotential = 8
            });

            memeLibrary.Add(new MemeData
            {
                MemeName = "Italian Brainrot",
                MemeType = MemeType.VisualEffect,
                Description = "The aesthetic corruption meme. Makes everything look 'brainrotted.'",
                DialogueLines = new string[]
                {
                    "The brainrot is spreading...",
                    "Your perception of reality is being... enhanced.",
                    "Is this art? Is this a meme? Is there a difference?",
                    "The brainrot level in this area is over 9000."
                },
                GameplayEffect = MemeGameplayEffect.VisualCorruption,
                TrollPotential = 6
            });

            memeLibrary.Add(new MemeData
            {
                MemeName = "Skibidi",
                MemeType = MemeType.AudioEffect,
                Description = "Audio distortion meme. Corrupts game audio in hilarious ways.",
                DialogueLines = new string[]
                {
                    "The audio is acting... skibidi.",
                    "Your ears are not ready for this.",
                    "The sound designer had a breakdown here.",
                    "This is what happens when memes infect the audio engine."
                },
                GameplayEffect = MemeGameplayEffect.AudioDistortion,
                TrollPotential = 5
            });

            memeLibrary.Add(new MemeData
            {
                MemeName = "Ohio",
                MemeType = MemeType.Environment,
                Description = "Only in Ohio. Surreal environmental elements.",
                DialogueLines = new string[]
                {
                    "You have entered Ohio. Nothing makes sense here.",
                    "Only in Ohio would this be a game mechanic.",
                    "The laws of physics are different in Ohio.",
                    "Welcome to Ohio. Population: confused."
                },
                GameplayEffect = MemeGameplayEffect.PhysicsModifier,
                TrollPotential = 7
            });
        }

        /// <summary>
        /// Queues a meme reference for injection into the current level.
        /// </summary>
        public void QueueMemeReference(string memeName, Vector3 position)
        {
            MemeData meme = memeLibrary.Find(m => m.MemeName == memeName);
            if (meme == null) return;

            MemeReference memeRef = new MemeReference
            {
                Meme = meme,
                SpawnPosition = position,
                TriggerTime = Time.time + UnityEngine.Random.Range(1f, 5f)
            };

            pendingMemeReferences.Enqueue(memeRef);
        }

        /// <summary>
        /// Immediately spawns a meme element at the specified position.
        /// </summary>
        public GameObject SpawnMemeElement(string memeName, Vector3 position)
        {
            MemeData meme = memeLibrary.Find(m => m.MemeName == memeName);
            if (meme == null) return null;

            GameObject prefab = GetPrefabForMeme(meme);
            if (prefab == null) return null;

            GameObject instance = Instantiate(prefab, position, Quaternion.identity);

            // Show meme dialogue
            if (meme.DialogueLines != null && meme.DialogueLines.Length > 0)
            {
                string dialogue = meme.DialogueLines[UnityEngine.Random.Range(0, meme.DialogueLines.Length)];
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowTrollDialogue(dialogue, 3f);
                }
            }

            return instance;
        }

        /// <summary>
        /// Triggers a random meme event.
        /// </summary>
        public void TriggerRandomMeme(Vector3 position)
        {
            if (memeLibrary.Count == 0) return;
            MemeData randomMeme = memeLibrary[UnityEngine.Random.Range(0, memeLibrary.Count)];

            ExecuteMemeEffect(randomMeme, position);
        }

        /// <summary>
        /// Executes a meme reference.
        /// </summary>
        private void ExecuteMemeReference(MemeReference memeRef)
        {
            ExecuteMemeEffect(memeRef.Meme, memeRef.SpawnPosition);
        }

        /// <summary>
        /// Executes the gameplay effect of a meme.
        /// </summary>
        private void ExecuteMemeEffect(MemeData meme, Vector3 position)
        {
            // Play associated sound
            PlayMemeSound(meme);

            // Spawn visual element
            GameObject prefab = GetPrefabForMeme(meme);
            if (prefab != null)
            {
                Instantiate(prefab, position, Quaternion.identity);
            }

            // Show dialogue
            if (meme.DialogueLines != null && meme.DialogueLines.Length > 0)
            {
                string dialogue = meme.DialogueLines[UnityEngine.Random.Range(0, meme.DialogueLines.Length)];
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowTrollDialogue(meme.MemeName, dialogue, 3f);
                }
            }

            // Apply gameplay effect
            switch (meme.GameplayEffect)
            {
                case MemeGameplayEffect.AudioDistortion:
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.DistortMusic(UnityEngine.Random.Range(0.5f, 1.5f), 3f);
                    }
                    break;

                case MemeGameplayEffect.VisualCorruption:
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowGlitchEffect(2f);
                    }
                    break;

                case MemeGameplayEffect.PhysicsModifier:
                    StartCoroutine(TemporaryPhysicsChange(5f));
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Temporarily modifies physics (Ohio meme effect).
        /// </summary>
        private IEnumerator TemporaryPhysicsChange(float duration)
        {
            Vector3 originalGravity = Physics.gravity;
            Physics.gravity = new Vector3(
                UnityEngine.Random.Range(-3f, 3f),
                UnityEngine.Random.Range(-15f, -3f),
                UnityEngine.Random.Range(-3f, 3f)
            );

            yield return new WaitForSeconds(duration);

            Physics.gravity = originalGravity;
        }

        /// <summary>
        /// Plays the sound associated with a meme.
        /// </summary>
        private void PlayMemeSound(MemeData meme)
        {
            if (AudioManager.Instance == null) return;

            AudioClip clip = null;
            switch (meme.MemeName)
            {
                case "Solo Penguin": clip = soloPenguinTheme; break;
                case "Punch Monkey": clip = punchMonkeySound; break;
                case "Italian Brainrot": clip = brainrotAudio; break;
                case "Skibidi": clip = vineBoomeSound; break;
                case "Ohio": clip = ohioAmbience; break;
            }

            if (clip != null)
            {
                AudioManager.Instance.PlaySFX(clip);
            }
        }

        /// <summary>
        /// Gets the spawn prefab for a meme type.
        /// </summary>
        private GameObject GetPrefabForMeme(MemeData meme)
        {
            switch (meme.MemeName)
            {
                case "Solo Penguin": return soloPenguinPrefab;
                case "Punch Monkey": return punchMonkeyPrefab;
                case "Italian Brainrot": return brainrotEffectPrefab;
                default: return null;
            }
        }

        /// <summary>
        /// Gets a random meme dialogue line for any meme.
        /// </summary>
        public string GetRandomMemeDialogue()
        {
            if (memeLibrary.Count == 0) return "No memes found. How sad.";

            MemeData randomMeme = memeLibrary[UnityEngine.Random.Range(0, memeLibrary.Count)];
            if (randomMeme.DialogueLines == null || randomMeme.DialogueLines.Length == 0)
                return $"{randomMeme.MemeName} has nothing to say.";

            return randomMeme.DialogueLines[UnityEngine.Random.Range(0, randomMeme.DialogueLines.Length)];
        }
    }

    /// <summary>
    /// Data container for a meme reference.
    /// </summary>
    [Serializable]
    public class MemeData
    {
        public string MemeName;
        public MemeType MemeType;
        public string Description;
        public string[] DialogueLines;
        public MemeGameplayEffect GameplayEffect;
        [Range(1, 10)]
        public int TrollPotential;
        public Sprite MemeSprite;
    }

    public enum MemeType
    {
        Character,
        Hazard,
        VisualEffect,
        AudioEffect,
        Environment,
        Dialogue
    }

    public enum MemeGameplayEffect
    {
        None,
        MovingPlatform,
        DamageHazard,
        SpeedBoost,
        SpeedSlow,
        GravityFlip,
        VisualCorruption,
        AudioDistortion,
        PhysicsModifier,
        ControlSwap
    }

    /// <summary>
    /// Represents a queued meme reference waiting to be triggered.
    /// </summary>
    public class MemeReference
    {
        public MemeData Meme;
        public Vector3 SpawnPosition;
        public float TriggerTime;
    }
}
