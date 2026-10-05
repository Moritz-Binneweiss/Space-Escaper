using UnityEngine;
using UnityEngine.InputSystem;

public class MobileInput : MonoBehaviour
{
    private const float DEADZONE = 100f;

    //Animation
    private Animator anim;

    public static MobileInput Instance { set; get; }

    private bool tap,
        swipeLeft,
        swipeRight,
        isDragging;
    private Vector2 swipeDelta,
        startTouch;

    public bool Tap
    {
        get { return tap; }
    }
    public Vector2 SwipeDelta
    {
        get { return swipeDelta; }
    }
    public bool SwipeLeft
    {
        get { return swipeLeft; }
    }
    public bool SwipeRight
    {
        get { return swipeRight; }
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        //Reseting all the booleans
        tap = swipeLeft = swipeRight = false;
        swipeDelta = Vector2.zero;

        #region Touch and Mouse Inputs
        // Pointer.current is the touchscreen on the phone and the mouse in the
        // Editor, so both share this path. On a touchscreen it follows the first
        // finger.
        Pointer pointer = Pointer.current;
        if (pointer != null)
        {
            if (pointer.press.wasPressedThisFrame)
            {
                tap = true;
                isDragging = true;
                startTouch = pointer.position.ReadValue();
            }
            else if (!pointer.press.isPressed)
            {
                isDragging = false;
            }

            //Calculate distance
            if (isDragging)
                swipeDelta = pointer.position.ReadValue() - startTouch;
        }
        #endregion

        //Let's check if we're beyond the deadzone
        if (swipeDelta.magnitude > DEADZONE)
        {
            //This is a confirmed swipe, only one per drag
            if (swipeDelta.x < 0)
                swipeLeft = true;
            else
                swipeRight = true;

            isDragging = false;
            swipeDelta = Vector2.zero;
        }

        #region Keyboard Inputs
        // The arrow keys act like a swipe, for testing in Play Mode.
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.leftArrowKey.wasPressedThisFrame)
                swipeLeft = true;
            else if (keyboard.rightArrowKey.wasPressedThisFrame)
                swipeRight = true;
        }
        #endregion

        if (swipeLeft)
            anim.SetTrigger("Left");
        if (swipeRight)
            anim.SetTrigger("Right");
    }
}
