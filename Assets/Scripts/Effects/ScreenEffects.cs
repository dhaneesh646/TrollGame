using System.Collections;
using UnityEngine;

namespace TrollGame.Effects
{
    /// <summary>
    /// Collection of screen-space visual effects for trolling.
    /// Includes screen shake, rotation, zoom, color grading changes,
    /// and various distortion effects — all through camera manipulation.
    /// </summary>
    public class ScreenEffects : MonoBehaviour
    {
        public static ScreenEffects Instance { get; private set; }

        [Header("Camera Reference")]
        [SerializeField] private Camera targetCamera;

        [Header("Shake Settings")]
        [SerializeField] private float defaultShakeIntensity = 0.3f;
        [SerializeField] private float defaultShakeDuration = 0.5f;

        private Vector3 originalCameraPos;
        private Quaternion originalCameraRot;
        private float originalFOV;
        private bool isShaking = false;
        private bool isRotating = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera != null)
            {
                originalCameraPos = targetCamera.transform.localPosition;
                originalCameraRot = targetCamera.transform.localRotation;
                originalFOV = targetCamera.fieldOfView;
            }
        }

        // ==========================================
        // SCREEN SHAKE
        // ==========================================

        /// <summary>
        /// Shakes the camera with default settings.
        /// </summary>
        public void Shake()
        {
            Shake(defaultShakeDuration, defaultShakeIntensity);
        }

        /// <summary>
        /// Shakes the camera with custom duration and intensity.
        /// </summary>
        public void Shake(float duration, float intensity)
        {
            if (!isShaking)
            {
                StartCoroutine(ShakeRoutine(duration, intensity));
            }
        }

        private IEnumerator ShakeRoutine(float duration, float intensity)
        {
            isShaking = true;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float dampening = 1f - (elapsed / duration);

                Vector3 offset = new Vector3(
                    Random.Range(-intensity, intensity) * dampening,
                    Random.Range(-intensity, intensity) * dampening,
                    0
                );

                if (targetCamera != null)
                {
                    targetCamera.transform.localPosition = originalCameraPos + offset;
                }

                yield return null;
            }

            if (targetCamera != null)
            {
                targetCamera.transform.localPosition = originalCameraPos;
            }

            isShaking = false;
        }

        // ==========================================
        // SCREEN ROTATION (Disorientation)
        // ==========================================

        /// <summary>
        /// Gradually rotates the screen to disorient the player.
        /// </summary>
        public void RotateScreen(float targetAngle, float duration)
        {
            if (!isRotating)
            {
                StartCoroutine(RotateRoutine(targetAngle, duration));
            }
        }

        private IEnumerator RotateRoutine(float targetAngle, float duration)
        {
            isRotating = true;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float angle = Mathf.Lerp(0, targetAngle, t);

                if (targetCamera != null)
                {
                    targetCamera.transform.localRotation = originalCameraRot * Quaternion.Euler(0, 0, angle);
                }

                yield return null;
            }

            isRotating = false;
        }

        /// <summary>
        /// Resets camera rotation to normal.
        /// </summary>
        public void ResetRotation(float duration = 0.5f)
        {
            StartCoroutine(ResetRotationRoutine(duration));
        }

        private IEnumerator ResetRotationRoutine(float duration)
        {
            if (targetCamera == null) yield break;

            Quaternion currentRot = targetCamera.transform.localRotation;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                targetCamera.transform.localRotation = Quaternion.Lerp(currentRot, originalCameraRot, t);
                yield return null;
            }

            targetCamera.transform.localRotation = originalCameraRot;
        }

        // ==========================================
        // FOV MANIPULATION
        // ==========================================

        /// <summary>
        /// Smoothly changes the FOV (zooms in/out) for dramatic effect.
        /// </summary>
        public void ChangeFOV(float targetFOV, float duration)
        {
            StartCoroutine(FOVRoutine(targetFOV, duration));
        }

        private IEnumerator FOVRoutine(float targetFOV, float duration)
        {
            if (targetCamera == null) yield break;

            float startFOV = targetCamera.fieldOfView;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                targetCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
                yield return null;
            }

            targetCamera.fieldOfView = targetFOV;
        }

        /// <summary>
        /// Resets FOV to original value.
        /// </summary>
        public void ResetFOV(float duration = 0.5f)
        {
            ChangeFOV(originalFOV, duration);
        }

        /// <summary>
        /// Creates a dramatic zoom-in-then-out effect.
        /// </summary>
        public void PunchZoom(float zoomFOV = 30f, float duration = 0.3f)
        {
            StartCoroutine(PunchZoomRoutine(zoomFOV, duration));
        }

        private IEnumerator PunchZoomRoutine(float zoomFOV, float duration)
        {
            // Zoom in
            yield return StartCoroutine(FOVRoutine(zoomFOV, duration * 0.3f));
            // Zoom back
            yield return StartCoroutine(FOVRoutine(originalFOV, duration * 0.7f));
        }

        // ==========================================
        // SPECIAL TROLL EFFECTS
        // ==========================================

        /// <summary>
        /// Makes the screen slowly tilt like the whole world is off-balance.
        /// Subtle enough that the player might not notice at first.
        /// </summary>
        public void SubtleTilt(float maxAngle = 5f, float period = 10f, float duration = 30f)
        {
            StartCoroutine(SubtleTiltRoutine(maxAngle, period, duration));
        }

        private IEnumerator SubtleTiltRoutine(float maxAngle, float period, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float angle = Mathf.Sin(elapsed / period * Mathf.PI * 2f) * maxAngle;

                if (targetCamera != null)
                {
                    targetCamera.transform.localRotation = originalCameraRot * Quaternion.Euler(0, 0, angle);
                }

                yield return null;
            }

            if (targetCamera != null)
            {
                targetCamera.transform.localRotation = originalCameraRot;
            }
        }

        /// <summary>
        /// Flips the screen upside down.
        /// </summary>
        public void FlipScreen(float duration = 1f)
        {
            RotateScreen(180f, duration);
        }

        /// <summary>
        /// Creates a "drunk" camera effect — wobble + slight blur feeling.
        /// </summary>
        public void DrunkEffect(float duration = 10f, float intensity = 1f)
        {
            StartCoroutine(DrunkRoutine(duration, intensity));
        }

        private IEnumerator DrunkRoutine(float duration, float intensity)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float dampening = 1f - (elapsed / duration);

                if (targetCamera != null)
                {
                    // Wobbly position
                    float wobbleX = Mathf.Sin(elapsed * 2f) * 0.1f * intensity * dampening;
                    float wobbleY = Mathf.Cos(elapsed * 1.5f) * 0.05f * intensity * dampening;
                    targetCamera.transform.localPosition = originalCameraPos +
                        new Vector3(wobbleX, wobbleY, 0);

                    // Wobbly rotation
                    float rotZ = Mathf.Sin(elapsed * 0.8f) * 3f * intensity * dampening;
                    targetCamera.transform.localRotation = originalCameraRot *
                        Quaternion.Euler(0, 0, rotZ);

                    // Wobbly FOV
                    float fovOffset = Mathf.Sin(elapsed * 3f) * 5f * intensity * dampening;
                    targetCamera.fieldOfView = originalFOV + fovOffset;
                }

                yield return null;
            }

            ResetAll();
        }

        /// <summary>
        /// Resets all camera properties to their original values.
        /// </summary>
        public void ResetAll()
        {
            if (targetCamera != null)
            {
                targetCamera.transform.localPosition = originalCameraPos;
                targetCamera.transform.localRotation = originalCameraRot;
                targetCamera.fieldOfView = originalFOV;
            }

            isShaking = false;
            isRotating = false;
        }
    }
}
