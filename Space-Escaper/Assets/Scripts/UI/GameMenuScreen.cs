using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The screen during a run: score, coins and score multiplier, and the pause button. Every few hundred points it
    /// shows how far the highscore is and says when the run beats it, and an achievement the run unlocks pops up
    /// under it for a moment. The first run after the app starts shows how to change lanes.
    /// </summary>
    public class GameMenuScreen : MonoBehaviour
    {
        private const float k_PopupDuration = 2.5f;
        private const float k_SwipeHintDuration = 6f;

        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private RunScore m_runScore;
        [SerializeField] private TMP_Text m_scoreText;
        [SerializeField] private TMP_Text m_coinText;
        [SerializeField] private TMP_Text m_modifierText;
        [Tooltip("The coin icon, which pops for every coin collected.")]
        [SerializeField] private PopEffect m_coinIconPop;
        [SerializeField] private Button m_pauseButton;

        [Header("Highscore")]
        [Tooltip("Points between two looks at the way to the highscore.")]
        [SerializeField] private int m_highscoreBarStep = 100;
        [Tooltip("The way to the highscore, shown for a moment.")]
        [SerializeField] private GameObject m_highscoreBar;
        [SerializeField] private TMP_Text m_highscoreBarText;
        [SerializeField] private TMP_Text m_pointsToGoText;
        [Tooltip("Filled as far as the score has come, by its right anchor.")]
        [SerializeField] private RectTransform m_highscoreBarFill;
        [SerializeField] private GameObject m_newHighscoreBanner;

        [Header("Achievements")]
        [SerializeField] private AchievementTracker m_achievements;
        [Tooltip("An achievement the run has just unlocked, shown for a moment under the way to the highscore.")]
        [SerializeField] private GameObject m_achievementPopup;
        [SerializeField] private TMP_Text m_achievementTitleText;
        [SerializeField] private TMP_Text m_achievementRewardText;

        [Header("First Run")]
        [SerializeField] private GameObject m_swipeHint;

        private readonly List<AchievementData> m_achievementsToShow = new List<AchievementData>();
        private int m_shownScore;
        private int m_shownCoins;
        private float m_shownModifier;
        private int m_lastHighscoreBarStep;
        private float m_highscoreBarEndTime;
        private bool m_hasShownNewHighscore;
        private float m_newHighscoreBannerEndTime;
        private float m_achievementPopupEndTime;
        private float m_swipeHintEndTime;

        // Once per start of the app, so a run after a reload does not show it again.
        private static bool s_hasShownSwipeHint;

        // For tests, which check that the screen is up during the run.
        internal Button PauseButton => m_pauseButton;

        // For tests, which check what pops up during the run.
        internal GameObject AchievementPopup => m_achievementPopup;

        internal TMP_Text AchievementTitleText => m_achievementTitleText;

        // The editor starts Play Mode without a domain reload, so static fields would keep their
        // values from the last one.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            s_hasShownSwipeHint = false;
        }

        // The screen comes on with the run and again after a pause or a revive. The popups go on where they were.
        private void OnEnable()
        {
            m_pauseButton.onClick.AddListener(m_gameManager.Pause);
            m_achievements.Unlocked += AchievementTracker_Unlocked;
            ShowScore();
            ShowCoins();
            ShowModifier();

            if (!s_hasShownSwipeHint)
            {
                s_hasShownSwipeHint = true;
                m_swipeHint.SetActive(true);
                m_swipeHintEndTime = Time.time + k_SwipeHintDuration;
            }
        }

        private void OnDisable()
        {
            m_pauseButton.onClick.RemoveListener(m_gameManager.Pause);
            m_achievements.Unlocked -= AchievementTracker_Unlocked;
        }

        // After every Update, so the screen shows the score of this frame. A text only changes
        // with its number, which keeps canvas rebuilds out of the other frames. SetText writes the
        // number into the text's own buffer, in a build without a new string.
        private void LateUpdate()
        {
            if (m_runScore.Score != m_shownScore)
            {
                ShowScore();
                ShowHighscoreProgress();
            }

            if (m_runScore.Coins != m_shownCoins)
            {
                ShowCoins();
                m_coinIconPop.Play();
            }

            if (m_runScore.Modifier != m_shownModifier)
            {
                ShowModifier();
            }

            HideWhenDone(m_highscoreBar, m_highscoreBarEndTime);
            HideWhenDone(m_newHighscoreBanner, m_newHighscoreBannerEndTime);
            ShowAchievementsUnlocked();
            HideSwipeHint();
        }

        /// <summary>
        /// Shows an achievement on the popup. For tests also outside a run.
        /// </summary>
        internal void ShowAchievement(AchievementData achievement)
        {
            m_achievementTitleText.SetText(achievement.Title);
            m_achievementRewardText.SetText("+{0}", achievement.Reward);
        }

        // Only what unlocks while the screen is up. What the crash unlocks comes with the death screen, on its card.
        private void AchievementTracker_Unlocked(IReadOnlyList<AchievementData> achievements)
        {
            for (int i = 0; i < achievements.Count; i++)
            {
                m_achievementsToShow.Add(achievements[i]);
            }
        }

        private void ShowScore()
        {
            m_shownScore = m_runScore.Score;
            m_scoreText.SetText("{0}", m_shownScore);
        }

        private void ShowCoins()
        {
            m_shownCoins = m_runScore.Coins;
            m_coinText.SetText("{0}", m_shownCoins);
        }

        private void ShowModifier()
        {
            m_shownModifier = m_runScore.Modifier;
            m_modifierText.SetText("×{0:0.0}", m_shownModifier);
        }

        // The highscore to beat is the one the run started with. Once the run has beaten it, the banner says so once and
        // the bar is done for this run.
        private void ShowHighscoreProgress()
        {
            int highscore = m_runScore.PreviousHighscore;
            if (m_runScore.IsNewHighscore)
            {
                if (!m_hasShownNewHighscore)
                {
                    m_hasShownNewHighscore = true;
                    m_highscoreBar.SetActive(false);
                    m_newHighscoreBanner.SetActive(true);
                    m_newHighscoreBannerEndTime = Time.time + k_PopupDuration;
                }

                return;
            }

            if (highscore <= 0)
            {
                return;
            }

            int step = m_shownScore / m_highscoreBarStep;
            if (step > m_lastHighscoreBarStep)
            {
                m_lastHighscoreBarStep = step;
                m_highscoreBarText.SetText("{0}", highscore);
                m_highscoreBar.SetActive(true);
                m_highscoreBarEndTime = Time.time + k_PopupDuration;
            }

            if (m_highscoreBar.activeSelf)
            {
                m_pointsToGoText.SetText("{0} TO GO", highscore - m_shownScore);
                m_highscoreBarFill.anchorMax = new Vector2((float)m_shownScore / highscore, 1f);
            }
        }

        // Several that unlock together come one after another, each as long as the other popups.
        private void ShowAchievementsUnlocked()
        {
            HideWhenDone(m_achievementPopup, m_achievementPopupEndTime);
            if (m_achievementPopup.activeSelf || m_achievementsToShow.Count == 0)
            {
                return;
            }

            ShowAchievement(m_achievementsToShow[0]);
            m_achievementsToShow.RemoveAt(0);
            m_achievementPopup.SetActive(true);
            m_achievementPopupEndTime = Time.time + k_PopupDuration;
            AudioSystem.Instance.PlayAchievementUnlocked();
        }

        // Time.time stands still in the pause, so a popup keeps its time left.
        private static void HideWhenDone(GameObject popup, float endTime)
        {
            if (popup.activeSelf && Time.time >= endTime)
            {
                popup.SetActive(false);
            }
        }

        // Done after the first swipe, which shows the player has found out.
        private void HideSwipeHint()
        {
            if (!m_swipeHint.activeSelf)
            {
                return;
            }

            MobileInput input = MobileInput.Instance;
            if (Time.time >= m_swipeHintEndTime || input.HasSwipedLeft || input.HasSwipedRight)
            {
                m_swipeHint.SetActive(false);
            }
        }
    }
}
