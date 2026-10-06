using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Pauses and continues a run.
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
