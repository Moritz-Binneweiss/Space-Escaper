using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace SpaceEscaper.Tests
{
    /// <summary>
    /// The rules of the achievements on achievements made for the test, so a change to the real ones in their
    /// assets does not change what is tested.
    /// </summary>
    public class AchievementProgressTests
    {
        private const string k_StarterShip = "AristocratSkin1";
        private const string k_BoughtShip = "FreeterSkin1";
        private const string k_LastShip = "VagorSkin1";

        private static readonly string[] s_ships = { k_StarterShip, k_BoughtShip, k_LastShip };

        private readonly List<AchievementData> m_achievements = new List<AchievementData>();
        private readonly List<AchievementData> m_unlocked = new List<AchievementData>();
        private AchievementData m_firstFlight;
        private AchievementData m_boughtShip;
        private AchievementData m_crashes;
        private AchievementData m_longestRun;
        private AchievementData m_coins;
        private AchievementData m_allShips;
        private AchievementData m_logbooks;
        private AchievementData m_allOthers;
        private SaveData m_saveData;
        private AchievementProgress m_progress;

        [SetUp]
        public void CreateAchievements()
        {
            m_firstFlight = Create("FirstFlight", AchievementGoal.Flights, 1, 11);
            m_boughtShip = Create("BoughtShip", AchievementGoal.FlightsInBoughtShips, 1, 20);
            m_crashes = Create("Crashes", AchievementGoal.Crashes, 3, 66);
            m_longestRun = Create("LongestRun", AchievementGoal.LongestRun, 1998, 198);
            m_coins = Create("Coins", AchievementGoal.CoinsCollected, 1976, 300);
            m_allShips = Create("AllShips", AchievementGoal.ShipsOwned, 0, 900);
            m_logbooks = Create("Logbooks", AchievementGoal.Logbooks, 5, 500, true);
            m_allOthers = Create("AllOthers", AchievementGoal.OtherAchievements, 0, 1000);
            m_saveData = SaveData.FromJson(null);
            m_saveData.RepairShips(k_StarterShip, s_ships);
            m_progress = new AchievementProgress(m_achievements, s_ships);
        }

        [TearDown]
        public void DestroyAchievements()
        {
            foreach (AchievementData achievement in m_achievements)
            {
                Object.DestroyImmediate(achievement);
            }

            m_achievements.Clear();
            m_unlocked.Clear();
        }

        [Test]
        public void NewSaveHasNothingToClaim()
        {
            m_progress.UnlockReached(m_saveData, m_unlocked);

            Assert.That(m_unlocked, Is.Empty);
            Assert.That(m_progress.CountToClaim(m_saveData), Is.Zero);
            Assert.That(m_progress.GetShareUnlocked(m_saveData), Is.Zero);
            Assert.That(m_progress.GetState(m_firstFlight, m_saveData), Is.EqualTo(AchievementState.InProgress));
            Assert.That(m_progress.GetProgress(m_allShips, m_saveData), Is.EqualTo(1), "The starter ship is owned");
        }

        [Test]
        public void FirstFlightUnlocksTheFirstAchievement()
        {
            m_saveData.RecordFlight(false);
            m_progress.UnlockReached(m_saveData, m_unlocked);

            Assert.That(m_unlocked, Is.EqualTo(new[] { m_firstFlight }));
            Assert.That(m_progress.GetState(m_firstFlight, m_saveData), Is.EqualTo(AchievementState.ToClaim));
            Assert.That(m_progress.CountToClaim(m_saveData), Is.EqualTo(1));
        }

        [Test]
        public void OnlyAFlightInABoughtShipCounts()
        {
            for (int i = 0; i < 3; i++)
            {
                m_saveData.RecordFlight(false);
            }

            m_progress.UnlockReached(m_saveData, m_unlocked);
            Assert.That(m_saveData.IsAchievementUnlocked(m_boughtShip.Id), Is.False, "The starter ship counted");

            m_saveData.RecordFlight(true);
            m_progress.UnlockReached(m_saveData, m_unlocked);
            Assert.That(m_saveData.IsAchievementUnlocked(m_boughtShip.Id), Is.True);
            Assert.That(m_saveData.Flights, Is.EqualTo(4), "A flight in a bought ship is a flight too");
        }

        [Test]
        public void CrashesAddUpOverRuns()
        {
            m_saveData.RecordCrash(10);
            m_saveData.RecordCrash(20);
            m_progress.UnlockReached(m_saveData, m_unlocked);
            Assert.That(m_progress.GetProgress(m_crashes, m_saveData), Is.EqualTo(2));
            Assert.That(m_saveData.IsAchievementUnlocked(m_crashes.Id), Is.False);

            m_saveData.RecordCrash(30);
            m_progress.UnlockReached(m_saveData, m_unlocked);
            Assert.That(m_saveData.IsAchievementUnlocked(m_crashes.Id), Is.True);
        }

        // Two runs of 1000 m are not one of 1998.
        [Test]
        public void LongestRunCountsOneRun()
        {
            m_saveData.RecordCrash(1000);
            m_saveData.RecordCrash(1000);
            m_saveData.RecordCrash(400);
            m_progress.UnlockReached(m_saveData, m_unlocked);
            Assert.That(m_progress.GetProgress(m_longestRun, m_saveData), Is.EqualTo(1000));
            Assert.That(m_saveData.IsAchievementUnlocked(m_longestRun.Id), Is.False);

            m_saveData.RecordCrash(1998);
            m_progress.UnlockReached(m_saveData, m_unlocked);
            Assert.That(m_saveData.IsAchievementUnlocked(m_longestRun.Id), Is.True);
        }

        // The distance counts while the run grows, so the run gets the achievement before it crashes.
        [Test]
        public void DistanceUnlocksWhileTheRunGoesOn()
        {
            m_saveData.RecordRunDistance(1500);
            m_saveData.RecordRunDistance(1997);
            m_progress.UnlockReached(m_saveData, m_unlocked);
            Assert.That(m_saveData.IsAchievementUnlocked(m_longestRun.Id), Is.False);

            m_saveData.RecordRunDistance(1998);
            m_progress.UnlockReached(m_saveData, m_unlocked);
            Assert.That(m_saveData.IsAchievementUnlocked(m_longestRun.Id), Is.True);
            Assert.That(m_saveData.Crashes, Is.Zero, "The run counted as a crash");

            m_saveData.RecordRunDistance(400);
            Assert.That(m_saveData.LongestRun, Is.EqualTo(1998), "The next run made the longest one shorter");
        }

        // Rewards add to the coins to spend, not to the coins collected.
        [Test]
        public void OnlyCoinsFromRunsCountAsCollected()
        {
            m_saveData.RecordFlight(false);
            m_progress.UnlockReached(m_saveData, m_unlocked);
            m_saveData.TryClaimAchievement(m_firstFlight.Id, m_firstFlight.Reward);
            for (int i = 0; i < 1975; i++)
            {
                m_saveData.RecordCoin();
            }

            m_progress.UnlockReached(m_saveData, m_unlocked);
            Assert.That(m_progress.GetProgress(m_coins, m_saveData), Is.EqualTo(1975));
            Assert.That(m_saveData.IsAchievementUnlocked(m_coins.Id), Is.False);

            m_saveData.RecordCoin();
            m_progress.UnlockReached(m_saveData, m_unlocked);
            Assert.That(m_saveData.IsAchievementUnlocked(m_coins.Id), Is.True);
        }

        [Test]
        public void OwningEveryShipUnlocksItsAchievement()
        {
            m_saveData.AddCoins(1000);
            m_saveData.TryBuyShip(k_BoughtShip, 100);
            m_progress.UnlockReached(m_saveData, m_unlocked);
            Assert.That(m_progress.GetTarget(m_allShips), Is.EqualTo(s_ships.Length));
            Assert.That(m_progress.GetProgress(m_allShips, m_saveData), Is.EqualTo(2));
            Assert.That(m_saveData.IsAchievementUnlocked(m_allShips.Id), Is.False);

            m_saveData.TryBuyShip(k_LastShip, 100);
            m_progress.UnlockReached(m_saveData, m_unlocked);
            Assert.That(m_saveData.IsAchievementUnlocked(m_allShips.Id), Is.True);
        }

        // A new ship makes the target larger, but what is unlocked stays so.
        [Test]
        public void UnlockedStaysUnlocked()
        {
            m_saveData.AddCoins(1000);
            m_saveData.TryBuyShip(k_BoughtShip, 100);
            m_saveData.TryBuyShip(k_LastShip, 100);
            m_progress.UnlockReached(m_saveData, m_unlocked);

            string[] moreShips = { k_StarterShip, k_BoughtShip, k_LastShip, "NewShip" };
            AchievementProgress later = new AchievementProgress(m_achievements, moreShips);
            Assert.That(later.GetProgress(m_allShips, m_saveData), Is.LessThan(later.GetTarget(m_allShips)));
            Assert.That(later.GetState(m_allShips, m_saveData), Is.EqualTo(AchievementState.ToClaim));
        }

        [Test]
        public void ComingSoonNeverUnlocks()
        {
            m_progress.UnlockReached(m_saveData, m_unlocked);

            Assert.That(m_progress.GetState(m_logbooks, m_saveData), Is.EqualTo(AchievementState.ComingSoon));
            Assert.That(m_progress.GetProgress(m_logbooks, m_saveData), Is.Zero);
            Assert.That(m_progress.GetTarget(m_logbooks), Is.EqualTo(5));
            Assert.That(m_unlocked, Has.No.Member(m_logbooks));
        }

        // The one for all others unlocks in the same call as the last of the others, after it.
        [Test]
        public void AllOthersUnlocksWithTheLastOfThem()
        {
            AchievementProgress progress = new AchievementProgress(new[] { m_allOthers, m_firstFlight, m_crashes },
                s_ships);
            m_saveData.RecordFlight(false);
            progress.UnlockReached(m_saveData, m_unlocked);
            Assert.That(progress.GetProgress(m_allOthers, m_saveData), Is.EqualTo(1));
            Assert.That(progress.GetTarget(m_allOthers), Is.EqualTo(2));

            for (int i = 0; i < 3; i++)
            {
                m_saveData.RecordCrash(10);
            }

            progress.UnlockReached(m_saveData, m_unlocked);
            Assert.That(m_unlocked, Is.EqualTo(new[] { m_firstFlight, m_crashes, m_allOthers }));
        }

        // The collectibles are among the others, so the one for all of them waits for them.
        [Test]
        public void AllOthersWaitsForWhatComesLater()
        {
            m_saveData.AddCoins(1000);
            m_saveData.TryBuyShip(k_BoughtShip, 100);
            m_saveData.TryBuyShip(k_LastShip, 100);
            m_saveData.RecordFlight(true);
            for (int i = 0; i < 1976; i++)
            {
                m_saveData.RecordCoin();
            }

            for (int i = 0; i < 3; i++)
            {
                m_saveData.RecordCrash(1998);
            }

            m_progress.UnlockReached(m_saveData, m_unlocked);
            Assert.That(m_unlocked, Has.Count.EqualTo(6), "Every achievement but the last two");
            Assert.That(m_progress.GetProgress(m_allOthers, m_saveData), Is.EqualTo(6));
            Assert.That(m_progress.GetTarget(m_allOthers), Is.EqualTo(7));
            Assert.That(m_saveData.IsAchievementUnlocked(m_allOthers.Id), Is.False);
        }

        [Test]
        public void RewardIsPaidOutOnce()
        {
            m_saveData.RecordFlight(false);
            m_progress.UnlockReached(m_saveData, m_unlocked);

            Assert.That(m_saveData.TryClaimAchievement(m_firstFlight.Id, m_firstFlight.Reward), Is.True);
            Assert.That(m_saveData.Coins, Is.EqualTo(11));
            Assert.That(m_progress.GetState(m_firstFlight, m_saveData), Is.EqualTo(AchievementState.Claimed));
            Assert.That(m_progress.CountToClaim(m_saveData), Is.Zero);

            Assert.That(m_saveData.TryClaimAchievement(m_firstFlight.Id, m_firstFlight.Reward), Is.False);
            Assert.That(m_saveData.Coins, Is.EqualTo(11), "The reward was paid out twice");
        }

        [Test]
        public void LockedRewardCannotBeClaimed()
        {
            Assert.That(m_saveData.TryClaimAchievement(m_crashes.Id, m_crashes.Reward), Is.False);
            Assert.That(m_saveData.Coins, Is.Zero);
            Assert.That(m_progress.GetState(m_crashes, m_saveData), Is.EqualTo(AchievementState.InProgress));
        }

        [Test]
        public void ShareCountsUnlockedClaimedOrNot()
        {
            m_saveData.RecordFlight(true);
            m_progress.UnlockReached(m_saveData, m_unlocked);
            m_saveData.TryClaimAchievement(m_firstFlight.Id, m_firstFlight.Reward);

            Assert.That(m_progress.GetShareUnlocked(m_saveData), Is.EqualTo(2f / m_achievements.Count));
        }

        // Made like an asset, through the fields the Inspector shows.
        private AchievementData Create(string id, AchievementGoal goal, int target, int reward,
            bool isComingSoon = false)
        {
            AchievementData achievement = ScriptableObject.CreateInstance<AchievementData>();
            SerializedObject serialized = new SerializedObject(achievement);
            serialized.FindProperty("m_id").stringValue = id;
            serialized.FindProperty("m_goal").enumValueIndex = (int)goal;
            serialized.FindProperty("m_target").intValue = target;
            serialized.FindProperty("m_reward").intValue = reward;
            serialized.FindProperty("m_isComingSoon").boolValue = isComingSoon;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            m_achievements.Add(achievement);
            return achievement;
        }
    }
}
