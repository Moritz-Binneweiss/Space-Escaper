using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Counts what the achievements need while the game goes on, unlocks the ones the save has reached and pays out
    /// their rewards. It remembers what the current run has unlocked, for the death screen.
    /// </summary>
    public class AchievementTracker : MonoBehaviour
    {
        [SerializeField] private AchievementCatalog m_achievementCatalog;
        [Tooltip("The starter ship is the one not bought, and owning all ships is an achievement.")]
        [SerializeField] private ShipCatalog m_shipCatalog;
        [Tooltip("Brings the distance of the run at a crash.")]
        [SerializeField] private RunScore m_runScore;

        private readonly List<AchievementData> m_unlockedThisRun = new List<AchievementData>();
        private readonly List<AchievementData> m_newlyUnlocked = new List<AchievementData>();
        private AchievementProgress m_progress;

        public IReadOnlyList<AchievementData> Achievements => m_achievementCatalog.Achievements;

        /// <summary>
        /// What the run has unlocked so far, in the order it came, also before a revive.
        /// </summary>
        public IReadOnlyList<AchievementData> UnlockedThisRun => m_unlockedThisRun;

        /// <summary>
        /// Achievements unlocked whose reward is not claimed yet.
        /// </summary>
        public int RewardsToClaim => Progress.CountToClaim(SaveSystem.Data);

        /// <summary>
        /// The share of all achievements unlocked, claimed or not, from 0 to 1.
        /// </summary>
        public float ShareUnlocked => Progress.GetShareUnlocked(SaveSystem.Data);

        // The scene wakes its objects in no set order, so a screen may ask before Awake would have run here.
        private AchievementProgress Progress
        {
            get
            {
                if (m_progress == null)
                {
                    m_progress = new AchievementProgress(m_achievementCatalog.Achievements, m_shipCatalog.GetShipIds());
                }

                return m_progress;
            }
        }

        /// <summary>
        /// Raised when achievements unlock, once the save holds them. A screen that shows them may already be on.
        /// </summary>
        public event Action Unlocked;

        private void OnEnable()
        {
            GameManager.RunStarted += GameManager_RunStarted;
            GameManager.ShipCrashed += GameManager_ShipCrashed;
            GameManager.HangarClosed += GameManager_HangarClosed;
            Coin.Collected += Coin_Collected;
        }

        private void OnDisable()
        {
            GameManager.RunStarted -= GameManager_RunStarted;
            GameManager.ShipCrashed -= GameManager_ShipCrashed;
            GameManager.HangarClosed -= GameManager_HangarClosed;
            Coin.Collected -= Coin_Collected;
        }

        // A save can have reached achievements that nothing has unlocked yet: one from before them with every ship
        // bought, or one where the app was closed in the hangar right after buying the last ship.
        private void Start()
        {
            if (UnlockReached())
            {
                SaveSystem.Save();
            }
        }

        public AchievementState GetState(AchievementData achievement)
        {
            return Progress.GetState(achievement, SaveSystem.Data);
        }

        public int GetProgress(AchievementData achievement)
        {
            return Progress.GetProgress(achievement, SaveSystem.Data);
        }

        public int GetTarget(AchievementData achievement)
        {
            return Progress.GetTarget(achievement);
        }

        /// <summary>
        /// Pays out the reward of an unlocked achievement, once, and writes it to disk right away like a purchase.
        /// </summary>
        public bool TryClaim(AchievementData achievement)
        {
            if (!SaveSystem.Data.TryClaimAchievement(achievement.Id, achievement.Reward))
            {
                return false;
            }

            SaveSystem.Save();
            return true;
        }

        // Written to disk right away, so the flight counts even if the app is closed during the run.
        private void GameManager_RunStarted()
        {
            m_unlockedThisRun.Clear();
            SaveData saveData = SaveSystem.Data;
            saveData.RecordFlight(saveData.CurrentShipId != m_shipCatalog.StarterShip.Id);
            UnlockReached();
            SaveSystem.Save();
        }

        private void Coin_Collected()
        {
            SaveSystem.Data.RecordCoin();
        }

        // RunScore adds the coins and the highscore of the crash as well and saves them itself, so neither needs to
        // come first.
        private void GameManager_ShipCrashed()
        {
            SaveSystem.Data.RecordCrash(m_runScore.Distance);
            UnlockReached();
            SaveSystem.Save();
        }

        // Only the hangar sells the last ship.
        private void GameManager_HangarClosed()
        {
            if (UnlockReached())
            {
                SaveSystem.Save();
            }
        }

        private bool UnlockReached()
        {
            m_newlyUnlocked.Clear();
            Progress.UnlockReached(SaveSystem.Data, m_newlyUnlocked);
            if (m_newlyUnlocked.Count == 0)
            {
                return false;
            }

            m_unlockedThisRun.AddRange(m_newlyUnlocked);
            OnUnlocked();
            return true;
        }

        private void OnUnlocked() => Unlocked?.Invoke();
    }
}
