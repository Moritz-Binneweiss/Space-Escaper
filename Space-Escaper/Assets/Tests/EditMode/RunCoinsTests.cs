using NUnit.Framework;

namespace SpaceEscaper.Tests
{
    public class RunCoinsTests
    {
        [Test]
        public void DeathAddsTheCoinsOfTheRun()
        {
            SaveData saveData = SaveData.FromJson(null);
            RunCoins runCoins = new RunCoins();
            Collect(runCoins, 5);

            runCoins.BankInto(saveData);

            Assert.That(saveData.Coins, Is.EqualTo(5));
        }

        // A revive keeps the count going. Adding all of it again would give 13.
        [Test]
        public void DeathAfterReviveOnlyAddsTheNewCoins()
        {
            SaveData saveData = SaveData.FromJson(null);
            RunCoins runCoins = new RunCoins();
            Collect(runCoins, 5);
            runCoins.BankInto(saveData);
            Collect(runCoins, 3);

            runCoins.BankInto(saveData);

            Assert.That(runCoins.Count, Is.EqualTo(8));
            Assert.That(saveData.Coins, Is.EqualTo(8));
        }

        [Test]
        public void CoinsAddUpOverSeveralRuns()
        {
            SaveData saveData = SaveData.FromJson(null);
            RunCoins firstRun = new RunCoins();
            Collect(firstRun, 5);
            firstRun.BankInto(saveData);
            RunCoins secondRun = new RunCoins();
            Collect(secondRun, 3);

            secondRun.BankInto(saveData);

            Assert.That(saveData.Coins, Is.EqualTo(8));
        }

        private static void Collect(RunCoins runCoins, int count)
        {
            for (int i = 0; i < count; i++)
            {
                runCoins.Collect();
            }
        }
    }
}
