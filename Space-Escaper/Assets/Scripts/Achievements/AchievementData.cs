using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// What the game knows about one achievement: what it counts and how far, its reward and how the screens show
    /// it. One asset per achievement, all of them listed in the <see cref="AchievementCatalog"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "AchievementData", menuName = "Space Escaper/Achievement Data")]
    public class AchievementData : ScriptableObject
    {
        [Tooltip("The save stores the achievement under this ID. Changing it loses it in existing saves.")]
        [SerializeField] private string m_id;
        [SerializeField] private string m_title;
        [Tooltip("What it takes, in one line.")]
        [SerializeField] private string m_description;
        [SerializeField] private Sprite m_icon;
        [SerializeField] private AchievementGoal m_goal;
        [Tooltip("How many it takes. Owning ships and unlocking other achievements take all there are instead.")]
        [SerializeField] private int m_target = 1;
        [Tooltip("Shown after the numbers of the progress, like m for meters.")]
        [SerializeField] private string m_unit;
        [Tooltip("Coins it pays out when claimed on the achievements screen.")]
        [SerializeField] private int m_reward;
        [Tooltip("Greyed out with SOON and never unlocked, until what it counts is in the game.")]
        [SerializeField] private bool m_isComingSoon;

        public string Id => m_id;
        public string Title => m_title;
        public string Description => m_description;
        public Sprite Icon => m_icon;
        public AchievementGoal Goal => m_goal;
        public int Target => m_target;
        public string Unit => m_unit;
        public int Reward => m_reward;
        public bool IsComingSoon => m_isComingSoon;
    }
}
