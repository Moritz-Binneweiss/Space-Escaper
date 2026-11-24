using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The main menu: shows the coins, the highscore and the rewards to claim, starts a run and opens the hangar,
    /// the settings, the achievements or the collectibles.
    /// </summary>
    public class MainMenuScreen : MonoBehaviour
    {
        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private ScreenManager m_screens;
        [SerializeField] private AchievementTracker m_achievements;
        [SerializeField] private TMP_Text m_coinText;
        [SerializeField] private TMP_Text m_highscoreText;
        [Tooltip("The number of rewards to claim, on the corner of the medal. Hidden while there are none.")]
        [SerializeField] private GameObject m_rewardsBadge;
        [SerializeField] private TMP_Text m_rewardsText;
        [SerializeField] private Button m_startButton;
        [SerializeField] private Button m_hangarButton;
        [SerializeField] private Button m_settingsButton;
        [SerializeField] private Button m_achievementsButton;
        [SerializeField] private Button m_collectiblesButton;

        // For tests, which tap them like a player.
        internal Button StartButton => m_startButton;

        internal Button AchievementsButton => m_achievementsButton;

        internal Button HangarButton => m_hangarButton;

        internal Button SettingsButton => m_settingsButton;

        // For tests, which check them before and after claiming.
        internal GameObject RewardsBadge => m_rewardsBadge;

        internal TMP_Text RewardsText => m_rewardsText;

        // Coins change in the hangar. Coins and highscore change with a death, and the way back
        // from there reloads the scene.
        private void OnEnable()
        {
            m_startButton.onClick.AddListener(m_gameManager.Play);
            m_hangarButton.onClick.AddListener(HangarButton_Clicked);
            m_settingsButton.onClick.AddListener(m_screens.ShowSettings);
            m_achievementsButton.onClick.AddListener(m_screens.ShowAchievements);
            m_collectiblesButton.onClick.AddListener(m_screens.ShowCollectibles);
            m_achievements.Unlocked += AchievementTracker_Unlocked;

            SaveData saveData = SaveSystem.Data;
            m_coinText.SetText("{0}", saveData.Coins);
            m_highscoreText.SetText("{0}", saveData.Highscore);
            ShowRewardsToClaim();
        }

        private void OnDisable()
        {
            m_startButton.onClick.RemoveListener(m_gameManager.Play);
            m_hangarButton.onClick.RemoveListener(HangarButton_Clicked);
            m_settingsButton.onClick.RemoveListener(m_screens.ShowSettings);
            m_achievementsButton.onClick.RemoveListener(m_screens.ShowAchievements);
            m_collectiblesButton.onClick.RemoveListener(m_screens.ShowCollectibles);
            m_achievements.Unlocked -= AchievementTracker_Unlocked;
        }

        // The camera cuts to the hangar, behind the curtain.
        private void HangarButton_Clicked()
        {
            m_screens.ChangeBehindCurtain(m_gameManager.OpenHangar);
        }

        // The menu is on when the scene loads and when the hangar closes, and achievements unlock right after both.
        private void AchievementTracker_Unlocked(IReadOnlyList<AchievementData> achievements)
        {
            ShowRewardsToClaim();
        }

        private void ShowRewardsToClaim()
        {
            int rewardsToClaim = m_achievements.RewardsToClaim;
            m_rewardsBadge.SetActive(rewardsToClaim > 0);
            m_rewardsText.SetText("{0}", rewardsToClaim);
        }
    }
}
