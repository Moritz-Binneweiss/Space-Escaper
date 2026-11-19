using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;

namespace SpaceEscaper.Tests
{
    public class AchievementCatalogTests
    {
        private const string k_CatalogPath = "Assets/Data/Achievements/AchievementCatalog.asset";

        // Nothing counts these before the collectibles come with v1.8.0.
        private static readonly AchievementGoal[] s_collectibleGoals =
        {
            AchievementGoal.Logbooks, AchievementGoal.StarSystemCards, AchievementGoal.Plants,
            AchievementGoal.Collectibles,
        };

        private AchievementCatalog m_catalog;

        [SetUp]
        public void LoadCatalog()
        {
            m_catalog = AssetDatabase.LoadAssetAtPath<AchievementCatalog>(k_CatalogPath);
        }

        [Test]
        public void CatalogListsEveryAchievementOnce()
        {
            string[] assets = AssetDatabase.FindAssets("t:" + nameof(AchievementData), new[] { "Assets" });
            HashSet<AchievementData> listed = new HashSet<AchievementData>(m_catalog.Achievements);

            Assert.That(m_catalog.Achievements, Has.None.Null);
            Assert.That(listed.Count, Is.EqualTo(m_catalog.Achievements.Count), "An achievement is listed twice.");
            Assert.That(listed.Count, Is.EqualTo(assets.Length), "An achievement asset is missing in the catalog.");
        }

        [Test]
        public void AchievementIdsAreSetAndUnique()
        {
            HashSet<string> ids = new HashSet<string>();
            foreach (AchievementData achievement in m_catalog.Achievements)
            {
                Assert.That(achievement.Id, Is.Not.Empty, $"{achievement.name} has no ID");
                Assert.That(ids.Add(achievement.Id), Is.True, $"The ID {achievement.Id} is used twice");
            }
        }

        // Titles are capitals, like everything else in Audiowide.
        [Test]
        public void EveryAchievementHasTitleDescriptionIconAndReward()
        {
            foreach (AchievementData achievement in m_catalog.Achievements)
            {
                Assert.That(achievement.Title, Is.Not.Empty, $"{achievement.name} has no title");
                Assert.That(achievement.Title, Is.EqualTo(achievement.Title.ToUpperInvariant()),
                    $"The title of {achievement.name} is not in capitals");
                Assert.That(achievement.Description, Is.Not.Empty, $"{achievement.name} has no description");
                Assert.That(achievement.Icon, Is.Not.Null, $"{achievement.name} has no icon");
                Assert.That(achievement.Reward, Is.GreaterThan(0), $"{achievement.name} pays out nothing");
            }
        }

        // Owning ships and unlocking the others take all there are, the target would only mislead there.
        [Test]
        public void TargetIsSetWhereItCounts()
        {
            foreach (AchievementData achievement in m_catalog.Achievements)
            {
                bool takesAll = achievement.Goal == AchievementGoal.ShipsOwned
                    || achievement.Goal == AchievementGoal.OtherAchievements;
                if (takesAll)
                {
                    Assert.That(achievement.Target, Is.Zero, $"{achievement.name} takes all, its target is not read");
                }
                else
                {
                    Assert.That(achievement.Target, Is.GreaterThan(0), $"{achievement.name} has no target");
                }
            }
        }

        // The achievement for all the others waits for the collectibles, so it shows SOON with them.
        [Test]
        public void OnlyTheCollectiblesAndAllOthersComeLater()
        {
            foreach (AchievementData achievement in m_catalog.Achievements)
            {
                bool isCollectible = System.Array.IndexOf(s_collectibleGoals, achievement.Goal) >= 0;
                bool countsAllOthers = achievement.Goal == AchievementGoal.OtherAchievements;
                Assert.That(achievement.IsComingSoon, Is.EqualTo(isCollectible || countsAllOthers),
                    $"{achievement.name} comes later: {achievement.IsComingSoon}");
            }
        }
    }
}
