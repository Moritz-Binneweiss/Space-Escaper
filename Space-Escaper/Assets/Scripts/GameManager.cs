using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// Runs the main menu, the hangar shop, the score of a run and the death screen.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private const string k_GameSceneName = "Game";
        private const string k_PlayerTag = "Player";

        private const string k_ShowTrigger = "Show";
        private const string k_HideTrigger = "Hide";
        private const string k_CollectTrigger = "Collect";
        private const string k_DeadTrigger = "Dead";

        // Spelled this way in the death menu's Animator Controller.
        private const string k_AliveTrigger = "Allive";

        private const float k_ScorePerSecond = 3f;
        private const int k_ScorePerCoin = 1;
        private const float k_SpaceportHideDelay = 3f;

        [Header("Main Menu")]
        [SerializeField] private Animator m_mainMenuAnimator;
        [SerializeField] private Text m_menuCoinText;
        [SerializeField] private Text m_highscoreText;

        [Header("Run")]
        [SerializeField] private Animator m_gameMenuAnimator;
        [SerializeField] private Animator m_coinAnimator;
        [SerializeField] private Text m_scoreText;
        [SerializeField] private Text m_coinText;
        [SerializeField] private Text m_modifierText;
        [Tooltip("Hidden a few seconds after the run starts.")]
        [SerializeField] private GameObject m_spaceport;

        [Header("Ship")]
        [SerializeField] private ShipCatalog m_shipCatalog;
        [Tooltip("The flown ship. Its model and engine flame are created here.")]
        [SerializeField] private Transform m_shipContainer;

        [Header("Hangar Shop")]
        [SerializeField] private Animator m_shopAnimator;
        [SerializeField] private GameObject m_hangar;
        [Tooltip("The model of the ship shown in the hangar is created here.")]
        [SerializeField] private Transform m_shopShipContainer;
        [Tooltip("Graphic of the select or buy button: Select, Selected or the price tag of the shown ship.")]
        [SerializeField] private Image m_selectOrBuyImage;
        [SerializeField] private Sprite m_selectSprite;
        [SerializeField] private Sprite m_selectedSprite;
        [Tooltip("Skin buttons, one child per family in the order of ShipFamily.")]
        [SerializeField] private Transform m_skinButtonContainer;

        [Header("Death Menu")]
        [SerializeField] private Animator m_deathMenuAnimator;
        [SerializeField] private Text m_deathScoreText;
        [SerializeField] private Text m_deathCoinText;
        [SerializeField] private GameObject m_reviveButton;

        private PlayerMotor m_playerMotor;
        private bool m_isGameStarted;

        private float m_score;
        private int m_displayedScore;
        private float m_scoreModifier;
        private float m_reviveScore;
        private readonly RunCoins m_runCoins = new RunCoins();

        // The ship being flown. Flames and everything in the run belong to it, not
        // to the ship viewed in the hangar.
        private ShipData m_currentShip;
        private ShipData m_shownShip;
        private GameObject m_shipModel;
        private GameObject m_engineFlame;
        private GameObject m_hangarModel;

        private static readonly int s_familyCount = System.Enum.GetValues(typeof(ShipFamily)).Length;

        public static GameManager Instance { get; private set; }

        /// <summary>
        /// True while a run is going on and the ship is alive - the only time a pause
        /// makes sense. False in the menu, in the hangar and on the death screen.
        /// </summary>
        public bool IsRunActive => m_isGameStarted && m_playerMotor.IsRunning;

        private void Awake()
        {
            Time.timeScale = 1f;
            Instance = this;
            m_scoreModifier = 1f;
            m_playerMotor = GameObject.FindGameObjectWithTag(k_PlayerTag).GetComponent<PlayerMotor>();

            m_modifierText.text = FormatModifier(m_scoreModifier);
            m_coinText.text = m_runCoins.Count.ToString();
            m_scoreText.text = m_score.ToString("0");

            SaveData saveData = SaveSystem.Data;
            m_menuCoinText.text = saveData.Coins.ToString();
            m_highscoreText.text = saveData.Highscore.ToString();

            LoadShipSelection();
            ShowShipModel(m_currentShip);
            ShowOnlyChild(m_skinButtonContainer, (int)m_currentShip.Family);
            ShowShip(m_currentShip);

            m_reviveButton.SetActive(true);
            m_spaceport.SetActive(true);
            m_hangar.SetActive(false);
        }

        private void Update()
        {
            if (!m_isGameStarted)
            {
                return;
            }

            m_score += Time.deltaTime * m_scoreModifier * k_ScorePerSecond;
            if (m_displayedScore != (int)m_score)
            {
                m_displayedScore = (int)m_score;
                m_scoreText.text = m_displayedScore.ToString();
            }
        }

        /// <summary>
        /// Starts a run from the main menu.
        /// </summary>
        public void Play()
        {
            m_isGameStarted = true;
            AudioSystem.Instance.PlayGameMusic();
            m_playerMotor.StartRunning();
            FindAnyObjectByType<CameraMotor>().IsMoving = true;
            m_gameMenuAnimator.SetTrigger(k_ShowTrigger);
            m_mainMenuAnimator.SetTrigger(k_HideTrigger);
            m_engineFlame.SetActive(true);
            StartCoroutine(HideSpaceportAfterDelay());
        }

        public void CollectCoin()
        {
            m_coinAnimator.SetTrigger(k_CollectTrigger);
            m_runCoins.Collect();
            m_coinText.text = m_runCoins.Count.ToString();
            m_score += k_ScorePerCoin;
            m_scoreText.text = ((int)m_score).ToString();
        }

        public void UpdateModifier(float modifierAmount)
        {
            m_scoreModifier = 1f + modifierAmount;
            m_modifierText.text = FormatModifier(m_scoreModifier);
        }

        /// <summary>
        /// Reloads the scene, which brings the game back to the main menu.
        /// </summary>
        public void ReturnToMenu()
        {
            SceneManager.LoadScene(k_GameSceneName);
        }

        /// <summary>
        /// Shows the death screen, banks the coins of the run and saves a new
        /// highscore.
        /// </summary>
        public void HandleDeath()
        {
            // Shown and saved as the same whole number, so the death screen and the
            // highscore cannot disagree (rounding 41.7 shows 42, truncating saves 41).
            int finalScore = (int)m_score;

            m_gameMenuAnimator.SetTrigger(k_HideTrigger);
            m_deathScoreText.text = finalScore.ToString();
            m_deathCoinText.text = m_runCoins.Count.ToString();
            m_deathMenuAnimator.SetTrigger(k_DeadTrigger);

            // Paused, not stopped, so a revive continues the track where it was.
            // Going back to the menu reloads the scene, which starts the menu music.
            AudioSystem.Instance.PauseMusic();

            SaveData saveData = SaveSystem.Data;
            m_runCoins.BankInto(saveData);

            m_reviveScore = m_score;

            m_shipModel.SetActive(false);
            m_engineFlame.SetActive(false);

            saveData.RecordScore(finalScore);
            SaveSystem.Save();
        }

        /// <summary>
        /// Revives the ship. There is no cost and no ad behind it at the moment.
        /// </summary>
        public void RequestRevive()
        {
            m_reviveButton.SetActive(false);
            Revive();
        }

        public void OpenShop()
        {
            m_mainMenuAnimator.SetTrigger(k_HideTrigger);
            m_shopAnimator.SetTrigger(k_ShowTrigger);
            m_hangar.SetActive(true);
        }

        public void CloseShop()
        {
            m_mainMenuAnimator.SetTrigger(k_ShowTrigger);
            m_shopAnimator.SetTrigger(k_HideTrigger);
            m_hangar.SetActive(false);
        }

        public void ShowPreviousFamily()
        {
            ShowFamily(GetAdjacentFamily(-1));
        }

        public void ShowNextFamily()
        {
            ShowFamily(GetAdjacentFamily(1));
        }

        /// <summary>
        /// Shows a ship in the hangar: its model, and either its price tag or
        /// whether it is already the one being flown.
        /// </summary>
        public void ShowShip(ShipData ship)
        {
            m_shownShip = ship;
            ShowHangarModel(ship);
            m_selectOrBuyImage.sprite = GetSelectOrBuySprite(ship);
        }

        /// <summary>
        /// Flies the ship shown in the hangar if it is owned, or buys it if the
        /// player can afford it and flies it.
        /// </summary>
        public void SelectOrBuyShownShip()
        {
            SaveData saveData = SaveSystem.Data;
            int shipNumber = GetShipNumber(m_shownShip);
            if (saveData.IsShipUnlocked(shipNumber))
            {
                AudioSystem.Instance.PlayShipSelect();
                SelectShip(m_shownShip);
                return;
            }

            if (!saveData.TryBuyShip(shipNumber, m_shownShip.Price))
            {
                return;
            }

            AudioSystem.Instance.PlayShipPurchase();
            m_menuCoinText.text = saveData.Coins.ToString();
            SelectShip(m_shownShip);
        }

        private void LoadShipSelection()
        {
            SaveData saveData = SaveSystem.Data;
            saveData.RepairShips(m_shipCatalog.Ships.Count);
            m_currentShip = m_shipCatalog.Ships[saveData.CurrentShip - 1];
        }

        private void Revive()
        {
            m_deathMenuAnimator.SetTrigger(k_AliveTrigger);
            m_gameMenuAnimator.SetTrigger(k_ShowTrigger);
            m_score = m_reviveScore;
            m_shipModel.SetActive(true);
            m_engineFlame.SetActive(true);
            m_playerMotor.StartRunning();
            AudioSystem.Instance.ResumeMusic();
        }

        private void ShowFamily(ShipFamily family)
        {
            ShowOnlyChild(m_skinButtonContainer, (int)family);
            ShowShip(m_shipCatalog.GetFirstShipOfFamily(family));
        }

        private void SelectShip(ShipData ship)
        {
            ShowShipModel(ship);

            m_currentShip = ship;
            SaveSystem.Data.SelectShip(GetShipNumber(ship));

            // Written to disk right away rather than when the app is paused or
            // closed. After a purchase, that also keeps the coins spent.
            SaveSystem.Save();

            m_selectOrBuyImage.sprite = m_selectedSprite;
        }

        // The engine flame of every ship is switched the same way: off until a run
        // starts, on until the crash, and on again after a revive.
        private void ShowShipModel(ShipData ship)
        {
            if (m_shipModel != null)
            {
                Destroy(m_shipModel);
                Destroy(m_engineFlame);
            }

            m_shipModel = Instantiate(ship.Model, m_shipContainer);
            m_engineFlame = Instantiate(ship.EngineFlame, m_shipContainer);
            m_engineFlame.SetActive(false);
        }

        private void ShowHangarModel(ShipData ship)
        {
            if (m_hangarModel != null)
            {
                Destroy(m_hangarModel);
            }

            // The model prefab carries its place on the flown ship. The hangar
            // shows it on its stand instead, at a size of its own.
            m_hangarModel = Instantiate(ship.Model, m_shopShipContainer);
            m_hangarModel.transform.localPosition = Vector3.zero;
            m_hangarModel.transform.localScale = ship.HangarScale;
        }

        private Sprite GetSelectOrBuySprite(ShipData ship)
        {
            if (!SaveSystem.Data.IsShipUnlocked(GetShipNumber(ship)))
            {
                return ship.PriceTag;
            }

            return ship == m_currentShip ? m_selectedSprite : m_selectSprite;
        }

        // Wraps around at both ends of the hangar.
        private ShipFamily GetAdjacentFamily(int step)
        {
            return (ShipFamily)(((int)m_shownShip.Family + step + s_familyCount) % s_familyCount);
        }

        // The save still counts the ships by their place in the catalog, starting
        // at 1.
        private int GetShipNumber(ShipData ship)
        {
            for (int i = 0; i < m_shipCatalog.Ships.Count; i++)
            {
                if (m_shipCatalog.Ships[i] == ship)
                {
                    return i + 1;
                }
            }

            throw new System.ArgumentException($"{ship.name} is not in the ship catalog.");
        }

        private IEnumerator HideSpaceportAfterDelay()
        {
            yield return new WaitForSeconds(k_SpaceportHideDelay);
            m_spaceport.SetActive(false);
        }

        private static string FormatModifier(float modifier)
        {
            return "x" + modifier.ToString("0.0");
        }

        private static void ShowOnlyChild(Transform container, int index)
        {
            HideAllChildren(container);
            container.GetChild(index).gameObject.SetActive(true);
        }

        private static void HideAllChildren(Transform container)
        {
            foreach (Transform child in container)
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}
