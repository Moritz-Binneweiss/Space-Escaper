using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// A card on the colorways screen. It shows the main colors of its colorway whichever colorway the UI shows: its
    /// background, a swatch of its glow color and one of its starlight. The chosen card has a border in its glow color
    /// and a check.
    /// </summary>
    public class ColorwayCard : MonoBehaviour
    {
        [SerializeField] private ColorwayData m_colorway;
        [SerializeField] private Button m_button;
        [SerializeField] private Graphic m_background;
        [SerializeField] private Graphic m_border;
        [Tooltip("The border while another colorway is chosen.")]
        [SerializeField] private Color m_borderColor;
        [SerializeField] private Graphic m_glowSwatch;
        [SerializeField] private Graphic m_starlightSwatch;
        [SerializeField] private TMP_Text m_titleText;
        [Tooltip("The check at the end of the chosen card.")]
        [SerializeField] private GameObject m_check;
        [SerializeField] private Graphic m_checkPlate;
        [SerializeField] private Graphic m_checkIcon;

        public ColorwayData Colorway => m_colorway;

        // For tests, which tap it like a player and check which card shows the check.
        internal Button Button => m_button;

        internal GameObject Check => m_check;

        public event Action<ColorwayCard> Clicked;

        private void OnEnable()
        {
            m_button.onClick.AddListener(OnClicked);
            ShowColorway();
        }

        private void OnDisable()
        {
            m_button.onClick.RemoveListener(OnClicked);
        }

        public void ShowChosen(bool isChosen)
        {
            m_border.color = isChosen ? m_colorway.Glow : m_borderColor;
            m_check.SetActive(isChosen);
        }

        // The scene holds the same title and colors, so the layout tests measure what the game shows.
        private void ShowColorway()
        {
            m_titleText.SetText(m_colorway.Title);
            m_background.color = m_colorway.Background;
            m_glowSwatch.color = m_colorway.Glow;
            m_starlightSwatch.color = m_colorway.Starlight;
            m_checkPlate.color = m_colorway.Glow;
            m_checkIcon.color = m_colorway.TextOnGlow;
        }

        private void OnClicked()
        {
            Clicked?.Invoke(this);
        }
    }
}
