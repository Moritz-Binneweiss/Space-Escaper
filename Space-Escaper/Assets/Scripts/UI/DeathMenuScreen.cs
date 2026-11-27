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
        private const string k_AchievementUnlockedKey = "common.achievementUnlocked";
        private const string k_AchievementsUnlockedKey = "death.achievementsUnlocked";

        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private ScreenManager m_screens;
        [SerializeField] private RunScore m_runScore;
        [SerializeField] private AchievementTracker m_achievements;
        [Tooltip("Run ended, with the time of the run.")]
        [SerializeField] private LocalizedText m_runEndedText;
        [SerializeField] private LocalizedText m_scoreText;
        [SerializeField] private GameObject m_newHighscoreBadge;
        [SerializeField] private LocalizedText m_coinText;
        [SerializeField] private LocalizedText m_timeText;
        [SerializeField] private LocalizedText m_distanceText;
        [Tooltip("The first achievement the run has unlocked, with its reward. Hidden while it has unlocked none.")]
        [SerializeField] private GameObject m_unlockedCard;
        [SerializeField] private LocalizedText m_unlockedLabelText;
        [SerializeField] private TMP_Text m_unlockedTitleText;
        [SerializeField] private LocalizedText m_unlockedRewardText;
        [SerializeField] private Button m_reviveButton;
        [SerializeField] private Button m_playAgainButton;
        [SerializeField] private Button m_hangarButton;
        [SerializeField] private Button m_menuButton;

        private DropEffect m_newHighscoreBadgeDrop;
        private DropEffect m_unlockedCardDrop;

        // For tests, which tap them like a player.
        internal Button ReviveButton => m_reviveButton;

        internal Button PlayAgainButton => m_playAgainButton;

        internal Button HangarButton => m_hangarButton;

        internal Button MenuButton => m_menuButton;

        // For tests, which check it after beating a highscore.
        internal GameObject NewHighscoreBadge => m_newHighscoreBadge;

        // For tests, which check what the run has unlocked.
        internal GameObject UnlockedCard => m_unlockedCard;

        internal TMP_Text UnlockedLabelText => m_unlockedLabelText.Text;

        internal TMP_Text UnlockedTitleText => m_unlockedTitleText;

        // The badge and the card drop in shortly after the screen has come.
        private void Awake()
        {
            m_newHighscoreBadgeDrop = m_newHighscoreBadge.GetComponent<DropEffect>();
            m_unlockedCardDrop = m_unlockedCard.GetComponent<DropEffect>();
        }

        // The screen comes on with the crash, and what the crash unlocks comes right after, in the same call. Badge
        // and card drop in again each time, also after a revive.
        private void OnEnable()
        {
            m_reviveButton.onClick.AddListener(m_gameManager.RequestRevive);
            m_playAgainButton.onClick.AddListener(PlayAgainButton_Clicked);
            m_hangarButton.onClick.AddListener(HangarButton_Clicked);
            m_menuButton.onClick.AddListener(MenuButton_Clicked);
            m_achievements.Unlocked += AchievementTracker_Unlocked;
            m_newHighscoreBadge.SetActive(false);
            m_unlockedCard.SetActive(false);
            ShowUnlocked();

            int seconds = (int)m_runScore.FlightTime;
            float minutes = seconds / 60;
            float secondsOfMinute = seconds % 60;
            m_runEndedText.ShowNumbers(minutes, secondsOfMinute);
            m_timeText.ShowNumbers(minutes, secondsOfMinute);
            m_scoreText.ShowNumbers(m_runScore.Score);
            if (m_runScore.IsNewHighscore)
            {
                m_newHighscoreBadgeDrop.Play();
            }

            m_coinText.ShowNumbers(m_runScore.Coins);
            m_distanceText.ShowNumbers(m_runScore.Distance);
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
                m_unlockedLabelText.Show(k_AchievementsUnlockedKey, count - 1);
            }
            else
            {
                m_unlockedLabelText.Show(k_AchievementUnlockedKey);
            }

            m_unlockedTitleText.SetText(first.Title);
            m_unlockedRewardText.ShowNumbers(first.Reward);
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

        // A card already on its way stays on it.
        private void ShowUnlocked()
        {
            IReadOnlyList<AchievementData> unlocked = m_achievements.UnlockedThisRun;
            if (unlocked.Count == 0)
            {
                m_unlockedCard.SetActive(false);
                return;
            }

            ShowUnlocked(unlocked[0], unlocked.Count);
            m_unlockedCardDrop.Play();
        }
    }
}
