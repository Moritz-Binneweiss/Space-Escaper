namespace SpaceEscaper
{
    /// <summary>
    /// What an achievement counts. The assets store it as its number, so a new goal goes at the end.
    /// </summary>
    public enum AchievementGoal
    {
        /// <summary>
        /// Runs started, from the main menu or with Play again. A revive goes on with the run.
        /// </summary>
        Flights,

        /// <summary>
        /// Runs started in a ship other than the starter ship, so in one that was bought.
        /// </summary>
        FlightsInBoughtShips,

        /// <summary>
        /// Every crash, the ones before a revive too.
        /// </summary>
        Crashes,

        /// <summary>
        /// Meters of the longest run, counted when it crashes. A revive goes on with the run.
        /// </summary>
        LongestRun,

        /// <summary>
        /// Coins collected in runs. The rewards of achievements do not count.
        /// </summary>
        CoinsCollected,

        /// <summary>
        /// Ships owned, out of all ships of the game.
        /// </summary>
        ShipsOwned,

        /// <summary>
        /// Logbooks found. Like the cards, the plants and all collectibles, nothing counts them before the
        /// collectibles come with v1.8.0.
        /// </summary>
        Logbooks,

        StarSystemCards,

        Plants,

        Collectibles,

        /// <summary>
        /// The other achievements unlocked, out of all of them.
        /// </summary>
        OtherAchievements,
    }
}
