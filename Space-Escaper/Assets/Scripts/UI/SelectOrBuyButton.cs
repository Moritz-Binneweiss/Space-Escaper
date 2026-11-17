using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The look of the button under the ship in the hangar: the price of a ship to buy, Select for one that is owned,
    /// or Selected in the accent color for the one being flown.
    /// </summary>
    public class SelectOrBuyButton : MonoBehaviour
    {
        private const string k_SelectText = "SELECT";
        private const string k_SelectedText = "SELECTED";

        [SerializeField] private Graphic m_plate;
        [SerializeField] private Graphic m_shadow;
        [SerializeField] private TMP_Text m_label;
        [Tooltip("The coin before the price.")]
        [SerializeField] private GameObject m_priceIcon;
        [Tooltip("The check before Selected.")]
        [SerializeField] private GameObject m_selectedIcon;
        [SerializeField] private Color m_plateColor = Color.white;
        [SerializeField] private Color m_shadowColor;
        [SerializeField] private Color m_textColor;
        [SerializeField] private Color m_selectedPlateColor;
        [SerializeField] private Color m_selectedShadowColor;
        [SerializeField] private Color m_selectedTextColor;

        public void ShowPrice(int price)
        {
            Show(false, true);
            m_label.SetText("{0}", price);
        }

        public void ShowSelect()
        {
            Show(false, false);
            m_label.SetText(k_SelectText);
        }

        public void ShowSelected()
        {
            Show(true, false);
            m_label.SetText(k_SelectedText);
        }

        private void Show(bool isSelected, bool hasPrice)
        {
            m_plate.color = isSelected ? m_selectedPlateColor : m_plateColor;
            m_shadow.color = isSelected ? m_selectedShadowColor : m_shadowColor;
            m_label.color = isSelected ? m_selectedTextColor : m_textColor;
            m_priceIcon.SetActive(hasPrice);
            m_selectedIcon.SetActive(isSelected);
        }
    }
}
