using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Counts the score and the coins of a run. When the ship crashes, it adds the coins to the save and records the
    /// highscore.
    /// </summary>
    public class RunScore : MonoBehaviour
    {
        private const float k_ScorePerSecond = 3f;
        private const int k_ScorePerCoin = 1;

        [SerializeField] private GameManager m_gameManager;
        [Tooltip("The faster it flies, the more points a second brings.")]
        [SerializeField] private PlayerMotor m_ship;

        private readonly RunCoins m_runCoins = new RunCoins();
        private float m_score;

        /// <summary>
        /// Whole points, cut off rather than rounded, so the screens and the highscore cannot disagree. Rounding would
        /// show 42 for 41.7 and save 41.
        /// </summary>
        public int Score => (int)m_score;

        public int Coins => m_runCoins.Count;

        /// <summary>
        /// Points per second compared to the start of the run: 1 plus the speed the ship has gained since.
        /// </summary>
        public float Modifier => 1f + m_ship.SpeedGain;

        private void OnEnable()
        {
            Coin.Collected += Coin_Collected;
            GameManager.ShipCrashed += GameManager_ShipCrashed;
        }

        private void OnDisable()
        {
            Coin.Collected -= Coin_Collected;
            GameManager.ShipCrashed -= GameManager_ShipCrashed;
        }

        private void Update()
        {
            if (m_gameManager.State != GameState.Run)
            {
                return;
            }

            m_score += Time.deltaTime * Modifier * k_ScorePerSecond;
        }

        private void Coin_Collected()
        {
            m_runCoins.Collect();
            m_score += k_ScorePerCoin;
        }

        // Written to disk right away, so coins and highscore are kept even if the app is closed on the death screen.
        private void GameManager_ShipCrashed()
        {
            SaveData saveData = SaveSystem.Data;
            m_runCoins.BankInto(saveData);
            saveData.RecordScore(Score);
            SaveSystem.Save();
        }
    }
}
