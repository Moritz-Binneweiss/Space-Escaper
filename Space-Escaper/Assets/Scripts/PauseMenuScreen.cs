using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The pause menu: continues the run, opens the settings or goes back to the main menu.
    /// </summary>
    public class PauseMenuScreen : MonoBehaviour
    {
        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private ScreenManager m_screens;
        [SerializeField] private Button m_continueButton;
        [SerializeField] private Button m_settingsButton;
        [SerializeField] private Button m_exitButton;

        // For tests, which tap it when a focus change in the editor pauses the run.
        internal Button ContinueButton => m_continueButton;

        private void OnEnable()
        {
            m_continueButton.onClick.AddListener(m_gameManager.Continue);
            m_settingsButton.onClick.AddListener(m_screens.ShowSettings);
            m_exitButton.onClick.AddListener(m_gameManager.ReturnToMenu);
        }

        private void OnDisable()
        {
            m_continueButton.onClick.RemoveListener(m_gameManager.Continue);
            m_settingsButton.onClick.RemoveListener(m_screens.ShowSettings);
            m_exitButton.onClick.RemoveListener(m_gameManager.ReturnToMenu);
        }
    }
}
