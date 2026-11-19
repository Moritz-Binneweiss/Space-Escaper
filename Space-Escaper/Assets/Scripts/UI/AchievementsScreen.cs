using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The achievements, opened from the main menu. Until the achievements come with v1.5.3, the screen
    /// shows six examples from the mockups, which do nothing yet.
    /// </summary>
    public class AchievementsScreen : MonoBehaviour
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

        // The achievements leave the state of the game as it is, so going back shows the screen of the
        // state, the main menu.
        private void BackButton_Clicked()
        {
            m_screens.Show(m_gameManager.State);
        }
    }
}
