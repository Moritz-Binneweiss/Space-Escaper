using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The death screen: score, coins, time and distance of the run, whether it set a new highscore, what it unlocked,
    /// the revive while the run still has it, and the ways to a new run, the hangar or the main menu.
    /// </summary>
    public class DeathMenuScreen : MonoBehaviour
    {
        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private ScreenManager m_screens;
        [SerializeField] private RunScore m_runScore;
        [SerializeField] private AchievementTracker m_achievements;
        [Tooltip("Run ended, with the time of the run.")]
        [SerializeField] private TMP_Text m_runEndedText;
        [SerializeField] private TMP_Text m_scoreText;
        [SerializeField] private GameObject m_newHighscoreBadge;
        [SerializeField] private TMP_Text m_coinText;
        [SerializeField] private TMP_Text m_timeText;
        [SerializeField] private TMP_Text m_distanceText;
        [Tooltip("The first achievement the run has unlocked, with its reward. Hidden while it has unlocked none.")]
        [SerializeField] private GameObject m_unlockedCard;
        [SerializeField] private TMP_Text m_unlockedLabelText;
        [SerializeField] private TMP_Text m_unlockedTitleText;
        [SerializeField] private TMP_Text m_unlockedRewardText;
        [SerializeField] private Button m_reviveButton;
        [SerializeField] private Button m_playAgainButton;
        [SerializeField] private Button m_hangarButton;
        [SerializeField] private Button m_menuButton;

        // For tests, which tap them like a player.
        internal Button ReviveButton => m_reviveButton;

        internal Button PlayAgainButton => m_playAgainButton;

        internal Button HangarButton => m_hangarButton;

        internal Button MenuButton => m_menuButton;

        // For tests, which check it after beating a highscore.
        internal GameObject NewHighscoreBadge => m_newHighscoreBadge;

        // For tests, which check what the run has unlocked.
        internal GameObject UnlockedCard => m_unlockedCard;

        internal TMP_Text UnlockedLabelText => m_unlockedLabelText;

        internal TMP_Text UnlockedTitleText => m_unlockedTitleText;

        // The screen comes on with the crash, and what the crash unlocks comes right after, in the same call.
        private void OnEnable()
        {
            m_reviveButton.onClick.AddListener(m_gameManager.RequestRevive);
            m_playAgainButton.onClick.AddListener(PlayAgainButton_Clicked);
            m_hangarButton.onClick.AddListener(HangarButton_Clicked);
            m_menuButton.onClick.AddListener(MenuButton_Clicked);
            m_achievements.Unlocked += AchievementTracker_Unlocked;
            ShowUnlocked();

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
            m_playAgainButton.onClick.RemoveListener(PlayAgainButton_Clicked);
            m_hangarButton.onClick.RemoveListener(HangarButton_Clicked);
            m_menuButton.onClick.RemoveListener(MenuButton_Clicked);
            m_achievements.Unlocked -= AchievementTracker_Unlocked;
        }

        /// <summary>
        /// Shows an achievement on the card and how many more the run has unlocked. For tests also outside a run.
        /// </summary>
        internal void ShowUnlocked(AchievementData first, int count)
        {
            if (count > 1)
            {
                m_unlockedLabelText.SetText("ACHIEVEMENT UNLOCKED · +{0}", count - 1);
            }
            else
            {
                m_unlockedLabelText.SetText("ACHIEVEMENT UNLOCKED");
            }

            m_unlockedTitleText.SetText(first.Title);
            m_unlockedRewardText.SetText("+{0}", first.Reward);
        }

        private void AchievementTracker_Unlocked(IReadOnlyList<AchievementData> achievements)
        {
            ShowUnlocked();
        }

        // The three ways on reload the scene, behind the curtain. The revive goes on in this one.
        private void PlayAgainButton_Clicked()
        {
            m_screens.ChangeBehindCurtain(m_gameManager.PlayAgain);
        }

        private void HangarButton_Clicked()
        {
            m_screens.ChangeBehindCurtain(m_gameManager.ReturnToHangar);
        }

        private void MenuButton_Clicked()
        {
            m_screens.ChangeBehindCurtain(m_gameManager.ReturnToMenu);
        }

        private void ShowUnlocked()
        {
            IReadOnlyList<AchievementData> unlocked = m_achievements.UnlockedThisRun;
            m_unlockedCard.SetActive(unlocked.Count > 0);
            if (unlocked.Count > 0)
            {
                ShowUnlocked(unlocked[0], unlocked.Count);
            }
        }
    }
}
