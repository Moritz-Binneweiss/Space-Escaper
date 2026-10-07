using System.Collections;
using System.Collections.Generic;
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

        // Ships are numbered 1-9, three skins per family: ARISTOCRAT 1-3,
        // FREETER 4-6, VAGOR 7-9.
        private const int k_FamilyCount = 3;
        private const int k_SkinsPerFamily = 3;

        // Children of the shop sprite container. The price tag of ship n is
        // child n + 1.
        private const int k_SelectSprite = 0;
        private const int k_SelectedSprite = 1;
        private const int k_PriceTagOffset = 1;

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
        [Tooltip("Child 0 is the flame container, children 1-9 are the ships.")]
        [SerializeField] private Transform m_shipContainer;
        [Tooltip("Engine flames, one child per family.")]
        [SerializeField] private Transform m_flameContainer;

        [Header("Hangar Shop")]
        [SerializeField] private Animator m_shopAnimator;
        [SerializeField] private GameObject m_hangar;
        [Tooltip("Price per ship, index = ship - 1.")]
        [SerializeField] private List<int> m_shipPrices;
        [Tooltip("Select or buy button per ship, index = ship - 1.")]
        [SerializeField] private Transform m_buttonContainer;
        [Tooltip("Hangar model per ship, index = ship - 1.")]
        [SerializeField] private Transform m_shopShipContainer;
        [Tooltip("0 = Select, 1 = Selected, ship + 1 = price tag.")]
        [SerializeField] private Transform m_shopSpriteContainer;
        [Tooltip("Skin buttons, one child per family.")]
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

        private int m_currentShip;

        // The family of the ship being flown. Flames and everything in the run
        // belong to it, not to the family viewed in the hangar.
        private int m_currentFamily;
        private int m_selectedFamily;

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
            ShowOnlyChild(m_buttonContainer, m_currentShip - 1);
            ShowOnlyChild(m_shopShipContainer, m_currentShip - 1);
            ShowOnlyChild(m_shopSpriteContainer, k_SelectedSprite);
            ShowOnlyChild(m_skinButtonContainer, m_currentFamily);
            HideAllChildren(m_flameContainer);

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
            m_flameContainer.GetChild(m_currentFamily).gameObject.SetActive(true);
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

            m_shipContainer.GetChild(m_currentShip).GetComponent<Renderer>().enabled = false;
            m_flameContainer.GetChild(m_currentFamily).gameObject.SetActive(false);

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
            m_selectedFamily = (m_selectedFamily + k_FamilyCount - 1) % k_FamilyCount;
            ShowSelectedFamily();
        }

        public void ShowNextFamily()
        {
            m_selectedFamily = (m_selectedFamily + 1) % k_FamilyCount;
            ShowSelectedFamily();
        }

        /// <summary>
        /// Shows a ship in the hangar: its model, its button, and either its price
        /// tag or whether it is already the one being flown.
        /// </summary>
        public void ShowShip(int ship)
        {
            ShowOnlyChild(m_buttonContainer, ship - 1);
            ShowOnlyChild(m_shopShipContainer, ship - 1);
            ShowOnlyChild(m_shopSpriteContainer, GetShopSprite(ship));
        }

        /// <summary>
        /// Flies an owned ship, or buys one the player can afford and flies it.
        /// </summary>
        public void SelectOrBuyShip(int ship)
        {
            SaveData saveData = SaveSystem.Data;
            if (saveData.IsShipUnlocked(ship))
            {
                AudioSystem.Instance.PlayShipSelect();
                SelectShip(ship);
                return;
            }

            if (!saveData.TryBuyShip(ship, m_shipPrices[ship - 1]))
            {
                return;
            }

            AudioSystem.Instance.PlayShipPurchase();
            m_menuCoinText.text = saveData.Coins.ToString();
            SelectShip(ship);
        }

        private void LoadShipSelection()
        {
            SaveData saveData = SaveSystem.Data;

            // Child 0 of the ship container is the flame container, the ships follow.
            saveData.RepairShips(m_shipContainer.childCount - 1);

            m_currentShip = saveData.CurrentShip;
            m_currentFamily = GetFamilyOfShip(m_currentShip);
            m_selectedFamily = m_currentFamily;
        }

        private void Revive()
        {
            m_deathMenuAnimator.SetTrigger(k_AliveTrigger);
            m_gameMenuAnimator.SetTrigger(k_ShowTrigger);
            m_score = m_reviveScore;
            m_shipContainer.GetChild(m_currentShip).GetComponent<Renderer>().enabled = true;
            m_flameContainer.GetChild(m_currentFamily).gameObject.SetActive(true);
            m_playerMotor.StartRunning();
            AudioSystem.Instance.ResumeMusic();
        }

        private void ShowSelectedFamily()
        {
            ShowOnlyChild(m_skinButtonContainer, m_selectedFamily);
            ShowShip(GetFirstShipOfFamily(m_selectedFamily));
        }

        private void SelectShip(int ship)
        {
            ShowShipModel(ship);
            HideAllChildren(m_flameContainer);

            m_currentShip = ship;
            m_currentFamily = m_selectedFamily;
            SaveSystem.Data.SelectShip(ship);

            // Written to disk right away rather than when the app is paused or
            // closed. After a purchase, that also keeps the coins spent.
            SaveSystem.Save();

            ShowOnlyChild(m_shopSpriteContainer, k_SelectedSprite);
        }

        private void ShowShipModel(int ship)
        {
            // Child 0 is the flame container, which stays visible with every ship.
            ShowOnlyChild(m_shipContainer, ship);
            m_shipContainer.GetChild(0).gameObject.SetActive(true);
        }

        private int GetShopSprite(int ship)
        {
            if (!SaveSystem.Data.IsShipUnlocked(ship))
            {
                return ship + k_PriceTagOffset;
            }

            return ship == m_currentShip ? k_SelectedSprite : k_SelectSprite;
        }

        private IEnumerator HideSpaceportAfterDelay()
        {
            yield return new WaitForSeconds(k_SpaceportHideDelay);
            m_spaceport.SetActive(false);
        }

        private static int GetFirstShipOfFamily(int family)
        {
            return family * k_SkinsPerFamily + 1;
        }

        private static int GetFamilyOfShip(int ship)
        {
            return (ship - 1) / k_SkinsPerFamily;
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
