using NUnit.Framework;

namespace SpaceEscaper.Tests
{
    public class SaveDataTests
    {
        private const string k_StarterShip = "AristocratSkin1";
        private const string k_CheapShip = "AristocratSkin2";
        private const string k_ExpensiveShip = "FreeterSkin1";
        private const string k_LastShip = "VagorSkin3";

        private static readonly string[] s_knownShips = { k_StarterShip, k_CheapShip, k_ExpensiveShip, k_LastShip };

        [TestCase(null)]
        [TestCase("")]
        [TestCase("{}")]
        public void NewInstallationStartsWithDefaults(string json)
        {
            SaveData saveData = SaveData.FromJson(json);

            saveData.RepairShips(k_StarterShip, s_knownShips);

            Assert.That(saveData.Coins, Is.EqualTo(0));
            Assert.That(saveData.Highscore, Is.EqualTo(0));
            Assert.That(saveData.CurrentShipId, Is.EqualTo(k_StarterShip));
            Assert.That(saveData.IsShipUnlocked(k_StarterShip), Is.True);
            Assert.That(saveData.IsShipUnlocked(k_CheapShip), Is.False);
            Assert.That(saveData.UseNewSounds, Is.True);
            Assert.That(saveData.IsMusicMuted, Is.False);
            Assert.That(saveData.IsSfxMuted, Is.False);
            Assert.That(saveData.MusicVolume, Is.EqualTo(1f));
            Assert.That(saveData.SfxVolume, Is.EqualTo(1f));
        }

        [Test]
        public void SaveCarriesVersionNumber()
        {
            string json = SaveData.FromJson(null).ToJson();

            StringAssert.Contains("\"m_version\": 2", json);
        }

        // A save of version 1, which stored ships as numbers.
        [Test]
        public void OlderSaveStartsOver()
        {
            SaveData saveData = SaveData.FromJson("{\"m_version\": 1, \"m_coins\": 264, \"m_highscore\": 3890, "
                + "\"m_currentShip\": 4, \"m_unlockedShips\": [1, 4]}");

            saveData.RepairShips(k_StarterShip, s_knownShips);

            Assert.That(saveData.Coins, Is.EqualTo(0));
            Assert.That(saveData.Highscore, Is.EqualTo(0));
            Assert.That(saveData.CurrentShipId, Is.EqualTo(k_StarterShip));
        }

        [TestCase("")]
        [TestCase("NoSuchShip")]
        [TestCase("aristocratskin2")]
        public void UnknownShipFallsBackToStarterShip(string shipId)
        {
            SaveData saveData = SaveData.FromJson("{\"m_currentShipId\": \"" + shipId + "\"}");

            saveData.RepairShips(k_StarterShip, s_knownShips);

            Assert.That(saveData.CurrentShipId, Is.EqualTo(k_StarterShip));
            Assert.That(saveData.IsShipUnlocked(k_StarterShip), Is.True);
        }

        [Test]
        public void StarterShipAndFlownShipCountAsBought()
        {
            SaveData saveData = SaveData.FromJson(
                "{\"m_currentShipId\": \"" + k_LastShip + "\", \"m_unlockedShipIds\": []}");

            saveData.RepairShips(k_StarterShip, s_knownShips);

            Assert.That(saveData.CurrentShipId, Is.EqualTo(k_LastShip));
            Assert.That(saveData.IsShipUnlocked(k_LastShip), Is.True);
            Assert.That(saveData.IsShipUnlocked(k_StarterShip), Is.True);
        }

        [TestCase(1000, 250)]
        [TestCase(750, 0)]
        public void PurchaseSpendsCoinsAndUnlocksShip(int coins, int coinsLeft)
        {
            SaveData saveData = CreateSaveWithCoins(coins);

            bool isBought = saveData.TryBuyShip(k_ExpensiveShip, 750);

            Assert.That(isBought, Is.True);
            Assert.That(saveData.Coins, Is.EqualTo(coinsLeft));
            Assert.That(saveData.IsShipUnlocked(k_ExpensiveShip), Is.True);
        }

        [Test]
        public void PurchaseWithoutEnoughCoinsChangesNothing()
        {
            SaveData saveData = CreateSaveWithCoins(749);

            bool isBought = saveData.TryBuyShip(k_ExpensiveShip, 750);

            Assert.That(isBought, Is.False);
            Assert.That(saveData.Coins, Is.EqualTo(749));
            Assert.That(saveData.IsShipUnlocked(k_ExpensiveShip), Is.False);
        }

