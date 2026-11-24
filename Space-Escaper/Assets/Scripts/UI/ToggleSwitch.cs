using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// Shows a toggle as a switch: the knob sits right and the track takes the glow color while it is on. Switched,
    /// the knob slides over and the color follows it.
    /// </summary>
    [RequireComponent(typeof(Toggle))]
    public class ToggleSwitch : MonoBehaviour
    {
        private const float k_SlideDuration = 0.15f;

        [SerializeField] private Graphic m_track;
        [SerializeField] private RectTransform m_knob;
        [SerializeField] private ColorwayColor m_onColor = new ColorwayColor(ColorwayRole.Glow);
        [SerializeField] private ColorwayColor m_offColor = new ColorwayColor(ColorwayRole.Fixed);
        [Tooltip("Horizontal position of the knob in the track while the toggle is off.")]
        [SerializeField] private float m_offX = -12f;
        [Tooltip("Horizontal position of the knob in the track while the toggle is on.")]
        [SerializeField] private float m_onX = 12f;

        private Toggle m_toggle;
        private bool m_isShownOn;

        // How far the knob is on its way from off to on, and where its slide started.
        private float m_position;
        private float m_slideStart;
        private float m_slideTime;

        // For tests, which follow it.
        internal RectTransform Knob => m_knob;

        private void Awake()
        {
            m_toggle = GetComponent<Toggle>();
        }

        // A switch coming on shows its state at once.
        private void OnEnable()
        {
            m_isShownOn = m_toggle.isOn;
            m_position = m_isShownOn ? 1f : 0f;
            m_slideTime = k_SlideDuration;
            Show();
        }

        // Watches the toggle instead of its event: SetIsOnWithoutNotify, which shows a saved setting, changes it
        // without one. In time that runs in the pause too, as the settings open from there.
        private void LateUpdate()
        {
            if (m_toggle.isOn != m_isShownOn)
            {
                m_isShownOn = m_toggle.isOn;
                m_slideStart = m_position;
                m_slideTime = 0f;
            }

            if (m_slideTime < k_SlideDuration)
            {
                m_slideTime += Time.unscaledDeltaTime;
                float time = Mathf.Clamp01(m_slideTime / k_SlideDuration);
                m_position = Mathf.Lerp(m_slideStart, m_isShownOn ? 1f : 0f, Easing.EaseOut(time));
                Show();
            }
        }

        private void Show()
        {
            ColorwayData colorway = ColorwayManager.Current;
            m_track.color = Color.Lerp(m_offColor.Resolve(colorway), m_onColor.Resolve(colorway), m_position);
            m_knob.anchoredPosition = new Vector2(Mathf.Lerp(m_offX, m_onX, m_position), m_knob.anchoredPosition.y);
        }
    }
}
