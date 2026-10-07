using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Everything the game keeps between sessions: coins, highscore, ships and audio
    /// settings. <see cref="SaveSystem"/> stores it as one JSON file.
    /// </summary>
    /// <remarks>
    /// The field names are the keys in the file. A renamed field loses its value in
    /// existing saves, unless it gets [FormerlySerializedAs].
    /// </remarks>
    [Serializable]
    public class SaveData
    {
        public const int k_StarterShip = 1;

        // Written into every save, so a later version that changes the fields can
        // tell an older save and convert it while loading.
        private const int k_CurrentVersion = 1;

        [SerializeField] private int m_version = k_CurrentVersion;
        [SerializeField] private int m_coins;
        [SerializeField] private int m_highscore;
        [SerializeField] private int m_currentShip = k_StarterShip;
        [SerializeField] private List<int> m_unlockedShips = new List<int> { k_StarterShip };
        [SerializeField] private bool m_useNewSounds = true;
        [SerializeField] private bool m_isMusicMuted;
        [SerializeField] private bool m_isSfxMuted;
        [SerializeField] private float m_masterVolume = 1f;

        public int Coins => m_coins;
        public int Highscore => m_highscore;
        public int CurrentShip => m_currentShip;

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
        /// Position of the volume slider from 0 to 1, not the amplitude, so the
        /// handle comes back where the player left it.
        /// </summary>
        public float MasterVolume
        {
            get => m_masterVolume;
            set => m_masterVolume = value;
        }

        /// <summary>
        /// Reads a save from JSON. Fields missing in the JSON keep their defaults, so
        /// null or an empty string gives the save of a new installation.
        /// </summary>
        public static SaveData FromJson(string json)
        {
            SaveData saveData = new SaveData();
            if (!string.IsNullOrEmpty(json))
            {
                JsonUtility.FromJsonOverwrite(json, saveData);
            }

            return saveData;
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        public void AddCoins(int coins)
        {
            m_coins += coins;
        }

        public void SpendCoins(int coins)
        {
            m_coins -= coins;
        }

        /// <summary>
        /// Keeps the score of a run as the new highscore if it beats the old one.
        /// </summary>
        public void RecordScore(int score)
        {
            m_highscore = Mathf.Max(m_highscore, score);
        }

        public bool IsShipUnlocked(int ship)
        {
            return m_unlockedShips.Contains(ship);
        }

        public void UnlockShip(int ship)
        {
            if (!IsShipUnlocked(ship))
            {
                m_unlockedShips.Add(ship);
            }
        }

        public void SelectShip(int ship)
        {
            m_currentShip = ship;
        }
    }
}
