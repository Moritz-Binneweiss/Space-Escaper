using System;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// The color of an element of the UI as a role in the colorway, so it follows the colorway the UI shows: a color
    /// of the colorway or a fixed one, see-through like in the mockups or mixed into what lies below it, like a
    /// greyed-out element.
    /// </summary>
    [Serializable]
    public struct ColorwayColor
    {
        // Below this the color and what lies below it are about equally bright, and there is no alpha to work out.
        private const float k_MinLuminanceRange = 0.0001f;

        // The 3D world behind the UI is dark space in every colorway. The colors of the UI over it were made for this.
        private static readonly Color s_space = new Color32(5, 6, 7, 255);

        [SerializeField] private ColorwayRole m_role;
        [Tooltip("The color of the role Fixed, the same in every colorway.")]
        [SerializeField] private Color m_fixedColor;
        [Tooltip("How much of the color shows over what lies below it, like opacity in CSS.")]
        [SerializeField, Range(0f, 1f)] private float m_opacity;
        [Tooltip("Mixes it with what lies below it into one opaque color, like a greyed-out element, instead of " +
            "drawing it see-through.")]
        [SerializeField] private bool m_isMixedOpaque;
        [Tooltip("Lies over the 3D world, dark in every colorway, instead of over the background of the colorway.")]
        [SerializeField] private bool m_isOverWorld;
        [Tooltip("How much white lies between the background and it, like the sections of the settings.")]
        [SerializeField, Range(0f, 1f)] private float m_underlayWhite;

        public ColorwayColor(ColorwayRole role)
            : this(role, Color.white, 1f, false, false, 0f)
        {
        }

        public ColorwayColor(ColorwayRole role, Color fixedColor, float opacity, bool isMixedOpaque, bool isOverWorld,
            float underlayWhite)
        {
            m_role = role;
            m_fixedColor = fixedColor;
            m_opacity = opacity;
            m_isMixedOpaque = isMixedOpaque;
            m_isOverWorld = isOverWorld;
            m_underlayWhite = underlayWhite;
        }

        public ColorwayRole Role => m_role;
        public float Opacity => m_opacity;

        /// <summary>
        /// The color in a colorway, as the UI draws it.
        /// </summary>
        public Color Resolve(ColorwayData colorway)
        {
            Color color = GetRoleColor(colorway);
            if (m_opacity >= 1f)
            {
                return color;
            }

            Color below = m_isOverWorld ? s_space : colorway.Background;
            Color underlay = Color.Lerp(below, Color.white, m_underlayWhite);
            if (m_isMixedOpaque)
            {
                Color mixed = Color.Lerp(underlay, color, m_opacity);
                mixed.a = 1f;
                return mixed;
            }

            color.a = GetLinearAlpha(color, underlay, m_opacity);
            return color;
        }

        /// <summary>
        /// The alpha at which a color over an underlay looks like it does in a browser at an opacity. Unity blends
        /// see-through UI in linear light, a browser in sRGB, so 7 % white over black looks like 30 % here.
        /// </summary>
        public static float GetLinearAlpha(Color color, Color underlay, float opacity)
        {
            float underlayLuminance = GetLuminance(underlay);
            float range = GetLuminance(color) - underlayLuminance;
            if (Mathf.Abs(range) < k_MinLuminanceRange)
            {
                return opacity;
            }

            float blended = GetLuminance(Color.Lerp(underlay, color, opacity));
            return Mathf.Clamp01((blended - underlayLuminance) / range);
        }

        /// <summary>
        /// The relative luminance of a color, from 0 for black to 1 for white, as the contrast of text counts it.
        /// </summary>
        public static float GetLuminance(Color color)
        {
            return 0.2126f * Mathf.GammaToLinearSpace(color.r) + 0.7152f * Mathf.GammaToLinearSpace(color.g) +
                0.0722f * Mathf.GammaToLinearSpace(color.b);
        }

        private Color GetRoleColor(ColorwayData colorway)
        {
            switch (m_role)
            {
                case ColorwayRole.Background:
                    return colorway.Background;

                case ColorwayRole.Glow:
                    return colorway.Glow;

                case ColorwayRole.GlowShadow:
                    return colorway.GlowShadow;

                case ColorwayRole.TextOnGlow:
                    return colorway.TextOnGlow;

                case ColorwayRole.Starlight:
                    return colorway.Starlight;

                case ColorwayRole.TextOnStarlight:
                    return colorway.TextOnStarlight;

                default:
                    return m_fixedColor;
            }
        }
    }
}
