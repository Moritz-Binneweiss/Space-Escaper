using UnityEngine;
using UnityEngine.EventSystems;

namespace SpaceEscaper
{
    /// <summary>
    /// Lets a finger move as far as on Android before its touch becomes a drag. Unity's default of 10 pixels is less
    /// than a millimetre on a phone, so a tap that wobbles a little would drag the list or the sheet it lies on, and
    /// its button would not click.
    /// </summary>
    [RequireComponent(typeof(EventSystem))]
    public class DragThreshold : MonoBehaviour
    {
        // The touch slop of Android, in its density-independent pixels, 160 to the inch.
        private const float k_TouchSlopDp = 8f;
        private const float k_DpPerInch = 160f;

        // A screen that tells no dpi counts as a typical phone, as for the swipes of the ship.
        private const float k_FallbackDpi = 420f;

        // Unity's default stays the least, for the mouse in the editor.
        private const int k_MinPixels = 10;

        private void Awake()
        {
            GetComponent<EventSystem>().pixelDragThreshold = GetPixels(Screen.dpi);
        }

        // How many pixels a finger moves on a screen with this many dots per inch before its touch becomes a drag.
        // Internal for tests.
        internal static int GetPixels(float dpi)
        {
            float pixels = k_TouchSlopDp * (dpi > 0f ? dpi : k_FallbackDpi) / k_DpPerInch;
            return Mathf.Max(k_MinPixels, Mathf.RoundToInt(pixels));
        }
    }
}
