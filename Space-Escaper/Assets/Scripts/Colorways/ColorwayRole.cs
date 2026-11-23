namespace SpaceEscaper
{
    /// <summary>
    /// Which color of a colorway an element of the UI takes. Plates, their text and the greys stay the same in every
    /// colorway, they take a fixed color.
    /// </summary>
    /// <remarks>
    /// The scene stores the number of a role, so new roles go at the end.
    /// </remarks>
    public enum ColorwayRole
    {
        Fixed,
        Background,
        Glow,
        GlowShadow,
        TextOnGlow,
        Starlight,
        TextOnStarlight
    }
}
