using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// One colorway of the UI: its background, the glow with its shadow and the text on it, the starlight with the
    /// text on it, and the images of both glows. One asset per colorway, all of them listed in the
    /// <see cref="ColorwayCatalog"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "ColorwayData", menuName = "Space Escaper/Colorway Data")]
    public class ColorwayData : ScriptableObject
    {
        [Tooltip("The save stores the chosen colorway under this ID. Changing it loses the choice in existing saves.")]
        [SerializeField] private string m_id;
        [SerializeField] private string m_title;
        [Tooltip("Dark like space, behind the screens that cover the world.")]
        [SerializeField] private Color m_background;
        [Tooltip("Launch, switches, progress and what is chosen.")]
        [SerializeField] private Color m_glow;
        [SerializeField] private Color m_glowShadow;
        [SerializeField] private Color m_textOnGlow;
        [Tooltip("Coins, the highscore, rewards and what is new.")]
        [SerializeField] private Color m_starlight;
        [SerializeField] private Color m_textOnStarlight;
        [Tooltip("The glow around Launch, filled, made for the glow color over the dark world.")]
        [SerializeField] private Sprite m_glowImage;
        [Tooltip("The glow around the bars and banners in a run, only outside their plate, made for the starlight.")]
        [SerializeField] private Sprite m_starlightGlowImage;

        public string Id => m_id;
        public string Title => m_title;
        public Color Background => m_background;
        public Color Glow => m_glow;
        public Color GlowShadow => m_glowShadow;
        public Color TextOnGlow => m_textOnGlow;
        public Color Starlight => m_starlight;
        public Color TextOnStarlight => m_textOnStarlight;
        public Sprite GlowImage => m_glowImage;
        public Sprite StarlightGlowImage => m_starlightGlowImage;
    }
}
