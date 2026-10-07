using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Opens and closes the settings, from the main menu or from the pause menu.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        private const string k_ShowTrigger = "Show";
        private const string k_HideTrigger = "Hide";

        [SerializeField] private Animator m_pauseMenuAnimator;
        [SerializeField] private Animator m_settingsAnimator;
        [SerializeField] private Animator m_mainMenuAnimator;

        public void OpenFromMainMenu()
        {
            m_settingsAnimator.SetTrigger(k_ShowTrigger);
            m_mainMenuAnimator.SetTrigger(k_HideTrigger);
        }

        public void OpenFromPauseMenu()
        {
            m_settingsAnimator.SetTrigger(k_ShowTrigger);
            m_pauseMenuAnimator.SetTrigger(k_HideTrigger);
        }

        /// <summary>
        /// Goes back to where the settings were opened from: the pause menu during
        /// a run, the main menu otherwise.
        /// </summary>
        public void Close()
        {
            if (PauseMenu.IsPaused)
            {
                m_pauseMenuAnimator.SetTrigger(k_ShowTrigger);
                m_settingsAnimator.SetTrigger(k_HideTrigger);
            }
            else
            {
                m_settingsAnimator.SetTrigger(k_HideTrigger);
                m_mainMenuAnimator.SetTrigger(k_ShowTrigger);
            }
        }
    }
}
