using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The screen during a run: score, coins and score multiplier, and the pause button.
    /// </summary>
    public class GameMenuScreen : MonoBehaviour
    {
        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private RunScore m_runScore;
        [SerializeField] private Text m_scoreText;
        [SerializeField] private Text m_coinText;
        [SerializeField] private Text m_modifierText;
        [Tooltip("The coin icon, which pops for every coin collected.")]
        [SerializeField] private PopEffect m_coinIconPop;
        [SerializeField] private Button m_pauseButton;

        private int m_shownScore;
        private int m_shownCoins;
        private float m_shownModifier;

        // For tests, which check that the screen is up during the run.
        internal Button PauseButton => m_pauseButton;

        private void OnEnable()
        {
            m_pauseButton.onClick.AddListener(m_gameManager.Pause);
            ShowScore();
            ShowCoins();
            ShowModifier();
        }

        private void OnDisable()
        {
            m_pauseButton.onClick.RemoveListener(m_gameManager.Pause);
        }

        // After every Update, so the screen shows the score of this frame. A text only changes
        // with its number, which keeps new strings and canvas rebuilds out of the other frames.
        private void LateUpdate()
        {
            if (m_runScore.Score != m_shownScore)
            {
                ShowScore();
            }

            if (m_runScore.Coins != m_shownCoins)
            {
                ShowCoins();
                m_coinIconPop.Play();
            }

            if (m_runScore.Modifier != m_shownModifier)
            {
                ShowModifier();
            }
        }

        private void ShowScore()
        {
            m_shownScore = m_runScore.Score;
            m_scoreText.text = m_shownScore.ToString();
        }

        private void ShowCoins()
        {
            m_shownCoins = m_runScore.Coins;
            m_coinText.text = m_shownCoins.ToString();
        }

        private void ShowModifier()
        {
            m_shownModifier = m_runScore.Modifier;
            m_modifierText.text = "x" + m_shownModifier.ToString("0.0");
        }
    }
}
