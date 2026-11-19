using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Everything the game keeps between sessions: coins, highscore, ships, audio
    /// settings and the achievements. <see cref="SaveSystem"/> stores it as one JSON file.
    /// </summary>
    /// <remarks>
    /// The field names are the keys in the file. A renamed field loses its value in
    /// existing saves, unless it gets [FormerlySerializedAs].
    /// </remarks>
    [Serializable]
    public class SaveData
    {
        // Written into every save, so the game can tell a save of another version.
        private const int k_CurrentVersion = 2;

        [SerializeField] private int m_version = k_CurrentVersion;
        [SerializeField] private int m_coins;
        [SerializeField] private int m_highscore;
        [SerializeField] private string m_currentShipId = "";
        [SerializeField] private List<string> m_unlockedShipIds = new List<string>();
        [SerializeField] private bool m_useNewSounds = true;
        [SerializeField] private bool m_isMusicMuted;
        [SerializeField] private bool m_isSfxMuted;
        [SerializeField] private float m_musicVolume = 1f;
        [SerializeField] private float m_sfxVolume = 1f;
        [SerializeField] private int m_flights;
        [SerializeField] private int m_flightsInBoughtShips;
        [SerializeField] private int m_crashes;
        [SerializeField] private int m_longestRun;
        [SerializeField] private int m_coinsCollected;
        [SerializeField] private List<string> m_unlockedAchievementIds = new List<string>();
        [SerializeField] private List<string> m_claimedAchievementIds = new List<string>();

        public int Coins => m_coins;
        public int Highscore => m_highscore;
        public string CurrentShipId => m_currentShipId;

        /// <summary>
        /// Runs started, from the main menu or with Play again. A revive goes on with the run.
        /// </summary>
        public int Flights => m_flights;

        /// <summary>
        /// Runs started in a ship other than the starter ship, so in one that was bought.
        /// </summary>
        public int FlightsInBoughtShips => m_flightsInBoughtShips;

        public int Crashes => m_crashes;

        /// <summary>
        /// Meters of the longest run so far.
        /// </summary>
        public int LongestRun => m_longestRun;

        /// <summary>
        /// Coins collected in runs, without the rewards of achievements.
        /// </summary>
        public int CoinsCollected => m_coinsCollected;

        public bool UseNewSounds
        {
            get => m_useNewSounds;
            set => m_useNewSounds = value;
        }

        public bool IsMusicMuted
        {
            get => m_isMusicMuted;
            set => m_isMusicMuted = value;
        }

        public bool IsSfxMuted
        {
            get => m_isSfxMuted;
            set => m_isSfxMuted = value;
        }

        /// <summary>
        /// Position of the music slider from 0 to 1, not the amplitude, so the
        /// handle comes back where the player left it.
        /// </summary>
        public float MusicVolume
        {
            get => m_musicVolume;
            set => m_musicVolume = value;
        }

        /// <summary>
        /// Position of the slider for the sound effects, like <see cref="MusicVolume"/>.
        /// </summary>
        public float SfxVolume
        {
            get => m_sfxVolume;
            set => m_sfxVolume = value;
        }

        /// <summary>
        /// Reads a save from JSON. Fields missing in the JSON keep their defaults, so
        /// null or an empty string gives the save of a new installation, and so does a
        /// save of another version.
        /// </summary>
        /// <remarks>
        /// Saves of other versions start over instead of being converted, as long as
        /// there are only test saves. From the store release on, convert them here.
        /// </remarks>
        public static SaveData FromJson(string json)
        {
            SaveData saveData = new SaveData();
            if (string.IsNullOrEmpty(json))
            {
                return saveData;
            }

            JsonUtility.FromJsonOverwrite(json, saveData);
            return saveData.m_version == k_CurrentVersion ? saveData : new SaveData();
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        public void AddCoins(int coins)
        {
            m_coins += coins;
        }

        /// <summary>
        /// Keeps the score of a run as the new highscore if it beats the old one.
        /// </summary>
        public void RecordScore(int score)
        {
            m_highscore = Mathf.Max(m_highscore, score);
        }

        public bool IsShipUnlocked(string shipId)
        {
            return m_unlockedShipIds.Contains(shipId);
        }

        /// <summary>
        /// Buys a ship that is not owned yet, if the coins are enough. Spends the
        /// coins and unlocks the ship, both or neither.
        /// </summary>
        public bool TryBuyShip(string shipId, int price)
        {
            if (IsShipUnlocked(shipId) || m_coins < price)
            {
                return false;
            }

            m_coins -= price;
            UnlockShip(shipId);
            return true;
        }

        public void SelectShip(string shipId)
        {
            m_currentShipId = shipId;
        }

        public void RecordFlight(bool isBoughtShip)
        {
            m_flights++;
            if (isBoughtShip)
            {
                m_flightsInBoughtShips++;
            }
        }

        /// <summary>
        /// Counts a coin collected in a run, as it is collected.
        /// </summary>
        public void RecordCoin()
        {
            m_coinsCollected++;
        }

        /// <summary>
        /// Counts a crash and keeps the distance of the run if it is the longest. A revive goes on with the run, so
        /// its next crash brings the distance of the whole run.
        /// </summary>
        public void RecordCrash(int runDistance)
        {
            m_crashes++;
            m_longestRun = Mathf.Max(m_longestRun, runDistance);
        }

        public bool IsAchievementUnlocked(string achievementId)
        {
            return m_unlockedAchievementIds.Contains(achievementId);
        }

        public bool IsAchievementClaimed(string achievementId)
        {
            return m_claimedAchievementIds.Contains(achievementId);
        }

        public void UnlockAchievement(string achievementId)
        {
            if (!IsAchievementUnlocked(achievementId))
            {
                m_unlockedAchievementIds.Add(achievementId);
            }
        }

        /// <summary>
        /// Pays out the reward of an unlocked achievement, once.
        /// </summary>
        public bool TryClaimAchievement(string achievementId, int reward)
        {
            if (!IsAchievementUnlocked(achievementId) || IsAchievementClaimed(achievementId))
            {
                return false;
            }

            m_claimedAchievementIds.Add(achievementId);
            m_coins += reward;
            return true;
        }

        /// <summary>
        /// Puts the ships of a loaded save in order: an unknown ship falls back to
        /// the starter ship, and the starter ship and the ship being flown always
        /// count as bought. A new save gets the starter ship this way.
        /// </summary>
        /// <remarks>
        /// An unknown ship would leave the game without a ship to show, and a ship
        /// flown without being in the list would be locked again after switching to
        /// another one.
        /// </remarks>
        public void RepairShips(string starterShipId, ICollection<string> knownShipIds)
        {
            if (!knownShipIds.Contains(m_currentShipId))
            {
                m_currentShipId = starterShipId;
            }

            UnlockShip(starterShipId);
            UnlockShip(m_currentShipId);
        }

        private void UnlockShip(string shipId)
        {
            if (!IsShipUnlocked(shipId))
            {
                m_unlockedShipIds.Add(shipId);
            }
        }
    }
}
