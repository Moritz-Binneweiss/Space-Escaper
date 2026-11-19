using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The main menu: shows the coins and the highscore, starts a run and opens the hangar, the settings,
    /// the achievements or the collectibles.
    /// </summary>
    public class MainMenuScreen : MonoBehaviour
    {
        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private ScreenManager m_screens;
        [SerializeField] private TMP_Text m_coinText;
        [SerializeField] private TMP_Text m_highscoreText;
        [SerializeField] private Button m_startButton;
        [SerializeField] private Button m_hangarButton;
        [SerializeField] private Button m_settingsButton;
        [SerializeField] private Button m_achievementsButton;
        [SerializeField] private Button m_collectiblesButton;

        // For tests, which tap it like a player.
        internal Button StartButton => m_startButton;

        // Coins change in the hangar. Coins and highscore change with a death, and the way back
        // from there reloads the scene.
        private void OnEnable()
        {
            m_startButton.onClick.AddListener(m_gameManager.Play);
            m_hangarButton.onClick.AddListener(m_gameManager.OpenHangar);
            m_settingsButton.onClick.AddListener(m_screens.ShowSettings);
            m_achievementsButton.onClick.AddListener(m_screens.ShowAchievements);
            m_collectiblesButton.onClick.AddListener(m_screens.ShowCollectibles);

            SaveData saveData = SaveSystem.Data;
            m_coinText.SetText("{0}", saveData.Coins);
            m_highscoreText.SetText("{0}", saveData.Highscore);
        }

        private void OnDisable()
        {
            m_startButton.onClick.RemoveListener(m_gameManager.Play);
            m_hangarButton.onClick.RemoveListener(m_gameManager.OpenHangar);
            m_settingsButton.onClick.RemoveListener(m_screens.ShowSettings);
            m_achievementsButton.onClick.RemoveListener(m_screens.ShowAchievements);
            m_collectiblesButton.onClick.RemoveListener(m_screens.ShowCollectibles);
        }
    }
}
