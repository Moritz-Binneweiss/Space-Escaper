using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The death screen: revives the ship or goes back to the main menu.
    /// </summary>
    public class DeathMenuScreen : MonoBehaviour
    {
        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private Button m_reviveButton;
        [SerializeField] private Button m_menuButton;

        // For tests, which tap it like a player.
        internal Button ReviveButton => m_reviveButton;

        private void OnEnable()
        {
            m_reviveButton.onClick.AddListener(m_gameManager.RequestRevive);
            m_menuButton.onClick.AddListener(m_gameManager.ReturnToMenu);
        }

        private void OnDisable()
        {
            m_reviveButton.onClick.RemoveListener(m_gameManager.RequestRevive);
            m_menuButton.onClick.RemoveListener(m_gameManager.ReturnToMenu);
        }
    }
}
