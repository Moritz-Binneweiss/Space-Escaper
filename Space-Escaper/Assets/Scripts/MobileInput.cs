using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceEscaper
{
    /// <summary>
    /// Turns touch, mouse and arrow keys into taps and lane swipes, once per frame.
    /// </summary>
    public class MobileInput : MonoBehaviour
    {
        private const float k_DeadzoneInPixels = 100f;
        private const string k_LeftTrigger = "Left";
        private const string k_RightTrigger = "Right";

        private Animator m_animator;
        private bool m_hasTapped;
        private bool m_hasSwipedLeft;
        private bool m_hasSwipedRight;
        private bool m_isDragging;
        private Vector2 m_swipeDelta;
        private Vector2 m_touchStartPosition;

        public static MobileInput Instance { get; private set; }

        public bool HasTapped => m_hasTapped;
        public Vector2 SwipeDelta => m_swipeDelta;
        public bool HasSwipedLeft => m_hasSwipedLeft;
        public bool HasSwipedRight => m_hasSwipedRight;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            m_animator = GetComponent<Animator>();
        }

        private void Update()
        {
            m_hasTapped = false;
            m_hasSwipedLeft = false;
            m_hasSwipedRight = false;
            m_swipeDelta = Vector2.zero;

            ReadPointer();
            DetectSwipe();
            ReadKeyboard();

            if (m_hasSwipedLeft)
            {
                m_animator.SetTrigger(k_LeftTrigger);
            }

            if (m_hasSwipedRight)
            {
                m_animator.SetTrigger(k_RightTrigger);
            }
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
                m_hasTapped = true;
                m_isDragging = true;
                m_touchStartPosition = pointer.position.ReadValue();
            }
            else if (!pointer.press.isPressed)
            {
                m_isDragging = false;
            }

            if (m_isDragging)
            {
                m_swipeDelta = pointer.position.ReadValue() - m_touchStartPosition;
            }
        }

        // A drag counts as a swipe as soon as it leaves the deadzone, and only
        // once per drag.
        private void DetectSwipe()
        {
            if (m_swipeDelta.magnitude <= k_DeadzoneInPixels)
            {
                return;
            }

            if (m_swipeDelta.x < 0)
            {
                m_hasSwipedLeft = true;
            }
            else
            {
                m_hasSwipedRight = true;
            }

            m_isDragging = false;
            m_swipeDelta = Vector2.zero;
        }

        // The arrow keys act like a swipe, for testing in Play Mode.
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
        }
    }
}
