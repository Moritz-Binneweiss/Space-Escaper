using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The look of a button that switches something on and off, like the music: in the accent color while it is on,
    /// grey while it is off, with the state written on it.
    /// </summary>
    public class OnOffButton : MonoBehaviour
    {
        private const string k_OnText = "ON";
        private const string k_OffText = "OFF";

        [SerializeField] private Graphic m_fill;
        [SerializeField] private Graphic m_border;
        [Tooltip("Icon, label and state, which all take the color of the text.")]
        [SerializeField] private Graphic[] m_texts;
        [SerializeField] private TMP_Text m_stateText;
        [SerializeField] private Color m_onFillColor;
        [SerializeField] private Color m_onBorderColor;
        [SerializeField] private Color m_onTextColor;
        [SerializeField] private Color m_offFillColor;
        [SerializeField] private Color m_offBorderColor;
        [SerializeField] private Color m_offTextColor;

        public void ShowState(bool isOn)
        {
            m_fill.color = isOn ? m_onFillColor : m_offFillColor;
            m_border.color = isOn ? m_onBorderColor : m_offBorderColor;
            foreach (Graphic text in m_texts)
            {
                text.color = isOn ? m_onTextColor : m_offTextColor;
            }

            m_stateText.text = isOn ? k_OnText : k_OffText;
        }
    }
}
