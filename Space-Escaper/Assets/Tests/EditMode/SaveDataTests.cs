using NUnit.Framework;

namespace SpaceEscaper.Tests
{
    public class SaveDataTests
    {
        // As in the scene: ARISTOCRAT 1-3, FREETER 4-6, VAGOR 7-9.
        private const int k_ShipCount = 9;

        [TestCase(null)]
        [TestCase("")]
        [TestCase("{}")]
        public void NewInstallationStartsWithDefaults(string json)
        {
            SaveData saveData = SaveData.FromJson(json);

            saveData.RepairShips(k_ShipCount);

            Assert.That(saveData.Coins, Is.EqualTo(0));
            Assert.That(saveData.Highscore, Is.EqualTo(0));
            Assert.That(saveData.CurrentShip, Is.EqualTo(SaveData.k_StarterShip));
            Assert.That(saveData.IsShipUnlocked(SaveData.k_StarterShip), Is.True);
            Assert.That(saveData.IsShipUnlocked(2), Is.False);
            Assert.That(saveData.UseNewSounds, Is.True);
            Assert.That(saveData.IsMusicMuted, Is.False);
            Assert.That(saveData.IsSfxMuted, Is.False);
            Assert.That(saveData.MasterVolume, Is.EqualTo(1f));
        }

        [Test]
        public void SaveCarriesVersionNumber()
        {
            string json = SaveData.FromJson(null).ToJson();

            StringAssert.Contains("\"m_version\": 1", json);
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(10)]
        [TestCase(42)]
        public void InvalidShipFallsBackToStarterShip(int ship)
        {
            SaveData saveData = SaveData.FromJson("{\"m_currentShip\": " + ship + "}");

            saveData.RepairShips(k_ShipCount);

            Assert.That(saveData.CurrentShip, Is.EqualTo(SaveData.k_StarterShip));
            Assert.That(saveData.IsShipUnlocked(SaveData.k_StarterShip), Is.True);
        }

        [Test]
        public void StarterShipAndFlownShipCountAsBought()
        {
            SaveData saveData = SaveData.FromJson("{\"m_currentShip\": 9, \"m_unlockedShips\": []}");

            saveData.RepairShips(k_ShipCount);

            Assert.That(saveData.CurrentShip, Is.EqualTo(9));
            Assert.That(saveData.IsShipUnlocked(9), Is.True);
            Assert.That(saveData.IsShipUnlocked(SaveData.k_StarterShip), Is.True);
        }

        [TestCase(1000, 250)]
        [TestCase(750, 0)]
        public void PurchaseSpendsCoinsAndUnlocksShip(int coins, int coinsLeft)
        {
            SaveData saveData = CreateSaveWithCoins(coins);

            bool isBought = saveData.TryBuyShip(4, 750);

            Assert.That(isBought, Is.True);
            Assert.That(saveData.Coins, Is.EqualTo(coinsLeft));
            Assert.That(saveData.IsShipUnlocked(4), Is.True);
        }

        [Test]
        public void PurchaseWithoutEnoughCoinsChangesNothing()
        {
            SaveData saveData = CreateSaveWithCoins(749);

            bool isBought = saveData.TryBuyShip(4, 750);

            Assert.That(isBought, Is.False);
            Assert.That(saveData.Coins, Is.EqualTo(749));
            Assert.That(saveData.IsShipUnlocked(4), Is.False);
        }

        [Test]
        public void OwnedShipIsNotBoughtAgain()
        {
            SaveData saveData = CreateSaveWithCoins(1000);
            saveData.TryBuyShip(4, 750);

            bool isBought = saveData.TryBuyShip(4, 750);

            Assert.That(isBought, Is.False);
            Assert.That(saveData.Coins, Is.EqualTo(250));
        }

        [Test]
        public void PurchaseSurvivesSaveAndLoad()
        {
            SaveData saveData = CreateSaveWithCoins(300);
            saveData.TryBuyShip(2, 250);
            saveData.SelectShip(2);

            SaveData loaded = SaveData.FromJson(saveData.ToJson());
            loaded.RepairShips(k_ShipCount);

            Assert.That(loaded.Coins, Is.EqualTo(50));
            Assert.That(loaded.CurrentShip, Is.EqualTo(2));
            Assert.That(loaded.IsShipUnlocked(2), Is.True);
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
            saveData.MasterVolume = 0.25f;

            SaveData loaded = SaveData.FromJson(saveData.ToJson());

            Assert.That(loaded.UseNewSounds, Is.False);
            Assert.That(loaded.IsMusicMuted, Is.True);
            Assert.That(loaded.IsSfxMuted, Is.True);
            Assert.That(loaded.MasterVolume, Is.EqualTo(0.25f));
        }

        private static SaveData CreateSaveWithCoins(int coins)
        {
            SaveData saveData = SaveData.FromJson(null);
            saveData.AddCoins(coins);
            return saveData;
        }
    }
}
