using System;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// The colors of an entry on the achievements screen in one of its states.
    /// </summary>
    [Serializable]
    public struct AchievementLook
    {
        [SerializeField] private Color m_background;
        [SerializeField] private Color m_border;
        [SerializeField] private Color m_iconTile;
        [SerializeField] private Color m_icon;
        [SerializeField] private Color m_title;
        [SerializeField] private Color m_description;
        [Tooltip("The amount of the reward next to the title.")]
        [SerializeField] private Color m_reward;
        [Tooltip("Tints the coin in front of the amount.")]
        [SerializeField] private Color m_rewardCoin;

        public Color Background => m_background;
        public Color Border => m_border;
        public Color IconTile => m_iconTile;
        public Color Icon => m_icon;
        public Color Title => m_title;
        public Color Description => m_description;
        public Color Reward => m_reward;
        public Color RewardCoin => m_rewardCoin;
    }
}
