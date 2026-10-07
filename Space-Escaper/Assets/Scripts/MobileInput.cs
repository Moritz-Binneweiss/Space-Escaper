using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceEscaper
{
    /// <summary>
    /// Turns touch, mouse and keys into taps, double taps and swipes, once per frame.
    /// </summary>
    public class MobileInput : MonoBehaviour
    {
        // In millimeters, so a swipe takes the same finger travel on every screen.
        // 6 mm are about 100 px on a typical phone.
        private const float k_DeadzoneInMillimeters = 6f;
        // Screen.dpi is 0 on devices that do not report it. 420 dpi is a typical
        // phone.
        private const float k_FallbackDpi = 420f;
        private const float k_MillimetersPerInch = 25.4f;
        // Longest time from one touch to the next for a double tap, as on Android.
        private const float k_DoubleTapIntervalInSeconds = 0.3f;

        private bool m_hasTapped;
        private bool m_hasDoubleTapped;
        private bool m_hasSwipedLeft;
        private bool m_hasSwipedRight;
        private bool m_hasSwipedUp;
        private bool m_hasSwipedDown;
        private bool m_isDragging;
        private bool m_isWaitingForSecondTap;
        private bool m_isSecondTap;
        private float m_deadzoneInPixels;
        private float m_tapStartTime;
        private Vector2 m_swipeDelta;
        private Vector2 m_touchStartPosition;

        public static MobileInput Instance { get; private set; }

        public bool HasTapped => m_hasTapped;
        /// <summary>
        /// True in the frame the second of two quick touches comes down. The first
        /// one has to end as a tap, not as a swipe.
        /// </summary>
        public bool HasDoubleTapped => m_hasDoubleTapped;
        public Vector2 SwipeDelta => m_swipeDelta;
        public bool HasSwipedLeft => m_hasSwipedLeft;
        public bool HasSwipedRight => m_hasSwipedRight;
        public bool HasSwipedUp => m_hasSwipedUp;
        public bool HasSwipedDown => m_hasSwipedDown;

        private void Awake()
        {
            Instance = this;

            float dpi = Screen.dpi > 0f ? Screen.dpi : k_FallbackDpi;
            m_deadzoneInPixels = k_DeadzoneInMillimeters / k_MillimetersPerInch * dpi;
        }

        private void Update()
        {
            m_hasTapped = false;
            m_hasDoubleTapped = false;
            m_hasSwipedLeft = false;
            m_hasSwipedRight = false;
            m_hasSwipedUp = false;
            m_hasSwipedDown = false;
            m_swipeDelta = Vector2.zero;

            ReadPointer();
            DetectSwipe();
            ReadKeyboard();
        }

        // Pointer.current is the touchscreen on the phone and the mouse in the
        // Editor, so both share this path. On a touchscreen it follows the first
        // finger.
        private void ReadPointer()
        {
            Pointer pointer = Pointer.current;
            if (pointer == null)
            {
                return;
            }

            if (pointer.press.wasPressedThisFrame)
            {
                m_isDragging = true;
                m_touchStartPosition = pointer.position.ReadValue();
                StartTap();
            }
            else if (!pointer.press.isPressed && m_isDragging)
            {
                m_isDragging = false;

                // Let go before the drag became a swipe. A quick flick can still
                // leave the deadzone in this last frame, and that is no tap.
                Vector2 finalDelta = pointer.position.ReadValue() - m_touchStartPosition;
                if (finalDelta.magnitude <= m_deadzoneInPixels)
                {
                    EndTap();
                }
            }

            if (m_isDragging)
            {
                m_swipeDelta = pointer.position.ReadValue() - m_touchStartPosition;
            }
        }

        // A drag counts as a swipe as soon as it leaves the deadzone, and only
        // once per drag. The longer axis decides the direction, so a swipe
        // upwards with a bit of sideways drift does not change lanes.
        private void DetectSwipe()
        {
            if (m_swipeDelta.magnitude <= m_deadzoneInPixels)
            {
                return;
            }

            if (Mathf.Abs(m_swipeDelta.x) > Mathf.Abs(m_swipeDelta.y))
            {
                if (m_swipeDelta.x < 0)
                {
                    m_hasSwipedLeft = true;
                }
                else
                {
                    m_hasSwipedRight = true;
                }
            }
            else if (m_swipeDelta.y < 0)
            {
                m_hasSwipedDown = true;
            }
            else
            {
                m_hasSwipedUp = true;
            }

            m_isDragging = false;
            m_swipeDelta = Vector2.zero;
        }

        // The arrow keys act like swipes and the space bar like a finger, for
        // testing in Play Mode.
        private void ReadKeyboard()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.leftArrowKey.wasPressedThisFrame)
            {
                m_hasSwipedLeft = true;
            }
            else if (keyboard.rightArrowKey.wasPressedThisFrame)
            {
                m_hasSwipedRight = true;
            }
            else if (keyboard.upArrowKey.wasPressedThisFrame)
            {
                m_hasSwipedUp = true;
            }
            else if (keyboard.downArrowKey.wasPressedThisFrame)
            {
                m_hasSwipedDown = true;
            }

            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                StartTap();
            }
            else if (keyboard.spaceKey.wasReleasedThisFrame)
            {
                EndTap();
            }
        }

        // A touch completes a double tap if it comes down soon after the one
        // before, and that one ended as a tap. As on Android, the touch that
        // completes a double tap cannot start the next one.
        private void StartTap()
        {
            float now = Time.unscaledTime;
            m_hasTapped = true;
            m_hasDoubleTapped = m_isWaitingForSecondTap && now - m_tapStartTime <= k_DoubleTapIntervalInSeconds;
            m_isSecondTap = m_hasDoubleTapped;
            m_isWaitingForSecondTap = false;
            m_tapStartTime = now;
        }

        private void EndTap()
        {
            m_isWaitingForSecondTap = !m_isSecondTap;
        }
    }
}
