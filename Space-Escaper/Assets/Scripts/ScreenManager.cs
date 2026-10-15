using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Shows one screen of the UI at a time and switches the others off: the screen of the state
    /// the game is in, or the settings over it.
    /// </summary>
    public class ScreenManager : MonoBehaviour
    {
        [SerializeField] private GameObject m_mainMenu;
        [SerializeField] private GameObject m_gameMenu;
        [SerializeField] private GameObject m_pauseMenu;
        [SerializeField] private GameObject m_settings;
        [SerializeField] private GameObject m_shop;
        [SerializeField] private GameObject m_deathMenu;

        private GameObject[] m_screens;

        private void Awake()
        {
            m_screens = new[] { m_mainMenu, m_gameMenu, m_pauseMenu, m_settings, m_shop, m_deathMenu };

            // The game starts in the main menu, whichever screen was left switched on
            // while editing the scene.
            Show(GameState.Menu);
        }

        public void Show(GameState state)
        {
            Show(GetScreen(state));
        }

        public void ShowSettings()
        {
            Show(m_settings);
        }

        private GameObject GetScreen(GameState state)
        {
            switch (state)
            {
                case GameState.Hangar:
                    return m_shop;

                case GameState.Run:
                    return m_gameMenu;

                case GameState.Paused:
                    return m_pauseMenu;

                case GameState.Dead:
                    return m_deathMenu;

                default:
                    return m_mainMenu;
            }
        }

        private void Show(GameObject screen)
        {
            foreach (GameObject candidate in m_screens)
            {
                candidate.SetActive(candidate == screen);
            }
        }
    }
}
