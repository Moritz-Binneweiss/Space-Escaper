using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Lets a popup drop into its place from a little above while it fades in, like the popups of the run in the
    /// mockups, and rise back up while it fades out. It runs in the time of the game, so it stands still in the pause
    /// like the time the popup is shown.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class DropEffect : MonoBehaviour
    {
        [Tooltip("Seconds from above into its place.")]
        [SerializeField] private float m_duration = 0.5f;
        [Tooltip("Seconds from its place back up out of sight.")]
        [SerializeField] private float m_leaveDuration = 0.3f;
        [Tooltip("How far above its place it starts and ends.")]
        [SerializeField] private float m_height = 14f;

        private RectTransform m_rectTransform;
        private CanvasGroup m_group;
        private Vector2 m_place;
        private float m_shown;
        private float m_startShown;
        private float m_startTime;
        private bool m_isMoving;
        private bool m_isLeaving;

        /// <summary>
        /// Whether the popup is up or on its way there, not leaving.
        /// </summary>
        public bool IsShown => gameObject.activeSelf && !m_isLeaving;

        private void Awake()
        {
            m_rectTransform = (RectTransform)transform;
            m_group = GetComponent<CanvasGroup>();
            m_place = m_rectTransform.anchoredPosition;
        }

        private void Update()
        {
            if (!m_isMoving)
            {
                return;
            }

            float time = Mathf.Clamp01((Time.time - m_startTime) / (m_isLeaving ? m_leaveDuration : m_duration));
            if (m_isLeaving)
            {
                Show(m_startShown * (1f - Easing.EaseIn(time)));
            }
            else
            {
                Show(Mathf.Lerp(m_startShown, 1f, Easing.EaseOut(time)));
            }

            if (time < 1f)
            {
                return;
            }

            m_isMoving = false;
            if (m_isLeaving)
            {
                m_isLeaving = false;
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Switches the popup on and drops it in. One that is leaving comes back from where it is, one that is up
        /// stays where it is.
        /// </summary>
        public void Play()
        {
            if (IsShown)
            {
                return;
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                Show(0f);
            }

            Move(false);
        }

        /// <summary>
        /// Lets the popup rise out of sight and switches it off there.
        /// </summary>
        public void Leave()
        {
            if (IsShown)
            {
                Move(true);
            }
        }

        private void Move(bool isLeaving)
        {
            m_isLeaving = isLeaving;
            m_startShown = m_shown;
            m_startTime = Time.time;
            m_isMoving = true;
        }

        private void Show(float shown)
        {
            m_shown = shown;
            m_group.alpha = shown;
            m_rectTransform.anchoredPosition = m_place + new Vector2(0f, m_height * (1f - shown));
        }
    }
}
