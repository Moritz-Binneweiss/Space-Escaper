using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Fits its rectangle to the safe area of the screen, the part that camera cutouts and notches leave
    /// free. Each screen keeps its controls in here, while its background and the game world still reach
    /// under the cutout.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        private Rect m_fittedSafeArea;
        private Vector2Int m_fittedScreenSize;

        // Fitting as the screen comes on keeps its first frame out of the cutout.
        private void OnEnable()
        {
            FitToScreen();
        }

        // The safe area can change while a screen is shown, when a foldable phone opens or when the Device
        // Simulator of the editor switches to another device. Only the screen shown runs this.
        private void Update()
        {
            if (Screen.safeArea != m_fittedSafeArea || Screen.width != m_fittedScreenSize.x
                || Screen.height != m_fittedScreenSize.y)
            {
                FitToScreen();
            }
        }

        /// <summary>
        /// Fits the rectangle to a safe area given in pixels of a screen of the given size.
        /// </summary>
        internal void Fit(Rect safeArea, Vector2Int screenSize)
        {
            // A minimized window can report no size at all.
            if (screenSize.x <= 0 || screenSize.y <= 0)
            {
                return;
            }

            RectTransform rectTransform = (RectTransform)transform;
            rectTransform.anchorMin = new Vector2(safeArea.xMin / screenSize.x, safeArea.yMin / screenSize.y);
            rectTransform.anchorMax = new Vector2(safeArea.xMax / screenSize.x, safeArea.yMax / screenSize.y);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            m_fittedSafeArea = safeArea;
            m_fittedScreenSize = screenSize;
        }

        private void FitToScreen()
        {
            Fit(Screen.safeArea, new Vector2Int(Screen.width, Screen.height));
        }
    }
}
