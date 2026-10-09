using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Shows one screen of the UI at a time and switches the others off.
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
            ShowMainMenu();
        }

        public void ShowMainMenu()
        {
            Show(m_mainMenu);
        }

        public void ShowGameMenu()
        {
            Show(m_gameMenu);
        }

        public void ShowPauseMenu()
        {
            Show(m_pauseMenu);
        }

        public void ShowSettings()
        {
            Show(m_settings);
        }

        public void ShowShop()
        {
            Show(m_shop);
        }

        public void ShowDeathMenu()
        {
            Show(m_deathMenu);
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
