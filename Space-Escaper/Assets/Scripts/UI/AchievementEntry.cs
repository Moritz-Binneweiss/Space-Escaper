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
        [SerializeField] private TMP_Text m_descriptionText;
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
        [SerializeField] private TMP_Text m_progressText;
        [SerializeField] private Button m_claimButton;
        [Tooltip("The coins of the reward on the claim button.")]
        [SerializeField] private TMP_Text m_claimText;

        private string m_progressFormat;

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
        /// Shows the achievement in a state, in the colors of that state.
        /// </summary>
        public void Show(AchievementState state, int progress, int target, AchievementLook look)
        {
            m_icon.sprite = m_achievement.Icon;
            m_titleText.SetText(m_achievement.Title);
            m_descriptionText.SetText(m_achievement.Description);
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
                m_progressText.SetText(GetProgressFormat(), (float)progress, (float)target);
            }

            m_background.color = look.Background;
            m_border.color = look.Border;
            m_iconTile.color = look.IconTile;
            m_icon.color = look.Icon;
            m_titleText.color = look.Title;
            m_descriptionText.color = look.Description;
            m_rewardText.color = look.Reward;
            m_rewardCoin.color = look.RewardCoin;
        }

        // Built once, so showing the progress makes no new string each time the screen opens.
        private string GetProgressFormat()
        {
            if (m_progressFormat == null)
            {
                m_progressFormat = string.IsNullOrEmpty(m_achievement.Unit)
                    ? "{0} / {1}"
                    : "{0} / {1} " + m_achievement.Unit;
            }

            return m_progressFormat;
        }

        private void OnClaimClicked()
        {
            ClaimClicked?.Invoke(this);
        }
    }
}
