using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// An entry on the achievements screen. It stands for one achievement and shows it on its way with its reward and
    /// progress, to claim with the claim button, claimed with a check, or greyed out with SOON until it comes.
    /// </summary>
    public class AchievementEntry : MonoBehaviour
    {
        [SerializeField] private AchievementData m_achievement;
        [SerializeField] private Image m_background;
        [SerializeField] private Image m_border;
        [SerializeField] private Image m_iconTile;
        [SerializeField] private Image m_icon;
        [SerializeField] private TMP_Text m_titleText;
        [SerializeField] private LocalizedText m_descriptionText;
        [Tooltip("The coins next to the title while the achievement is on its way or comes later.")]
        [SerializeField] private GameObject m_reward;
        [SerializeField] private Image m_rewardCoin;
        [SerializeField] private TMP_Text m_rewardText;
        [Tooltip("The check next to the title once the reward is claimed.")]
        [SerializeField] private GameObject m_claimedCheck;
        [SerializeField] private GameObject m_soonChip;
        [Tooltip("The bar under the title while the achievement is on its way, unless it takes just one.")]
        [SerializeField] private GameObject m_progress;
        [Tooltip("The green part of the bar, as wide as the share done.")]
        [SerializeField] private RectTransform m_progressFill;
        [SerializeField] private LocalizedText m_progressText;
        [SerializeField] private Button m_claimButton;
        [Tooltip("The coins of the reward on the claim button.")]
        [SerializeField] private TMP_Text m_claimText;

        public AchievementData Achievement => m_achievement;

        // For tests, which tap it like a player.
        internal Button ClaimButton => m_claimButton;

        public event Action<AchievementEntry> ClaimClicked;

        private void OnEnable()
        {
            m_claimButton.onClick.AddListener(OnClaimClicked);
        }

        private void OnDisable()
        {
            m_claimButton.onClick.RemoveListener(OnClaimClicked);
        }

        /// <summary>
        /// Shows the achievement in a state, in the colors of that state in the colorway the UI shows.
        /// </summary>
        public void Show(AchievementState state, int progress, int target, AchievementLook look)
        {
            m_icon.sprite = m_achievement.Icon;
            m_titleText.SetText(m_achievement.Title);
            m_descriptionText.Show(m_achievement.DescriptionKey, m_achievement.Target);
            m_rewardText.SetText("+{0}", m_achievement.Reward);
            m_claimText.SetText("{0}", m_achievement.Reward);

            m_reward.SetActive(state == AchievementState.InProgress || state == AchievementState.ComingSoon);
            m_claimedCheck.SetActive(state == AchievementState.Claimed);
            m_soonChip.SetActive(state == AchievementState.ComingSoon);
            m_claimButton.gameObject.SetActive(state == AchievementState.ToClaim);

            // An achievement that takes one, like the first flight, needs no bar to say how far it is.
            bool isShowingProgress = state == AchievementState.InProgress && target > 1;
            m_progress.SetActive(isShowingProgress);
            if (isShowingProgress)
            {
                m_progressFill.anchorMax = new Vector2(Mathf.Clamp01((float)progress / target), 1f);
                m_progressText.Show(m_achievement.ProgressKey, progress, target);
            }

            ColorwayData colorway = ColorwayManager.Current;
            m_background.color = look.Background.Resolve(colorway);
            m_border.color = look.Border.Resolve(colorway);
            m_iconTile.color = look.IconTile.Resolve(colorway);
            m_icon.color = look.Icon.Resolve(colorway);
            m_titleText.color = look.Title.Resolve(colorway);
            m_descriptionText.Text.color = look.Description.Resolve(colorway);
            m_rewardText.color = look.Reward.Resolve(colorway);
            m_rewardCoin.color = look.RewardCoin.Resolve(colorway);
        }

        private void OnClaimClicked()
        {
            ClaimClicked?.Invoke(this);
        }
    }
}
