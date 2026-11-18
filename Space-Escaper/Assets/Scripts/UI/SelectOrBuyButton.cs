using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The look of the button under the ship in the hangar: the price of a ship to buy, faded while the coins are not
    /// enough, Select for one that is owned, or Selected in the accent color for the one being flown.
    /// </summary>
    public class SelectOrBuyButton : MonoBehaviour
    {
        private const string k_SelectText = "SELECT";
        private const string k_SelectedText = "SELECTED";
        private const float k_TooExpensiveAlpha = 0.5f;

        [Tooltip("Fades the whole button while the ship costs more coins than the player has.")]
        [SerializeField] private CanvasGroup m_canvasGroup;
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

        public void ShowPrice(int price, bool canAfford)
        {
            Show(false, true);
            m_label.SetText("{0}", price);
            m_canvasGroup.alpha = canAfford ? 1f : k_TooExpensiveAlpha;
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
            m_canvasGroup.alpha = 1f;
            m_plate.color = isSelected ? m_selectedPlateColor : m_plateColor;
            m_shadow.color = isSelected ? m_selectedShadowColor : m_shadowColor;
            m_label.color = isSelected ? m_selectedTextColor : m_textColor;
            m_priceIcon.SetActive(hasPrice);
            m_selectedIcon.SetActive(isSelected);
        }
    }
}