        [Test]
        public void OwnedShipIsNotBoughtAgain()
        {
            SaveData saveData = CreateSaveWithCoins(1000);
            saveData.TryBuyShip(k_ExpensiveShip, 750);

            bool isBought = saveData.TryBuyShip(k_ExpensiveShip, 750);

            Assert.That(isBought, Is.False);
            Assert.That(saveData.Coins, Is.EqualTo(250));
        }

        [Test]
        public void PurchaseSurvivesSaveAndLoad()
        {
            SaveData saveData = CreateSaveWithCoins(300);
            saveData.TryBuyShip(k_CheapShip, 250);
            saveData.SelectShip(k_CheapShip);

            SaveData loaded = SaveData.FromJson(saveData.ToJson());
            loaded.RepairShips(k_StarterShip, s_knownShips);

            Assert.That(loaded.Coins, Is.EqualTo(50));
            Assert.That(loaded.CurrentShipId, Is.EqualTo(k_CheapShip));
            Assert.That(loaded.IsShipUnlocked(k_CheapShip), Is.True);
        }

        [Test]
        public void HighscoreOnlyGoesUp()
        {
            SaveData saveData = SaveData.FromJson(null);

            saveData.RecordScore(500);
            saveData.RecordScore(300);

            Assert.That(saveData.Highscore, Is.EqualTo(500));
        }

        [Test]
        public void AudioSettingsSurviveSaveAndLoad()
        {
            SaveData saveData = SaveData.FromJson(null);
            saveData.UseNewSounds = false;
            saveData.IsMusicMuted = true;
            saveData.IsSfxMuted = true;
            saveData.MusicVolume = 0.25f;
            saveData.SfxVolume = 0.75f;

            SaveData loaded = SaveData.FromJson(saveData.ToJson());

            Assert.That(loaded.UseNewSounds, Is.False);
            Assert.That(loaded.IsMusicMuted, Is.True);
            Assert.That(loaded.IsSfxMuted, Is.True);
            Assert.That(loaded.MusicVolume, Is.EqualTo(0.25f));
            Assert.That(loaded.SfxVolume, Is.EqualTo(0.75f));
        }

        [Test]
        public void AchievementsSurviveSaveAndLoad()
        {
            SaveData saveData = SaveData.FromJson(null);
            saveData.RecordFlight(true);
            saveData.RecordFlight(false);
            saveData.RecordCoin();
            saveData.RecordCrash(812);
            saveData.UnlockAchievement("Liftoff");
            saveData.UnlockAchievement("ItsWorking");
            saveData.TryClaimAchievement("Liftoff", 11);

            SaveData loaded = SaveData.FromJson(saveData.ToJson());

            Assert.That(loaded.Flights, Is.EqualTo(2));
            Assert.That(loaded.FlightsInBoughtShips, Is.EqualTo(1));
            Assert.That(loaded.CoinsCollected, Is.EqualTo(1));
            Assert.That(loaded.Crashes, Is.EqualTo(1));
            Assert.That(loaded.LongestRun, Is.EqualTo(812));
            Assert.That(loaded.IsAchievementUnlocked("ItsWorking"), Is.True);
            Assert.That(loaded.IsAchievementClaimed("ItsWorking"), Is.False);
            Assert.That(loaded.IsAchievementClaimed("Liftoff"), Is.True);
            Assert.That(loaded.Coins, Is.EqualTo(11));
        }

        // The achievements came with new fields and without a new version, so a save from before them loads as it
        // is, with nothing counted yet.
        [Test]
        public void SaveFromBeforeTheAchievementsKeepsWhatItHas()
        {
            SaveData saveData = SaveData.FromJson("{\"m_version\": 2, \"m_coins\": 264, \"m_highscore\": 3890, "
                + "\"m_currentShipId\": \"" + k_CheapShip + "\", \"m_unlockedShipIds\": [\"" + k_CheapShip + "\"]}");

            saveData.RepairShips(k_StarterShip, s_knownShips);

            Assert.That(saveData.Coins, Is.EqualTo(264));
            Assert.That(saveData.Highscore, Is.EqualTo(3890));
            Assert.That(saveData.IsShipUnlocked(k_CheapShip), Is.True);
            Assert.That(saveData.Flights, Is.Zero);
            Assert.That(saveData.Crashes, Is.Zero);
            Assert.That(saveData.CoinsCollected, Is.Zero);
            Assert.That(saveData.IsAchievementUnlocked("Liftoff"), Is.False);
        }

        private static SaveData CreateSaveWithCoins(int coins)
        {
            SaveData saveData = SaveData.FromJson(null);
            saveData.AddCoins(coins);
            return saveData;
        }
    }
}
