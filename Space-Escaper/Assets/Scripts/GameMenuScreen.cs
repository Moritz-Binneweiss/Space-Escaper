using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The screen during a run, with the pause button.
    /// </summary>
    public class GameMenuScreen : MonoBehaviour
    {
        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private Button m_pauseButton;

        // For tests, which check that the screen is up during the run.
        internal Button PauseButton => m_pauseButton;

        private void OnEnable()
        {
            m_pauseButton.onClick.AddListener(m_gameManager.Pause);
        }

        private void OnDisable()
        {
            m_pauseButton.onClick.RemoveListener(m_gameManager.Pause);
        }
    }
}
