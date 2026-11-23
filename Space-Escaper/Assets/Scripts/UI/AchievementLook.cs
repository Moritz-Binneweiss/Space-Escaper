using System;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// The colors of an entry on the achievements screen in one of its states, as roles in the colorway.
    /// </summary>
    [Serializable]
    public struct AchievementLook
    {
        [SerializeField] private ColorwayColor m_background;
        [SerializeField] private ColorwayColor m_border;
        [SerializeField] private ColorwayColor m_iconTile;
        [SerializeField] private ColorwayColor m_icon;
        [SerializeField] private ColorwayColor m_title;
        [SerializeField] private ColorwayColor m_description;
        [Tooltip("The amount of the reward next to the title.")]
        [SerializeField] private ColorwayColor m_reward;
        [Tooltip("Tints the coin in front of the amount.")]
        [SerializeField] private ColorwayColor m_rewardCoin;

        public ColorwayColor Background => m_background;
        public ColorwayColor Border => m_border;
        public ColorwayColor IconTile => m_iconTile;
        public ColorwayColor Icon => m_icon;
        public ColorwayColor Title => m_title;
        public ColorwayColor Description => m_description;
        public ColorwayColor Reward => m_reward;
        public ColorwayColor RewardCoin => m_rewardCoin;
    }
}
