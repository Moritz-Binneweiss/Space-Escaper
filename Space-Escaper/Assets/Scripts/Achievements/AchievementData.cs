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
        private const string k_ProgressKey = "achievements.progress";

        [Tooltip("The save stores the achievement under this ID. Changing it loses it in existing saves.")]
        [SerializeField] private string m_id;
        [SerializeField] private string m_title;
        [Tooltip("The key of what it takes, in one line, in the files of the languages. {0} is the target.")]
        [SerializeField] private string m_descriptionKey;
        [SerializeField] private Sprite m_icon;
        [SerializeField] private AchievementGoal m_goal;
        [Tooltip("How many it takes. Owning ships and unlocking other achievements take all there are instead.")]
        [SerializeField] private int m_target = 1;
        [Tooltip("The key of the numbers of its progress in the files of the languages, {0} how far it has come " +
            "and {1} the target, maybe with a unit like m for meters.")]
        [SerializeField] private string m_progressKey = k_ProgressKey;
        [Tooltip("Coins it pays out when claimed on the achievements screen.")]
        [SerializeField] private int m_reward;
        [Tooltip("Greyed out with SOON and never unlocked, until what it counts is in the game.")]
        [SerializeField] private bool m_isComingSoon;

        public string Id => m_id;
        public string Title => m_title;
        public string DescriptionKey => m_descriptionKey;
        public Sprite Icon => m_icon;
        public AchievementGoal Goal => m_goal;
        public int Target => m_target;
        public string ProgressKey => m_progressKey;
        public int Reward => m_reward;
        public bool IsComingSoon => m_isComingSoon;
    }
}
