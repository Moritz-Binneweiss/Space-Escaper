using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// Shows a toggle as a switch: the knob sits right and the track takes the accent color while it is on.
    /// </summary>
    [RequireComponent(typeof(Toggle))]
    public class ToggleSwitch : MonoBehaviour
    {
        [SerializeField] private Graphic m_track;
        [SerializeField] private RectTransform m_knob;
        [SerializeField] private Color m_onColor;
        [SerializeField] private Color m_offColor;
        [Tooltip("Horizontal position of the knob in the track while the toggle is off.")]
        [SerializeField] private float m_offX = -12f;
        [Tooltip("Horizontal position of the knob in the track while the toggle is on.")]
        [SerializeField] private float m_onX = 12f;

        private Toggle m_toggle;
        private bool m_isShownOn;

        private void Awake()
        {
            m_toggle = GetComponent<Toggle>();
        }

        private void OnEnable()
        {
            Show();
        }

        // Watches the toggle instead of its event: SetIsOnWithoutNotify, which shows a saved setting, changes it
        // without one.
        private void LateUpdate()
        {
            if (m_toggle.isOn != m_isShownOn)
            {
                Show();
            }
        }

        private void Show()
        {
            m_isShownOn = m_toggle.isOn;
            m_track.color = m_isShownOn ? m_onColor : m_offColor;
            m_knob.anchoredPosition = new Vector2(m_isShownOn ? m_onX : m_offX, m_knob.anchoredPosition.y);
        }
    }
}
