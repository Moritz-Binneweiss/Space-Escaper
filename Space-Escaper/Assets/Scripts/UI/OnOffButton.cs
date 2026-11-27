using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The look of a button that switches something on and off, like the music: in the glow color while it is on,
    /// grey while it is off, with the state written on it.
    /// </summary>
    public class OnOffButton : MonoBehaviour
    {
        private const string k_OnKey = "common.on";
        private const string k_OffKey = "common.off";

        [SerializeField] private Graphic m_fill;
        [SerializeField] private Graphic m_border;
        [Tooltip("Icon, label and state, which all take the color of the text.")]
        [SerializeField] private Graphic[] m_texts;
        [SerializeField] private LocalizedText m_stateText;
        [SerializeField] private ColorwayColor m_onFillColor = new ColorwayColor(ColorwayRole.Glow);
        [SerializeField] private ColorwayColor m_onBorderColor = new ColorwayColor(ColorwayRole.Glow);
        [SerializeField] private ColorwayColor m_onTextColor = new ColorwayColor(ColorwayRole.Glow);
        [SerializeField] private ColorwayColor m_offFillColor = new ColorwayColor(ColorwayRole.Fixed);
        [SerializeField] private ColorwayColor m_offBorderColor = new ColorwayColor(ColorwayRole.Fixed);
        [SerializeField] private ColorwayColor m_offTextColor = new ColorwayColor(ColorwayRole.Fixed);

        public void ShowState(bool isOn)
        {
            ColorwayData colorway = ColorwayManager.Current;
            m_fill.color = (isOn ? m_onFillColor : m_offFillColor).Resolve(colorway);
            m_border.color = (isOn ? m_onBorderColor : m_offBorderColor).Resolve(colorway);
            Color textColor = (isOn ? m_onTextColor : m_offTextColor).Resolve(colorway);
            foreach (Graphic text in m_texts)
            {
                text.color = textColor;
            }

            m_stateText.Show(isOn ? k_OnKey : k_OffKey);
        }
    }
}
