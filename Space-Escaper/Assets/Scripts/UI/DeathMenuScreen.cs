using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The death screen: score, coins, time and distance of the run, whether it set a new highscore, the revive while
    /// the run still has it, and the ways to a new run, the hangar or the main menu.
    /// </summary>
    public class DeathMenuScreen : MonoBehaviour
    {
        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private RunScore m_runScore;
        [Tooltip("Run ended, with the time of the run.")]
        [SerializeField] private TMP_Text m_runEndedText;
        [SerializeField] private TMP_Text m_scoreText;
        [SerializeField] private GameObject m_newHighscoreBadge;
        [SerializeField] private TMP_Text m_coinText;
        [SerializeField] private TMP_Text m_timeText;
        [SerializeField] private TMP_Text m_distanceText;
        [SerializeField] private Button m_reviveButton;
        [SerializeField] private Button m_playAgainButton;
        [SerializeField] private Button m_hangarButton;
        [SerializeField] private Button m_menuButton;

        // For tests, which tap them like a player.
        internal Button ReviveButton => m_reviveButton;

        internal Button PlayAgainButton => m_playAgainButton;

        internal Button HangarButton => m_hangarButton;

        // For tests, which check it after beating a highscore.
        internal GameObject NewHighscoreBadge => m_newHighscoreBadge;

        private void OnEnable()
        {
            m_reviveButton.onClick.AddListener(m_gameManager.RequestRevive);
            m_playAgainButton.onClick.AddListener(m_gameManager.PlayAgain);
            m_hangarButton.onClick.AddListener(m_gameManager.ReturnToHangar);
            m_menuButton.onClick.AddListener(m_gameManager.ReturnToMenu);

            int seconds = (int)m_runScore.FlightTime;
            float minutes = seconds / 60;
            float secondsOfMinute = seconds % 60;
            m_runEndedText.SetText("RUN ENDED · {0}:{1:00}", minutes, secondsOfMinute);
            m_timeText.SetText("{0}:{1:00}", minutes, secondsOfMinute);
            m_scoreText.SetText("{0}", m_runScore.Score);
            m_newHighscoreBadge.SetActive(m_runScore.IsNewHighscore);
            m_coinText.SetText("+{0}", m_runScore.Coins);
            m_distanceText.SetText("{0} m", m_runScore.Distance);
            m_reviveButton.gameObject.SetActive(m_gameManager.CanRevive);
        }

        private void OnDisable()
        {
            m_reviveButton.onClick.RemoveListener(m_gameManager.RequestRevive);
            m_playAgainButton.onClick.RemoveListener(m_gameManager.PlayAgain);
            m_hangarButton.onClick.RemoveListener(m_gameManager.ReturnToHangar);
            m_menuButton.onClick.RemoveListener(m_gameManager.ReturnToMenu);
        }
    }
}
