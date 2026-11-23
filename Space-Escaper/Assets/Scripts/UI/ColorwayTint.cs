using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// Colors a graphic of the UI by its role in the colorway the UI shows, when it comes on and when another colorway
    /// is chosen. The scene holds the colors of the default colorway.
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public class ColorwayTint : MonoBehaviour
    {
        [SerializeField] private ColorwayColor m_color = new ColorwayColor(ColorwayRole.Glow);
        [Tooltip("Takes the image of the glow made for its color, which already holds how strongly it shows, so the " +
            "opacity is its alpha as it is.")]
        [SerializeField] private bool m_isGlow;

        private Graphic m_graphic;

        public ColorwayColor ColorwayColor => m_color;
        public bool IsGlow => m_isGlow;

        private void Awake()
        {
            m_graphic = GetComponent<Graphic>();
        }

        private void OnEnable()
        {
            ColorwayManager.ColorwayChanged += ColorwayManager_ColorwayChanged;
            Show();
        }

        private void OnDisable()
        {
            ColorwayManager.ColorwayChanged -= ColorwayManager_ColorwayChanged;
        }

        /// <summary>
        /// The color of the graphic in a colorway.
        /// </summary>
        public Color GetColor(ColorwayData colorway)
        {
            Color color = m_color.Resolve(colorway);
            if (m_isGlow)
            {
                color.a = m_color.Opacity;
            }

            return color;
        }

        /// <summary>
        /// The image of the glow in a colorway, the one of its glow color or of its starlight.
        /// </summary>
        public Sprite GetGlowImage(ColorwayData colorway)
        {
            return m_color.Role == ColorwayRole.Starlight ? colorway.StarlightGlowImage : colorway.GlowImage;
        }

        private void ColorwayManager_ColorwayChanged()
        {
            Show();
        }

        private void Show()
        {
            ColorwayData colorway = ColorwayManager.Current;
            if (colorway == null)
            {
                return;
            }

            if (m_isGlow)
            {
                ((Image)m_graphic).sprite = GetGlowImage(colorway);
            }

            m_graphic.color = GetColor(colorway);
        }
    }
}
