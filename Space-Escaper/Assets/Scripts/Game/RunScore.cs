using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Counts the score, the coins, the time and the distance of a run. When the ship crashes, it adds the coins to the
    /// save and records the highscore.
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
        private float m_flightTime;
        private float m_distance;
        private int m_previousHighscore;

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

        /// <summary>
        /// Seconds of flight in this run, without the pause and the death screen.
        /// </summary>
        public float FlightTime => m_flightTime;

        /// <summary>
        /// Units flown in this run, shown as meters. Whole ones, cut off like the score.
        /// </summary>
        public int Distance => (int)m_distance;

        /// <summary>
        /// The highscore when the run started. From the first crash on, the save already holds the score of this run.
        /// </summary>
        public int PreviousHighscore => m_previousHighscore;

        /// <summary>
        /// The best score so far, this run included.
        /// </summary>
        public int Highscore => Mathf.Max(m_previousHighscore, Score);

        /// <summary>
        /// Whether this run has beaten the highscore it started with. The very first run has none to beat.
        /// </summary>
        public bool IsNewHighscore => m_previousHighscore > 0 && Score > m_previousHighscore;

        private void OnEnable()
        {
            Coin.Collected += Coin_Collected;
            GameManager.RunStarted += GameManager_RunStarted;
            GameManager.ShipCrashed += GameManager_ShipCrashed;
        }

        private void OnDisable()
        {
            Coin.Collected -= Coin_Collected;
            GameManager.RunStarted -= GameManager_RunStarted;
            GameManager.ShipCrashed -= GameManager_ShipCrashed;
        }

        // The distance adds up what the ship flies each frame rather than reading its position, which the floating
        // origin moves back every few thousand units.
        private void Update()
        {
            if (m_gameManager.State != GameState.Run)
            {
                return;
            }

            m_score += Time.deltaTime * Modifier * k_ScorePerSecond;
            m_flightTime += Time.deltaTime;
            m_distance += Time.deltaTime * m_ship.Speed;
        }

        private void GameManager_RunStarted()
        {
            m_previousHighscore = SaveSystem.Data.Highscore;
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
