using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The collectibles, opened from the main menu. Until the collectibles come, the screen shows
    /// greyed out places for them that do nothing.
    /// </summary>
    public class CollectiblesScreen : MonoBehaviour
    {
        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private ScreenManager m_screens;
        [SerializeField] private Button m_backButton;

        private void OnEnable()
        {
            m_backButton.onClick.AddListener(BackButton_Clicked);
        }

        private void OnDisable()
        {
            m_backButton.onClick.RemoveListener(BackButton_Clicked);
        }

        // The collectibles leave the state of the game as it is, so going back shows the screen of the
        // state, the main menu.
        private void BackButton_Clicked()
        {
            m_screens.Show(m_gameManager.State);
        }
    }
}
