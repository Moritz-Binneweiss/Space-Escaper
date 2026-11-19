using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The credits, opened from the settings. Going back shows the settings again, which leave the state
    /// of the game as it is.
    /// </summary>
    public class CreditsScreen : MonoBehaviour
    {
        [SerializeField] private ScreenManager m_screens;
        [SerializeField] private Button m_backButton;

        private void OnEnable()
        {
            m_backButton.onClick.AddListener(m_screens.ShowSettings);
        }

        private void OnDisable()
        {
            m_backButton.onClick.RemoveListener(m_screens.ShowSettings);
        }
    }
}
