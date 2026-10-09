using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Opens and closes the settings, from the main menu or from the pause menu.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        [SerializeField] private ScreenManager m_screens;

        public void Open()
        {
            m_screens.ShowSettings();
        }

        /// <summary>
        /// Goes back to where the settings were opened from: the pause menu during
        /// a run, the main menu otherwise.
        /// </summary>
        public void Close()
        {
            if (PauseMenu.IsPaused)
            {
                m_screens.ShowPauseMenu();
            }
            else
            {
                m_screens.ShowMainMenu();
            }
        }
    }
}
