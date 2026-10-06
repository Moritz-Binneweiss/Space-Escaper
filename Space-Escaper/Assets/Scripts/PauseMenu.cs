using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Pauses and continues a run. Also pauses on its own when the app loses focus
    /// or goes to the background.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        private const string k_ShowTrigger = "Show";
        private const string k_HideTrigger = "Hide";

        [SerializeField] private Animator m_pauseMenuAnimator;
        [SerializeField] private Animator m_gameMenuAnimator;

        /// <summary>
        /// The one pause state of the game. SettingsManager reads it to decide where
        /// "Back" leads.
        /// </summary>
        public static bool IsPaused { get; private set; }

        private void Awake()
        {
            // Static, so it would otherwise survive the scene reload behind "Exit"
            // and stay stuck on true.
            IsPaused = false;
        }

        // A run must not go on while the player is away from the app, and must not
        // start again by itself when they come back. Android reports a pulled-down
        // notification shade only as lost focus, leaving the app as both. In the
        // Editor, clicking any window other than the Game view counts as lost focus.
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                Pause();
            }
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                Pause();
            }
        }

        /// <summary>
        /// Safe to call from anywhere, not only from the pause button: it only pauses
        /// a run that is going on with the ship alive, and only once.
        /// </summary>
        public void Pause()
        {
            if (IsPaused || !GameManager.Instance.IsRunActive)
            {
                return;
            }

            IsPaused = true;
            AudioSystem.Instance.StopEngine();
            m_gameMenuAnimator.SetTrigger(k_HideTrigger);
            m_pauseMenuAnimator.SetTrigger(k_ShowTrigger);
            Time.timeScale = 0f;
        }

        public void Continue()
        {
            if (!IsPaused)
            {
                return;
            }

            IsPaused = false;
            AudioSystem.Instance.StartEngine();
            m_gameMenuAnimator.SetTrigger(k_ShowTrigger);
            m_pauseMenuAnimator.SetTrigger(k_HideTrigger);
            Time.timeScale = 1f;
        }
    }
}
