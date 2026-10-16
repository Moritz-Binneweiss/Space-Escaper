namespace SpaceEscaper
{
    /// <summary>
    /// Counts the coins of one run and adds them to the save on every death. A
    /// revive keeps the count going, so a second death only adds the coins
    /// collected since the first.
    /// </summary>
    public class RunCoins
    {
        private int m_count;
        private int m_bankedCount;

        public int Count => m_count;

        public void Collect()
        {
            m_count++;
        }

        /// <summary>
        /// Adds the coins collected since the last call to the save.
        /// </summary>
        public void BankInto(SaveData saveData)
        {
            saveData.AddCoins(m_count - m_bankedCount);
            m_bankedCount = m_count;
        }
    }
}
