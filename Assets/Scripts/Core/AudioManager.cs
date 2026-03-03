using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TrollGame.Core
{
    /// <summary>
    /// Manages all audio for the game including troll sound effects,
    /// jumpscare sounds, fake system sounds, and dynamic music.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private AudioSource trollSource;

        [Header("System Sound Effects")]
        [SerializeField] private AudioClip windowsErrorSound;
        [SerializeField] private AudioClip bsodSound;
        [SerializeField] private AudioClip startupSound;
        [SerializeField] private AudioClip shutdownSound;
        [SerializeField] private AudioClip notificationSound;
        [SerializeField] private AudioClip criticalErrorSound;

        [Header("Troll Sound Effects")]
        [SerializeField] private AudioClip jumpscareSound;
        [SerializeField] private AudioClip trollLaughSound;
        [SerializeField] private AudioClip airHornSound;
        [SerializeField] private AudioClip sadTromboneSound;
        [SerializeField] private AudioClip victoryFanfareSound;
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private AudioClip rageSound;

        [Header("Music Tracks")]
        [SerializeField] private AudioClip normalMusic;
        [SerializeField] private AudioClip trollMusic;
        [SerializeField] private AudioClip chaseMusic;
        [SerializeField] private AudioClip creepyMusic;
        [SerializeField] private AudioClip victoryMusic;

        [Header("Settings")]
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private float sfxVolume = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ==========================================
        // MUSIC
        // ==========================================

        /// <summary>
        /// Plays background music with optional crossfade.
        /// </summary>
        public void PlayMusic(AudioClip clip, bool loop = true, float fadeTime = 1f)
        {
            if (musicSource == null || clip == null) return;
            StartCoroutine(CrossfadeMusic(clip, loop, fadeTime));
        }

        private IEnumerator CrossfadeMusic(AudioClip newClip, bool loop, float fadeTime)
        {
            // Fade out current
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);
                yield return null;
            }

            musicSource.clip = newClip;
            musicSource.loop = loop;
            musicSource.Play();

            // Fade in new
            elapsed = 0f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, musicVolume * masterVolume, elapsed / fadeTime);
                yield return null;
            }
        }

        /// <summary>
        /// Stops music with fade out.
        /// </summary>
        public void StopMusic(float fadeTime = 1f)
        {
            if (musicSource == null) return;
            StartCoroutine(FadeOutMusic(fadeTime));
        }

        private IEnumerator FadeOutMusic(float fadeTime)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);
                yield return null;
            }

            musicSource.Stop();
        }

        /// <summary>
        /// Switches to troll music (for when things get chaotic).
        /// </summary>
        public void SwitchToTrollMusic()
        {
            PlayMusic(trollMusic);
        }

        /// <summary>
        /// Switches to creepy/horror music (for fake virus scares etc).
        /// </summary>
        public void SwitchToCreepyMusic()
        {
            PlayMusic(creepyMusic);
        }

        // ==========================================
        // SOUND EFFECTS
        // ==========================================

        /// <summary>
        /// Plays a one-shot sound effect.
        /// </summary>
        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (sfxSource == null || clip == null) return;
            sfxSource.PlayOneShot(clip, volumeScale * sfxVolume * masterVolume);
        }

        /// <summary>
        /// Plays a troll sound effect on the dedicated troll audio source.
        /// </summary>
        public void PlayTrollSFX(AudioClip clip, float volumeScale = 1f)
        {
            if (trollSource == null || clip == null) return;
            trollSource.PlayOneShot(clip, volumeScale * sfxVolume * masterVolume);
        }

        // Convenience methods for common troll sounds
        public void PlayWindowsError() => PlayTrollSFX(windowsErrorSound);
        public void PlayBSODSound() => PlayTrollSFX(bsodSound);
        public void PlayStartupSound() => PlayTrollSFX(startupSound);
        public void PlayNotification() => PlayTrollSFX(notificationSound);
        public void PlayJumpscare() => PlayTrollSFX(jumpscareSound, 1.5f);
        public void PlayTrollLaugh() => PlayTrollSFX(trollLaughSound);
        public void PlayAirHorn() => PlayTrollSFX(airHornSound, 1.3f);
        public void PlaySadTrombone() => PlayTrollSFX(sadTromboneSound);
        public void PlayVictoryFanfare() => PlayTrollSFX(victoryFanfareSound);
        public void PlayDeathSound() => PlaySFX(deathSound);
        public void PlayCriticalError() => PlayTrollSFX(criticalErrorSound, 1.5f);

        // ==========================================
        // TROLL AUDIO EFFECTS
        // ==========================================

        /// <summary>
        /// Gradually distorts the music pitch to create unease.
        /// </summary>
        public void DistortMusic(float targetPitch, float duration)
        {
            StartCoroutine(DistortMusicRoutine(targetPitch, duration));
        }

        private IEnumerator DistortMusicRoutine(float targetPitch, float duration)
        {
            if (musicSource == null) yield break;

            float startPitch = musicSource.pitch;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                musicSource.pitch = Mathf.Lerp(startPitch, targetPitch, elapsed / duration);
                yield return null;
            }
        }

        /// <summary>
        /// Resets music pitch to normal.
        /// </summary>
        public void ResetMusicPitch(float duration = 0.5f)
        {
            DistortMusic(1f, duration);
        }

        /// <summary>
        /// Creates a "vinyl scratch" effect (sudden music stop).
        /// </summary>
        public void VinylScratch()
        {
            if (musicSource == null) return;
            StartCoroutine(VinylScratchRoutine());
        }

        private IEnumerator VinylScratchRoutine()
        {
            float originalPitch = musicSource.pitch;

            // Rapid pitch down
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                musicSource.pitch = Mathf.Lerp(originalPitch, 0f, elapsed / 0.3f);
                yield return null;
            }

            musicSource.Pause();
            musicSource.pitch = originalPitch;
        }

        /// <summary>
        /// Plays a random troll sound (for unpredictable chaos).
        /// </summary>
        public void PlayRandomTrollSound()
        {
            AudioClip[] trollSounds = new AudioClip[]
            {
                jumpscareSound, trollLaughSound, airHornSound,
                sadTromboneSound, windowsErrorSound, criticalErrorSound
            };

            List<AudioClip> validSounds = new List<AudioClip>();
            foreach (AudioClip clip in trollSounds)
            {
                if (clip != null) validSounds.Add(clip);
            }

            if (validSounds.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, validSounds.Count);
                PlayTrollSFX(validSounds[randomIndex]);
            }
        }

        /// <summary>
        /// Sets master volume.
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
        }
    }
}
