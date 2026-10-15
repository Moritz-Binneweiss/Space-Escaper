using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The main menu: starts a run and opens the hangar or the settings.
    /// </summary>
    public class MainMenuScreen : MonoBehaviour
    {
        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private ScreenManager m_screens;
        [SerializeField] private Button m_startButton;
        [SerializeField] private Button m_hangarButton;
        [SerializeField] private Button m_settingsButton;

        // For tests, which tap it like a player.
        internal Button StartButton => m_startButton;

        private void OnEnable()
        {
            m_startButton.onClick.AddListener(m_gameManager.Play);
            m_hangarButton.onClick.AddListener(m_gameManager.OpenHangar);
            m_settingsButton.onClick.AddListener(m_screens.ShowSettings);
        }

        private void OnDisable()
        {
            m_startButton.onClick.RemoveListener(m_gameManager.Play);
            m_hangarButton.onClick.RemoveListener(m_gameManager.OpenHangar);
            m_settingsButton.onClick.RemoveListener(m_screens.ShowSettings);
        }
    }
}
