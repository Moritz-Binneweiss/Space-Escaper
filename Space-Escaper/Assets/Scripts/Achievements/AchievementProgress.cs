using System.Collections.Generic;

namespace SpaceEscaper
{
    /// <summary>
    /// The rules of the achievements: how far a save has got with each, which it has reached and where each stands.
    /// A plain class, so tests can check the rules without a scene.
    /// </summary>
    public class AchievementProgress
    {
        private readonly IReadOnlyList<AchievementData> m_achievements;
        private readonly IReadOnlyList<string> m_shipIds;

        /// <param name="achievements">All achievements of the game.</param>
        /// <param name="shipIds">All ships of the game, for the achievement that takes owning them.</param>
        public AchievementProgress(IReadOnlyList<AchievementData> achievements, IReadOnlyList<string> shipIds)
        {
            m_achievements = achievements;
            m_shipIds = shipIds;
        }

        /// <summary>
        /// How many the achievement takes: its target, or all ships or all other achievements there are.
        /// </summary>
        public int GetTarget(AchievementData achievement)
        {
            switch (achievement.Goal)
            {
                case AchievementGoal.ShipsOwned:
                    return m_shipIds.Count;

                case AchievementGoal.OtherAchievements:
                    return m_achievements.Count - 1;

                default:
                    return achievement.Target;
            }
        }

        /// <summary>
        /// How far the save has got with the achievement, counted like its target.
        /// </summary>
        public int GetProgress(AchievementData achievement, SaveData saveData)
        {
            switch (achievement.Goal)
            {
                case AchievementGoal.Flights:
                    return saveData.Flights;

                case AchievementGoal.FlightsInBoughtShips:
                    return saveData.FlightsInBoughtShips;

                case AchievementGoal.Crashes:
                    return saveData.Crashes;

                case AchievementGoal.LongestRun:
                    return saveData.LongestRun;

                case AchievementGoal.CoinsCollected:
                    return saveData.CoinsCollected;

                case AchievementGoal.ShipsOwned:
                    return CountShipsOwned(saveData);

                case AchievementGoal.OtherAchievements:
                    return CountOthersUnlocked(achievement, saveData);

                default:
                    // The collectibles come with v1.8.0, nothing counts them before.
                    return 0;
            }
        }

        public AchievementState GetState(AchievementData achievement, SaveData saveData)
        {
            if (achievement.IsComingSoon)
            {
                return AchievementState.ComingSoon;
            }

            if (saveData.IsAchievementClaimed(achievement.Id))
            {
                return AchievementState.Claimed;
            }

            return saveData.IsAchievementUnlocked(achievement.Id)
                ? AchievementState.ToClaim
                : AchievementState.InProgress;
        }

        /// <summary>
        /// Unlocks every achievement the save has reached and adds it to <paramref name="unlocked"/>. The ones that
        /// count other achievements come last, so they count those that unlock in the same call.
        /// </summary>
        /// <remarks>
        /// An unlocked achievement stays unlocked, even if its target grows later, as with a new ship.
        /// </remarks>
        public void UnlockReached(SaveData saveData, List<AchievementData> unlocked)
        {
            UnlockReached(saveData, unlocked, false);
            UnlockReached(saveData, unlocked, true);
        }

        public int CountToClaim(SaveData saveData)
        {
            int count = 0;
            for (int i = 0; i < m_achievements.Count; i++)
            {
                if (GetState(m_achievements[i], saveData) == AchievementState.ToClaim)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// The share of all achievements unlocked, claimed or not, from 0 to 1.
        /// </summary>
        public float GetShareUnlocked(SaveData saveData)
        {
            int count = 0;
            for (int i = 0; i < m_achievements.Count; i++)
            {
                if (saveData.IsAchievementUnlocked(m_achievements[i].Id))
                {
                    count++;
                }
            }

            return m_achievements.Count > 0 ? (float)count / m_achievements.Count : 0f;
        }

        private void UnlockReached(SaveData saveData, List<AchievementData> unlocked, bool isCountingOthers)
        {
            for (int i = 0; i < m_achievements.Count; i++)
            {
                AchievementData achievement = m_achievements[i];
                bool isReached = !achievement.IsComingSoon
                    && (achievement.Goal == AchievementGoal.OtherAchievements) == isCountingOthers
                    && !saveData.IsAchievementUnlocked(achievement.Id)
                    && GetProgress(achievement, saveData) >= GetTarget(achievement);
                if (isReached)
                {
                    saveData.UnlockAchievement(achievement.Id);
                    unlocked.Add(achievement);
                }
            }
        }

        private int CountShipsOwned(SaveData saveData)
        {
            int count = 0;
            for (int i = 0; i < m_shipIds.Count; i++)
            {
                if (saveData.IsShipUnlocked(m_shipIds[i]))
                {
                    count++;
                }
            }

            return count;
        }

        private int CountOthersUnlocked(AchievementData achievement, SaveData saveData)
        {
            int count = 0;
            for (int i = 0; i < m_achievements.Count; i++)
            {
                if (m_achievements[i] != achievement && saveData.IsAchievementUnlocked(m_achievements[i].Id))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
