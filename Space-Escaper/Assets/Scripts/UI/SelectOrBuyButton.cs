using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The look of the button under the ship in the hangar: the price of a ship to buy, faded while the coins are not
    /// enough, Select for one that is owned, or Selected in the glow color for the one being flown.
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
        [SerializeField] private ColorwayColor m_plateColor = new ColorwayColor(ColorwayRole.Fixed);
        [SerializeField] private ColorwayColor m_shadowColor = new ColorwayColor(ColorwayRole.Fixed);
        [SerializeField] private ColorwayColor m_textColor = new ColorwayColor(ColorwayRole.Fixed);
        [SerializeField] private ColorwayColor m_selectedPlateColor = new ColorwayColor(ColorwayRole.Glow);
        [SerializeField] private ColorwayColor m_selectedShadowColor = new ColorwayColor(ColorwayRole.GlowShadow);
        [SerializeField] private ColorwayColor m_selectedTextColor = new ColorwayColor(ColorwayRole.TextOnGlow);

        private bool m_isSelected;

        // For tests, which check its colors.
        internal Graphic Plate => m_plate;

        // The hangar shows its ship again only when it changes, so the colors follow a colorway chosen since then here.
        private void OnEnable()
        {
            ShowColors();
        }

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
            m_isSelected = isSelected;
            ShowColors();
            m_priceIcon.SetActive(hasPrice);
            m_selectedIcon.SetActive(isSelected);
        }

        private void ShowColors()
        {
            ColorwayData colorway = ColorwayManager.Current;
            m_plate.color = (m_isSelected ? m_selectedPlateColor : m_plateColor).Resolve(colorway);
            m_shadow.color = (m_isSelected ? m_selectedShadowColor : m_shadowColor).Resolve(colorway);
            m_label.color = (m_isSelected ? m_selectedTextColor : m_textColor).Resolve(colorway);
        }
    }
}
