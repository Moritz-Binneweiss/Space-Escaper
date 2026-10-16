using System;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The hangar screen. It shows one ship at a time: the skin buttons and arrows pick it, and
    /// the button below it flies it if it is owned, or buys it.
    /// </summary>
    public class ShopScreen : MonoBehaviour
    {
        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private ShipCatalog m_shipCatalog;
        [SerializeField] private PlayerShip m_playerShip;
        [Tooltip("Shows the ship on its stand.")]
        [SerializeField] private Hangar m_hangar;
        [SerializeField] private Button m_backButton;
        [SerializeField] private Button m_previousFamilyButton;
        [SerializeField] private Button m_nextFamilyButton;
        [SerializeField] private Button m_selectOrBuyButton;
        [Tooltip("Graphic of the select or buy button: Select, Selected or the price tag of the shown ship.")]
        [SerializeField] private Image m_selectOrBuyImage;
        [SerializeField] private Sprite m_selectSprite;
        [SerializeField] private Sprite m_selectedSprite;
        [Tooltip("Skin buttons, one child per family in the order of ShipFamily.")]
        [SerializeField] private Transform m_skinButtonContainer;

        private ShipButton[] m_shipButtons;
        private ShipData m_shownShip;

        private static readonly int s_familyCount = Enum.GetValues(typeof(ShipFamily)).Length;

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
            m_backButton.onClick.AddListener(m_gameManager.CloseHangar);
            m_previousFamilyButton.onClick.AddListener(ShowPreviousFamily);
            m_nextFamilyButton.onClick.AddListener(ShowNextFamily);
            m_selectOrBuyButton.onClick.AddListener(SelectOrBuyShownShip);
            foreach (ShipButton shipButton in m_shipButtons)
            {
                shipButton.Clicked += ShipButton_Clicked;
            }
        }

        private void OnDisable()
        {
            m_backButton.onClick.RemoveListener(m_gameManager.CloseHangar);
            m_previousFamilyButton.onClick.RemoveListener(ShowPreviousFamily);
            m_nextFamilyButton.onClick.RemoveListener(ShowNextFamily);
            m_selectOrBuyButton.onClick.RemoveListener(SelectOrBuyShownShip);
            foreach (ShipButton shipButton in m_shipButtons)
            {
                shipButton.Clicked -= ShipButton_Clicked;
            }
        }

        private void ShipButton_Clicked(ShipData ship)
        {
            ShowShip(ship);
        }

        private void ShowPreviousFamily()
        {
            ShowFamily(GetAdjacentFamily(-1));
        }

        private void ShowNextFamily()
        {
            ShowFamily(GetAdjacentFamily(1));
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
        }

        private void ShowFamily(ShipFamily family)
        {
            ShowOnlyChild(m_skinButtonContainer, (int)family);
            ShowShip(m_shipCatalog.GetFirstShipOfFamily(family));
        }

        // The ship on the stand, and either its price tag or whether it is already the one being
        // flown.
        private void ShowShip(ShipData ship)
        {
            m_shownShip = ship;
            m_hangar.ShowShip(ship);
            m_selectOrBuyImage.sprite = GetSelectOrBuySprite(ship);
        }

        private void SelectShip(ShipData ship)
        {
            m_playerShip.ChangeShip(ship);
            SaveSystem.Data.SelectShip(ship.Id);

            // Written to disk right away rather than when the app is paused or
            // closed. After a purchase, that also keeps the coins spent.
            SaveSystem.Save();

            m_selectOrBuyImage.sprite = m_selectedSprite;
        }

        private Sprite GetSelectOrBuySprite(ShipData ship)
        {
            if (!SaveSystem.Data.IsShipUnlocked(ship.Id))
            {
                return ship.PriceTag;
            }

            return ship == m_playerShip.Ship ? m_selectedSprite : m_selectSprite;
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
