using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The hangar screen. It shows one ship at a time: the skin buttons and arrows pick it, and
    /// the button below it flies it if it is owned, or buys it. The coins are on show, and the
    /// skin buttons mark the ship on the stand, the ships owned and the one being flown.
    /// </summary>
    public class ShopScreen : MonoBehaviour
    {
        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private ScreenManager m_screens;
        [SerializeField] private ShipCatalog m_shipCatalog;
        [SerializeField] private PlayerShip m_playerShip;
        [Tooltip("Shows the ship on its stand.")]
        [SerializeField] private Hangar m_hangar;
        [SerializeField] private TMP_Text m_coinText;
        [SerializeField] private TMP_Text m_familyNameText;
        [SerializeField] private Button m_backButton;
        [SerializeField] private Button m_previousFamilyButton;
        [SerializeField] private Button m_nextFamilyButton;
        [SerializeField] private Button m_selectOrBuyButton;
        [Tooltip("The look of the select or buy button: the price of the shown ship, Select or Selected.")]
        [SerializeField] private SelectOrBuyButton m_selectOrBuyLook;
        [Tooltip("Skin buttons, one child per family in the order of ShipFamily.")]
        [SerializeField] private Transform m_skinButtonContainer;

        private ShipButton[] m_shipButtons;
        private ShipData m_shownShip;

        private static readonly int s_familyCount = Enum.GetValues(typeof(ShipFamily)).Length;

        // For tests, which tap them like a player.
        internal Button BackButton => m_backButton;

        internal Button NextFamilyButton => m_nextFamilyButton;

        internal SelectOrBuyButton SelectOrBuyLook => m_selectOrBuyLook;

        // For tests, which check when the family changes.
        internal ShipData ShownShip => m_shownShip;

        // The hangar opens on the ship being flown. What it shows then stays until the scene
        // reloads.
        private void Awake()
        {
            m_shipButtons = m_skinButtonContainer.GetComponentsInChildren<ShipButton>(true);
            ShowOnlyChild(m_skinButtonContainer, (int)m_playerShip.Ship.Family);
            ShowShip(m_playerShip.Ship);
        }

        private void OnEnable()
        {
            m_backButton.onClick.AddListener(BackButton_Clicked);
            m_previousFamilyButton.onClick.AddListener(ShowPreviousFamily);
            m_nextFamilyButton.onClick.AddListener(ShowNextFamily);
            m_selectOrBuyButton.onClick.AddListener(SelectOrBuyShownShip);
            foreach (ShipButton shipButton in m_shipButtons)
            {
                shipButton.Clicked += ShipButton_Clicked;
            }

            ShowCoins();
        }

        private void OnDisable()
        {
            m_backButton.onClick.RemoveListener(BackButton_Clicked);
            m_previousFamilyButton.onClick.RemoveListener(ShowPreviousFamily);
            m_nextFamilyButton.onClick.RemoveListener(ShowNextFamily);
            m_selectOrBuyButton.onClick.RemoveListener(SelectOrBuyShownShip);
            foreach (ShipButton shipButton in m_shipButtons)
            {
                shipButton.Clicked -= ShipButton_Clicked;
            }
        }

        // The camera cuts back to the spaceport, behind the curtain.
        private void BackButton_Clicked()
        {
            m_screens.ChangeBehindCurtain(m_gameManager.CloseHangar);
        }

        private void ShipButton_Clicked(ShipData ship)
        {
            ShowShip(ship);
        }

        private void ShowPreviousFamily()
        {
            SlideToFamily(-1);
        }

        private void ShowNextFamily()
        {
            SlideToFamily(1);
        }

        // The platform slides out with the shown ship, and the family changes once it is out of sight.
        private void SlideToFamily(int step)
        {
            ShipFamily family = GetAdjacentFamily(step);
            m_hangar.SlideToFamily(() => ShowFamily(family));
        }

        // Flies the shown ship if it is owned, or buys it if the player can afford it and flies it.
        private void SelectOrBuyShownShip()
        {
            SaveData saveData = SaveSystem.Data;
            if (saveData.IsShipUnlocked(m_shownShip.Id))
            {
                AudioSystem.Instance.PlayShipSelect();
                SelectShip(m_shownShip);
                return;
            }

            if (!saveData.TryBuyShip(m_shownShip.Id, m_shownShip.Price))
            {
                return;
            }

            AudioSystem.Instance.PlayShipPurchase();
            SelectShip(m_shownShip);
            ShowCoins();
        }

        private void ShowFamily(ShipFamily family)
        {
            ShowOnlyChild(m_skinButtonContainer, (int)family);
            ShowShip(m_shipCatalog.GetFirstShipOfFamily(family));
        }

        // The ship on the stand, its family, and either its price or whether it is already the one being flown.
        private void ShowShip(ShipData ship)
        {
            m_shownShip = ship;
            m_hangar.ShowShip(ship);
            m_familyNameText.SetText(ship.Family.ToString().ToUpperInvariant());
            ShowSelectOrBuy(ship);
            ShowShipButtons();
        }

        private void SelectShip(ShipData ship)
        {
            m_playerShip.ChangeShip(ship);
            SaveSystem.Data.SelectShip(ship.Id);

            // Written to disk right away rather than when the app is paused or
            // closed. After a purchase, that also keeps the coins spent.
            SaveSystem.Save();

            m_selectOrBuyLook.ShowSelected();
            ShowShipButtons();
        }

        private void ShowSelectOrBuy(ShipData ship)
        {
            SaveData saveData = SaveSystem.Data;
            if (!saveData.IsShipUnlocked(ship.Id))
            {
                m_selectOrBuyLook.ShowPrice(ship.Price, saveData.Coins >= ship.Price);
            }
            else if (ship == m_playerShip.Ship)
            {
                m_selectOrBuyLook.ShowSelected();
            }
            else
            {
                m_selectOrBuyLook.ShowSelect();
            }
        }

        private void ShowCoins()
        {
            m_coinText.SetText("{0}", SaveSystem.Data.Coins);
        }

        // The buttons of the other families are switched off, but they show the right state when they come on.
        private void ShowShipButtons()
        {
            SaveData saveData = SaveSystem.Data;
            foreach (ShipButton shipButton in m_shipButtons)
            {
                ShipData ship = shipButton.Ship;
                shipButton.ShowState(ship == m_shownShip, saveData.IsShipUnlocked(ship.Id), ship == m_playerShip.Ship);
            }
        }

        // Wraps around at both ends of the hangar.
        private ShipFamily GetAdjacentFamily(int step)
        {
            return (ShipFamily)(((int)m_shownShip.Family + step + s_familyCount) % s_familyCount);
        }

        private static void ShowOnlyChild(Transform container, int index)
        {
            foreach (Transform child in container)
            {
                child.gameObject.SetActive(false);
            }

            container.GetChild(index).gameObject.SetActive(true);
        }
    }
}
