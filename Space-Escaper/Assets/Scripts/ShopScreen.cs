using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The hangar screen: the skin buttons and arrows choose the ship shown, the button below it flies or buys it.
    /// </summary>
    public class ShopScreen : MonoBehaviour
    {
        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private Button m_backButton;
        [SerializeField] private Button m_previousFamilyButton;
        [SerializeField] private Button m_nextFamilyButton;
        [SerializeField] private Button m_selectOrBuyButton;

        private ShipButton[] m_shipButtons;

        private void Awake()
        {
            m_shipButtons = GetComponentsInChildren<ShipButton>(true);
        }

        private void OnEnable()
        {
            m_backButton.onClick.AddListener(m_gameManager.CloseHangar);
            m_previousFamilyButton.onClick.AddListener(m_gameManager.ShowPreviousFamily);
            m_nextFamilyButton.onClick.AddListener(m_gameManager.ShowNextFamily);
            m_selectOrBuyButton.onClick.AddListener(m_gameManager.SelectOrBuyShownShip);
            foreach (ShipButton shipButton in m_shipButtons)
            {
                shipButton.Clicked += m_gameManager.ShowShip;
            }
        }

        private void OnDisable()
        {
            m_backButton.onClick.RemoveListener(m_gameManager.CloseHangar);
            m_previousFamilyButton.onClick.RemoveListener(m_gameManager.ShowPreviousFamily);
            m_nextFamilyButton.onClick.RemoveListener(m_gameManager.ShowNextFamily);
            m_selectOrBuyButton.onClick.RemoveListener(m_gameManager.SelectOrBuyShownShip);
            foreach (ShipButton shipButton in m_shipButtons)
            {
                shipButton.Clicked -= m_gameManager.ShowShip;
            }
        }
    }
}
