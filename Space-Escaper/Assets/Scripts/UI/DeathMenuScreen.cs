using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The death screen: the score and the coins of the run, the revive while the run still has
    /// it, and the way back to the main menu.
    /// </summary>
    public class DeathMenuScreen : MonoBehaviour
    {
        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private RunScore m_runScore;
        [SerializeField] private TMP_Text m_scoreText;
        [SerializeField] private TMP_Text m_coinText;
        [SerializeField] private Button m_reviveButton;
        [SerializeField] private Button m_menuButton;

        // For tests, which tap it like a player.
        internal Button ReviveButton => m_reviveButton;

        private void OnEnable()
        {
            m_reviveButton.onClick.AddListener(m_gameManager.RequestRevive);
            m_menuButton.onClick.AddListener(m_gameManager.ReturnToMenu);

            m_scoreText.SetText("{0}", m_runScore.Score);
            m_coinText.SetText("{0}", m_runScore.Coins);
            m_reviveButton.gameObject.SetActive(m_gameManager.CanRevive);
        }

        private void OnDisable()
        {
            m_reviveButton.onClick.RemoveListener(m_gameManager.RequestRevive);
            m_menuButton.onClick.RemoveListener(m_gameManager.ReturnToMenu);
        }
    }
}
