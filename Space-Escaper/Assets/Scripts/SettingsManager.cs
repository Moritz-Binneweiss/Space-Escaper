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
        /// Goes back to where the settings were opened from. They leave the state of the game as
        /// it is, so that is the screen of the state: the pause menu during a run, the main menu
        /// otherwise.
        /// </summary>
        public void Close()
        {
            m_screens.Show(GameManager.Instance.State);
        }
    }
}
