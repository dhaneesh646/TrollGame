using System;
using System.Collections;
using UnityEngine;

namespace TrollGame.FourthWall
{
    /// <summary>
    /// Handles game window manipulation for 4th-wall breaking effects.
    /// Uses platform-specific APIs to move, resize, and shake the game window.
    /// 
    /// SAFETY NOTES:
    /// - Only manipulates the game's OWN window (not other applications)
    /// - Uses standard Win32 API calls (SetWindowPos, MoveWindow)
    /// - These are the same APIs used by OneShot and other approved Steam games
    /// - Zero antivirus risk — these are normal window management operations
    /// 
    /// IMPORTANT: Only works in Standalone (Windows) builds, not in Editor.
    /// </summary>
    public class WindowManipulator : MonoBehaviour
    {
        public static WindowManipulator Instance { get; private set; }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        private IntPtr windowHandle;
        private RECT originalRect;
        private bool hasOriginalRect = false;
#endif

        [Header("Window Settings")]
        [SerializeField] private float shakeIntensity = 20f;
        [SerializeField] private float shakeDuration = 1f;

        private bool isShaking = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            windowHandle = GetActiveWindow();
            SaveOriginalPosition();
#endif
        }

        /// <summary>
        /// Saves the original window position for restoration later.
        /// </summary>
        private void SaveOriginalPosition()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (windowHandle != IntPtr.Zero)
            {
                GetWindowRect(windowHandle, out originalRect);
                hasOriginalRect = true;
            }
#endif
        }

        /// <summary>
        /// Shakes the game window on the desktop (like the window is having a seizure).
        /// Guaranteed streamer reaction.
        /// </summary>
        public void ShakeWindow(float duration = -1f, float intensity = -1f)
        {
            if (isShaking) return;

            float dur = duration > 0 ? duration : shakeDuration;
            float inten = intensity > 0 ? intensity : shakeIntensity;

            StartCoroutine(WindowShakeRoutine(dur, inten));
        }

        private IEnumerator WindowShakeRoutine(float duration, float intensity)
        {
            isShaking = true;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (windowHandle == IntPtr.Zero || !hasOriginalRect)
            {
                isShaking = false;
                yield break;
            }

            int origX = originalRect.Left;
            int origY = originalRect.Top;
            int width = originalRect.Right - originalRect.Left;
            int height = originalRect.Bottom - originalRect.Top;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                int offsetX = (int)(UnityEngine.Random.Range(-intensity, intensity));
                int offsetY = (int)(UnityEngine.Random.Range(-intensity, intensity));

                MoveWindow(windowHandle, origX + offsetX, origY + offsetY, width, height, true);

                yield return null;
            }

            // Restore original position
            MoveWindow(windowHandle, origX, origY, width, height, true);
#else
            // In editor, just wait out the duration
            yield return new WaitForSeconds(duration);
#endif

            isShaking = false;
        }

        /// <summary>
        /// Moves the game window to a specific screen position.
        /// </summary>
        public void MoveWindowTo(int x, int y)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (windowHandle == IntPtr.Zero || !hasOriginalRect) return;

            int width = originalRect.Right - originalRect.Left;
            int height = originalRect.Bottom - originalRect.Top;

            MoveWindow(windowHandle, x, y, width, height, true);
#endif
        }

        /// <summary>
        /// Makes the window "run away" from its current position.
        /// The window slides to a random position on screen.
        /// </summary>
        public void WindowRunAway()
        {
            StartCoroutine(WindowRunAwayRoutine());
        }

        private IEnumerator WindowRunAwayRoutine()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (windowHandle == IntPtr.Zero || !hasOriginalRect) yield break;

            int screenWidth = GetSystemMetrics(0); // SM_CXSCREEN
            int screenHeight = GetSystemMetrics(1); // SM_CYSCREEN
            int width = originalRect.Right - originalRect.Left;
            int height = originalRect.Bottom - originalRect.Top;

            RECT currentRect;
            GetWindowRect(windowHandle, out currentRect);

            int targetX = UnityEngine.Random.Range(0, screenWidth - width);
            int targetY = UnityEngine.Random.Range(0, screenHeight - height);

            // Smooth slide to new position
            float duration = 0.5f;
            float elapsed = 0f;
            int startX = currentRect.Left;
            int startY = currentRect.Top;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                t = t * t * (3f - 2f * t); // Smoothstep

                int newX = (int)Mathf.Lerp(startX, targetX, t);
                int newY = (int)Mathf.Lerp(startY, targetY, t);

                MoveWindow(windowHandle, newX, newY, width, height, true);
                yield return null;
            }
#else
            yield return null;
#endif
        }

        /// <summary>
        /// Makes the window shrink down to nothing (fake "closing").
        /// </summary>
        public void FakeCloseWindow()
        {
            StartCoroutine(FakeCloseRoutine());
        }

        private IEnumerator FakeCloseRoutine()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (windowHandle == IntPtr.Zero || !hasOriginalRect) yield break;

            int origWidth = originalRect.Right - originalRect.Left;
            int origHeight = originalRect.Bottom - originalRect.Top;
            int centerX = originalRect.Left + origWidth / 2;
            int centerY = originalRect.Top + origHeight / 2;

            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;

                int newWidth = (int)Mathf.Lerp(origWidth, 0, t);
                int newHeight = (int)Mathf.Lerp(origHeight, 0, t);
                int newX = centerX - newWidth / 2;
                int newY = centerY - newHeight / 2;

                MoveWindow(windowHandle, newX, newY, Mathf.Max(newWidth, 1), Mathf.Max(newHeight, 1), true);
                yield return null;
            }

            // Brief pause (looks like it closed)
            yield return new WaitForSecondsRealtime(2f);

            // SURPRISE! Restore to full size
            MoveWindow(windowHandle, originalRect.Left, originalRect.Top, origWidth, origHeight, true);
#else
            yield return null;
#endif
        }

        /// <summary>
        /// Makes the window slowly drift across the screen.
        /// </summary>
        public void DriftWindow(float speed = 50f, float duration = 10f)
        {
            StartCoroutine(DriftRoutine(speed, duration));
        }

        private IEnumerator DriftRoutine(float speed, float duration)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (windowHandle == IntPtr.Zero) yield break;

            float elapsed = 0f;
            Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                RECT currentRect;
                GetWindowRect(windowHandle, out currentRect);

                int width = currentRect.Right - currentRect.Left;
                int height = currentRect.Bottom - currentRect.Top;

                int newX = currentRect.Left + (int)(direction.x * speed * Time.unscaledDeltaTime);
                int newY = currentRect.Top + (int)(direction.y * speed * Time.unscaledDeltaTime);

                // Bounce off screen edges
                int screenWidth = GetSystemMetrics(0);
                int screenHeight = GetSystemMetrics(1);

                if (newX < 0 || newX + width > screenWidth) direction.x *= -1;
                if (newY < 0 || newY + height > screenHeight) direction.y *= -1;

                MoveWindow(windowHandle, newX, newY, width, height, true);
                yield return null;
            }

            // Return to original position
            RestoreWindowPosition();
#else
            yield return new WaitForSeconds(duration);
#endif
        }

        /// <summary>
        /// Restores the window to its original position.
        /// </summary>
        public void RestoreWindowPosition()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (windowHandle != IntPtr.Zero && hasOriginalRect)
            {
                int width = originalRect.Right - originalRect.Left;
                int height = originalRect.Bottom - originalRect.Top;
                MoveWindow(windowHandle, originalRect.Left, originalRect.Top, width, height, true);
            }
#endif
        }
    }
}
