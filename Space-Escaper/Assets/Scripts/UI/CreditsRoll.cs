using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// Rolls the credits slowly up through their window and starts over once they have passed. A drag
    /// moves them by hand, and the roll goes on from where it lets go.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class CreditsRoll : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        [Tooltip("Units per second, the pace of the mockups.")]
        [SerializeField] private float m_speed = 60f;

        private ScrollRect m_scrollRect;
        private bool m_isDragging;
        private bool m_isStarting;

        private void Awake()
        {
            m_scrollRect = GetComponent<ScrollRect>();
        }

        // The window has its height only once the safe area has fitted it, so the roll starts in the
        // first frame the screen is on.
        private void OnEnable()
        {
            m_isDragging = false;
            m_isStarting = true;
        }

        // Unscaled time: the credits also open from the settings of the pause.
        private void LateUpdate()
        {
            if (m_isDragging)
            {
                return;
            }

            // The content hangs from the top of the window. At 0 it starts at the top, minus the height of
            // the window it lies just below the window, at its own height just above.
            RectTransform content = m_scrollRect.content;
            float below = -m_scrollRect.viewport.rect.height;
            float above = content.rect.height;
            float position = content.anchoredPosition.y + m_speed * Time.unscaledDeltaTime;
            if (m_isStarting)
            {
                m_isStarting = false;
                m_scrollRect.StopMovement();
                position = below;
            }
            else if (position > above)
            {
                // Nothing of the credits is in the window before or after the jump.
                position = below + (position - above);
            }
            else if (position < below)
            {
                // Dragged down beyond the start, where the window is empty as well.
                position = below;
                m_scrollRect.StopMovement();
            }

            content.anchoredPosition = new Vector2(content.anchoredPosition.x, position);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            m_isDragging = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            m_isDragging = false;
        }
    }
}
