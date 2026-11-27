using System;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The button of one language in the settings: in the glow color while the UI shows its language, white while it
    /// shows another. A tap reports its language.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class LanguageButton : MonoBehaviour
    {
        [SerializeField] private LanguageData m_language;
        [SerializeField] private Graphic m_plate;
        [SerializeField] private Graphic m_shadow;
        [SerializeField] private Graphic m_label;
        [SerializeField] private ColorwayColor m_plateColor = new ColorwayColor(ColorwayRole.Fixed);
        [SerializeField] private ColorwayColor m_shadowColor = new ColorwayColor(ColorwayRole.Fixed);
        [SerializeField] private ColorwayColor m_labelColor = new ColorwayColor(ColorwayRole.Fixed);
        [SerializeField] private ColorwayColor m_chosenPlateColor = new ColorwayColor(ColorwayRole.Glow);
        [SerializeField] private ColorwayColor m_chosenShadowColor = new ColorwayColor(ColorwayRole.GlowShadow);
        [SerializeField] private ColorwayColor m_chosenLabelColor = new ColorwayColor(ColorwayRole.TextOnGlow);

        private Button m_button;

        public LanguageData Language => m_language;

        // For tests, which tap it and check its colors.
        internal Button Button => m_button;
        internal Graphic Plate => m_plate;

        public event Action<LanguageData> Clicked;

        private void Awake()
        {
            m_button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            m_button.onClick.AddListener(OnClicked);
        }

        private void OnDisable()
        {
            m_button.onClick.RemoveListener(OnClicked);
        }

        public void ShowState(bool isChosen)
        {
            ColorwayData colorway = ColorwayManager.Current;
            m_plate.color = (isChosen ? m_chosenPlateColor : m_plateColor).Resolve(colorway);
            m_shadow.color = (isChosen ? m_chosenShadowColor : m_shadowColor).Resolve(colorway);
            m_label.color = (isChosen ? m_chosenLabelColor : m_labelColor).Resolve(colorway);
        }

        private void OnClicked()
        {
            Clicked?.Invoke(m_language);
        }
    }
}
