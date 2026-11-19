using System.Collections.Generic;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// All achievements of the game, in the order the achievements screen keeps within each state.
    /// </summary>
    [CreateAssetMenu(fileName = "AchievementCatalog", menuName = "Space Escaper/Achievement Catalog")]
    public class AchievementCatalog : ScriptableObject
    {
        [SerializeField] private List<AchievementData> m_achievements;

        public IReadOnlyList<AchievementData> Achievements => m_achievements;
    }
}
