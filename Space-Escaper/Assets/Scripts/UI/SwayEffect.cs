using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Sways a graphic from side to side, like the arrows of the swipe hint in the mockups. It runs in the time of the
    /// game, so it stands still in the pause.
    /// </summary>
    public class SwayEffect : MonoBehaviour
    {
        [Tooltip("Seconds for one sway, from one side to the other and back.")]
        [SerializeField] private float m_period = 1.2f;
        [Tooltip("How far it goes to each side.")]
        [SerializeField] private float m_distance = 5f;

        private RectTransform m_rectTransform;
        private Vector2 m_place;

        private void Awake()
        {
            m_rectTransform = (RectTransform)transform;
            m_place = m_rectTransform.anchoredPosition;
        }

        private void Update()
        {
            m_rectTransform.anchoredPosition = m_place + new Vector2(GetOffset(Time.time, m_period, m_distance), 0f);
        }

        /// <summary>
        /// Where the sway is at a time: on the left at the start of a period, on the right in its middle, easing in
        /// and out on each way like the keyframes of the mockup.
        /// </summary>
        internal static float GetOffset(float time, float period, float distance)
        {
            float phase = Mathf.Repeat(time / period, 1f);
            float way = phase < 0.5f ? phase * 2f : 2f - phase * 2f;
            return Mathf.Lerp(-distance, distance, Easing.EaseInOut(way));
        }
    }
}
