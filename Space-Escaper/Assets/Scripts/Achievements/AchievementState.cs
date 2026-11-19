namespace SpaceEscaper
{
    /// <summary>
    /// Where an achievement stands, as the achievements screen shows it.
    /// </summary>
    public enum AchievementState
    {
        InProgress,

        /// <summary>
        /// Unlocked, with its reward still to claim.
        /// </summary>
        ToClaim,

        Claimed,

        /// <summary>
        /// Greyed out until what it counts is in the game.
        /// </summary>
        ComingSoon,
    }
}
