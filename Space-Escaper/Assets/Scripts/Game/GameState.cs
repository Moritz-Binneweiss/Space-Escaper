namespace SpaceEscaper
{
    /// <summary>
    /// Where the game is: in the main menu, in the hangar, in a run, in the pause or on the death screen.
    /// </summary>
    /// <remarks>
    /// The settings are not a state of their own. They open over the main menu or the pause, which
    /// stays the state while they are open.
    /// </remarks>
    public enum GameState
    {
        Menu,
        Hangar,
        Run,
        Paused,
        Dead,
    }
}
