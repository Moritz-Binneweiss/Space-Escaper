using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The colorways, opened from the settings: a card per colorway, and a tap on one shows it everywhere at once and
    /// keeps it in the save. Going back shows the settings again, which leave the state of the game as it is.
    /// </summary>
    public class ColorwaysScreen : MonoBehaviour
    {
        [SerializeField] private ScreenManager m_screens;
        [SerializeField] private ColorwayManager m_colorways;
        [SerializeField] private Button m_backButton;
        [Tooltip("One card per colorway, in the order of the catalog.")]
        [SerializeField] private ColorwayCard[] m_cards;

        // For tests, which tap them like a player.
        internal IReadOnlyList<ColorwayCard> Cards => m_cards;
        internal Button BackButton => m_backButton;

        private void OnEnable()
        {
            m_backButton.onClick.AddListener(m_screens.ShowSettings);
            foreach (ColorwayCard card in m_cards)
            {
                card.Clicked += Card_Clicked;
            }

            ColorwayManager.ColorwayChanged += ColorwayManager_ColorwayChanged;
            ShowChosen();
        }

        private void OnDisable()
        {
            m_backButton.onClick.RemoveListener(m_screens.ShowSettings);
            foreach (ColorwayCard card in m_cards)
            {
                card.Clicked -= Card_Clicked;
            }

            ColorwayManager.ColorwayChanged -= ColorwayManager_ColorwayChanged;
        }

        private void Card_Clicked(ColorwayCard card)
        {
            m_colorways.Choose(card.Colorway);
        }

        private void ColorwayManager_ColorwayChanged()
        {
            ShowChosen();
        }

        private void ShowChosen()
        {
            foreach (ColorwayCard card in m_cards)
            {
                card.ShowChosen(card.Colorway == ColorwayManager.Current);
            }
        }
    }
}
