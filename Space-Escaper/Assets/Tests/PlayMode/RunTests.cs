using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace SpaceEscaper.Tests
{
    public class RunTests
    {
        private const string k_GameSceneName = "Game";
        private const string k_SaveFolderName = "PlayModeTests";
        private const string k_SaveFileName = "SaveData.json";

        // The same track sections on every run, so a failure can be replayed.
        private const int k_RandomSeed = 4711;

        // In game time, so a pause does not count.
        private const float k_MaxTimeToCrash = 60f;
        private const float k_TimeOnDeathScreen = 1f;
        private const float k_MaxTimeToFlyOn = 10f;

        // In real seconds, as the screens change in time that runs in the pause too. A change takes a fraction of
        // one, a reload through a button waits for the curtain to close first.
        private const float k_MaxTimeToChangeScreens = 5f;
        private const float k_MaxTimeToReload = 5f;

        // The curtain closes in 0.2 s and moves on by at most 1/30 s per frame, so a frame before it is closed it
        // covers at least 0.86.
        private const float k_NearlyClosed = 0.5f;

        // The platform in the hangar slides out and back in within 0.6 s.
        private const float k_MaxTimeToSlide = 5f;

        // A finger on the sheet of the pause, a little below its top edge, where the sheet shows only its ground. Its
        // way out is some 550 units, a quarter of it swipes the sheet away. A finger that rests before it lets go has
        // no speed left.
        private const float k_GrabberDepth = 10f;
        private const int k_DragSteps = 5;
        private const float k_ShortDrag = 40f;
        private const float k_LongDrag = 200f;
        private const float k_FingerRest = 0.2f;
        private const float k_FingerTolerance = 0.5f;

        // The knob of a switch slides over in 0.15 s, between two places 24 units apart.
        private const float k_SwitchTime = 0.3f;
        private const float k_KnobTolerance = 0.5f;

        // The first run beats it on the way to its first obstacle, at about 13 points.
        private const int k_HighscoreToBeat = 5;

        // Enough coins for a group of thousands: 1,234 in English, 1.234 in German.
        private const int k_GroupedCoins = 1234;

        // The achievements of the first flight, the third crash and the coins collected, which the runs of the test
        // reach.
        private const string k_FirstFlightId = "Liftoff";
        private const string k_ThirdCrashId = "AnotherHappyLanding";
        private const string k_CoinsId = "MoneyMoneyMoney";

        // Far enough ahead of the ship to be collected a good while before the first obstacle.
        private const float k_CoinDistance = 15f;

        // A popup of the run drops in from 14 units above its place in 0.5 s.
        private const float k_DropDuration = 0.5f;
        private const float k_DropHeight = 14f;

        // Unity keeps a position that a new one changes by less than about a millionth of it. A drop slows down to its
        // end, so its last step can be that small, and the popup stays as far short of its place, at -248.4 up to
        // 0.00025 units.
        private const float k_RestTolerance = 0.01f;

        // The test reads time and place after the frame of a tap, the score counts that frame
        // too. A frame of flight is a fraction of a unit, and the ship stops short of the full
        // step when it crashes.
        private const float k_DistanceTolerance = 1f;
        private const float k_TimeTolerance = 0.5f;

        // The fresh track after a revive starts at least 35 units ahead of the ship, and
        // obstacles reach at most 3 units back from the start of their tile. So a crash
        // within this distance can only come from the old track.
        private const float k_DistanceAfterRevive = 25f;

        // The world moves back every 20 units instead of every few thousand, so the run
        // crosses the floating origin on the way to the crash and again shortly after the
        // revive, while the explosion still burns.
        private const float k_FloatingOriginDistance = 20f;

        // The ship and the camera move a few units per frame at most. Whatever the floating
        // origin misses jumps by the whole distance.
        private const float k_MaxStepPerFrame = k_FloatingOriginDistance / 2f;
        private const float k_PlaceTolerance = 0.001f;

        // A new installation shows the default colorway. The test chooses one that differs from it in every color.
        private const string k_DefaultColorwayId = "Toxic";
        private const string k_ChosenColorwayId = "Nebula";
        private const string k_TestLanguageId = "en";
        private const string k_OtherLanguageId = "de";
        private const float k_ColorTolerance = 0.001f;

        private readonly Dictionary<Transform, float> m_places = new Dictionary<Transform, float>();
        private readonly List<TileManager> m_tileManagers = new List<TileManager>();
        private readonly List<Transform> m_groups = new List<Transform>();
        private string m_saveDirectory;
        private float m_listenerVolume;
        private PlayerLoopSystem m_originalPlayerLoop;
        private GameObject m_pendingTap;
        private PointerEventData m_pendingTapData;
        private Button m_continueButton;
        private Transform m_ship;
        private Transform m_camera;
        private float m_shiftedDistance;
        private int m_shiftCount;
        private float m_lastShipPlace;
        private float m_lastCameraPlace;

        [SetUp]
        public void SetUp()
        {
            // The test plays for real, and a death writes coins and highscore. So the
            // save goes to a folder of its own, which starts empty like a new install.
            m_saveDirectory = Path.Combine(Application.temporaryCachePath, k_SaveFolderName);
            if (Directory.Exists(m_saveDirectory))
            {
                Directory.Delete(m_saveDirectory, true);
            }

            Directory.CreateDirectory(m_saveDirectory);
            SaveSystem.LoadFrom(m_saveDirectory);

            // Played silently with the listener turned down. Music and sounds still go through the
            // same code as in the game, which a slider all the way down would not: it pauses them.
            m_listenerVolume = AudioListener.volume;
            AudioListener.volume = 0f;

            SaveSystem.Data.RecordScore(k_HighscoreToBeat);

            // The texts it checks are English, and the editor reports the language of Windows to the game.
            SaveSystem.Data.LanguageId = k_TestLanguageId;

#if UNITY_EDITOR || UNITY_ENABLE_CHECKS
            // The ship has to crash, whatever the test menu says.
            PlayerMotor.IsInvincible = false;
#endif

            m_places.Clear();
            m_shiftedDistance = 0f;
            m_shiftCount = 0;
            FloatingOrigin.WorldShifted += FloatingOrigin_WorldShifted;

            m_originalPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoop.SetPlayerLoop(AddToPreUpdate(PlayerLoop.GetCurrentPlayerLoop(), TapPendingButton));
        }

        // The save stays in the test folder: leaving Play Mode saves once more, and that
        // must not reach the real save. The next Play Mode loads the real one again.
        [TearDown]
        public void TearDown()
        {
            PlayerLoop.SetPlayerLoop(m_originalPlayerLoop);
            FloatingOrigin.WorldShifted -= FloatingOrigin_WorldShifted;
            AudioListener.volume = m_listenerVolume;
        }

        // Start, crash and revive through the buttons of the game. The death screen shows the
        // distance and the time flown and the beaten highscore, whose badge drops in after the
        // screen like the card of the first flight, the score stands still there,
        // and the revive must not crash the ship again: the obstacle that killed it is only
        // destroyed at the end of the frame, and the ship flies on in that same frame. All
        // along, the world moves back to the origin every few units, and nothing on screen may
        // jump.
        [UnityTest]
        public IEnumerator ShipCrashesAndFliesOnAfterRevive()
        {
            Random.InitState(k_RandomSeed);
            yield return SceneManager.LoadSceneAsync(k_GameSceneName);
            yield return null;

            GameManager gameManager = GameManager.Instance;
            RunScore runScore = Object.FindAnyObjectByType<RunScore>();
            m_ship = Object.FindAnyObjectByType<PlayerMotor>().transform;
            m_camera = Object.FindAnyObjectByType<CameraMotor>().transform;
            FloatingOrigin floatingOrigin = Object.FindAnyObjectByType<FloatingOrigin>();
            Assert.That(floatingOrigin, Is.Not.Null, "No FloatingOrigin in the scene");
            floatingOrigin.Distance = k_FloatingOriginDistance;
            FindTileManagersAndGroups();

            Button startButton = FindScreen<MainMenuScreen>().StartButton;
            Button pauseButton = FindScreen<GameMenuScreen>().PauseButton;
            DeathMenuScreen deathMenu = FindScreen<DeathMenuScreen>();
            Button reviveButton = deathMenu.ReviveButton;
            m_continueButton = FindScreen<PauseMenuScreen>().ContinueButton;
            RectTransform badge = (RectTransform)deathMenu.NewHighscoreBadge.transform;
            RectTransform card = (RectTransform)deathMenu.UnlockedCard.transform;
            Vector2 badgePlace = badge.anchoredPosition;
            Vector2 cardPlace = card.anchoredPosition;

            yield return Tap(startButton);
            Assert.That(gameManager.State, Is.EqualTo(GameState.Run), "Play did not start a run");
            Assert.That(pauseButton.gameObject.activeInHierarchy, Is.True, "No game menu during the run");

            // Straight down the middle lane until the first obstacle.
            float startTime = Time.time;
            WatchFromHere();
            float startPlace = GetPlace(m_ship);
            while (gameManager.State != GameState.Dead)
            {
                Assert.That(Time.time - startTime, Is.LessThan(k_MaxTimeToCrash), "No crash flying straight");
                AssertNothingJumped();
                yield return NextFrame();
            }

            float crashPlace = GetPlace(m_ship);
            float flightTime = Time.time - startTime;
            TestContext.WriteLine($"Crashed at z = {crashPlace:0.0} after {flightTime:0.0} s, "
                + $"the world moved back {m_shiftCount} times");
            Assert.That(reviveButton.gameObject.activeInHierarchy, Is.True, "No death menu with the revive button");
            AssertDeathIsSaved();
            Assert.That(runScore.Distance, Is.EqualTo(crashPlace - startPlace).Within(k_DistanceTolerance),
                "The distance is not the way the ship flew");
            Assert.That(runScore.FlightTime, Is.EqualTo(flightTime).Within(k_TimeTolerance),
                "The time is not the time the ship flew");
            Assert.That(deathMenu.NewHighscoreBadge.activeSelf, Is.True,
                "The beaten highscore is not on the death screen");
            AssertRunIsCounted(runScore, 1);
            Assert.That(SaveSystem.Data.IsAchievementUnlocked(k_FirstFlightId), Is.True,
                "The first flight did not unlock its achievement");
            Assert.That(deathMenu.UnlockedCard.activeSelf, Is.True,
                "The death screen does not show what the run unlocked");
            Assert.That(deathMenu.UnlockedTitleText.text, Is.EqualTo(FindAchievement(k_FirstFlightId).Title),
                "The card shows another achievement than the first flight");

            // Badge and card drop in after the screen. A frame after it came, they still wait unseen.
            yield return null;
            Assert.That(badge.GetComponent<CanvasGroup>().alpha, Is.EqualTo(0f), "The badge comes with the screen");
            Assert.That(card.GetComponent<CanvasGroup>().alpha, Is.EqualTo(0f), "The card comes with the screen");

            int deathScore = runScore.Score;
            yield return new WaitForSeconds(k_TimeOnDeathScreen);
            Assert.That(runScore.Score, Is.EqualTo(deathScore), "The score counted on after the crash");
            Assert.That(badge.GetComponent<CanvasGroup>().alpha, Is.EqualTo(1f), "The badge does not drop in");
            Assert.That(Vector2.Distance(badge.anchoredPosition, badgePlace), Is.LessThan(k_RestTolerance),
                "The badge does not drop into its place");
            Assert.That(card.GetComponent<CanvasGroup>().alpha, Is.EqualTo(1f), "The card does not drop in");
            Assert.That(Vector2.Distance(card.anchoredPosition, cardPlace), Is.LessThan(k_RestTolerance),
                "The card does not drop into its place");
            int shiftsBeforeRevive = m_shiftCount;
            WatchFromHere();
            yield return Tap(reviveButton);

            float reviveTime = Time.time;
            while (GetPlace(m_ship) < crashPlace + k_DistanceAfterRevive)
            {
                float distance = GetPlace(m_ship) - crashPlace;
                Assert.That(gameManager.State, Is.Not.EqualTo(GameState.Dead),
                    $"Crashed again {distance:0.0} units after the revive");
                Assert.That(Time.time - reviveTime, Is.LessThan(k_MaxTimeToFlyOn), "Did not fly on after the revive");
                AssertNothingJumped();
                yield return NextFrame();
            }

            Assert.That(pauseButton.gameObject.activeInHierarchy, Is.True, "No game menu after the revive");
            Assert.That(m_shiftCount, Is.GreaterThan(shiftsBeforeRevive),
                "The world did not move back after the revive");
        }

        // Play again and the hangar on the death screen reload the scene, like the way back to
        // the main menu, and go on from there right away.
        [UnityTest]
        public IEnumerator DeathScreenStartsANewRunOrOpensTheHangar()
        {
            Random.InitState(k_RandomSeed);
            yield return SceneManager.LoadSceneAsync(k_GameSceneName);
            yield return null;

            yield return Tap(FindScreen<MainMenuScreen>().StartButton);
            yield return FlyUntilCrash();
            GameManager crashedRun = GameManager.Instance;
            yield return Tap(FindScreen<DeathMenuScreen>().PlayAgainButton);
            yield return WaitForReload(crashedRun);
            RunScore runScore = Object.FindAnyObjectByType<RunScore>();
            Assert.That(GameManager.Instance.State, Is.EqualTo(GameState.Run), "Play again did not start a run");
            Assert.That(runScore.Coins, Is.EqualTo(0), "The new run started with coins");

            // The test is in the second frame of the new scene now. Both bring the time of loading it as
            // Time.deltaTime, so the run must not have counted anything yet.
            Assert.That(runScore.FlightTime, Is.Zero, "The new run counted the time of loading the scene");
            Assert.That(runScore.Distance, Is.Zero, "The new run started on the way");
            Assert.That(FindScreen<GameMenuScreen>().PauseButton.gameObject.activeInHierarchy, Is.True,
                "No game menu in the new run");

            yield return FlyUntilCrash();
            AssertRunIsCounted(Object.FindAnyObjectByType<RunScore>(), 2);
            Assert.That(FindScreen<DeathMenuScreen>().UnlockedCard.activeSelf, Is.False,
                "The second run shows what the first unlocked");
            crashedRun = GameManager.Instance;
            yield return Tap(FindScreen<DeathMenuScreen>().HangarButton);
            yield return WaitForReload(crashedRun);
            Assert.That(GameManager.Instance.State, Is.EqualTo(GameState.Hangar), "The hangar did not open");
        }

        // A save with two crashes, so the first run unlocks two achievements: the first flight as it starts and the
        // third crash as it ends. Their rewards wait on the medal in the main menu and at the top of the achievements
        // screen, and each is paid out once.
        [UnityTest]
        public IEnumerator RewardsWaitOnTheMedalUntilTheyAreClaimed()
        {
            SaveSystem.Data.RecordCrash(0);
            SaveSystem.Data.RecordCrash(0);
            Random.InitState(k_RandomSeed);
            yield return SceneManager.LoadSceneAsync(k_GameSceneName);
            yield return null;

            MainMenuScreen mainMenu = FindScreen<MainMenuScreen>();
            Assert.That(mainMenu.RewardsBadge.activeSelf, Is.False, "A save without achievements has rewards to claim");
            yield return Tap(mainMenu.StartButton);
            yield return FlyUntilCrash();
            DeathMenuScreen deathMenu = FindScreen<DeathMenuScreen>();
            Assert.That(deathMenu.UnlockedTitleText.text, Is.EqualTo(FindAchievement(k_FirstFlightId).Title),
                "The card does not show the first achievement of the run");
            Assert.That(deathMenu.UnlockedLabelText.text, Is.EqualTo("ACHIEVEMENT UNLOCKED · +1"),
                "The card does not count the second achievement of the run");
            GameManager crashedRun = GameManager.Instance;
            yield return Tap(deathMenu.MenuButton);
            yield return WaitForReload(crashedRun);

            mainMenu = FindScreen<MainMenuScreen>();
            Assert.That(mainMenu.RewardsBadge.activeSelf, Is.True, "The medal does not show the rewards to claim");
            Assert.That(mainMenu.RewardsText.text, Is.EqualTo("2"), "The medal counts the rewards wrong");
            yield return Tap(mainMenu.AchievementsButton);

            // The screen takes taps once it has been drawn, a frame later.
            yield return null;
            AchievementsScreen achievements = FindScreen<AchievementsScreen>();
            AchievementEntry entry = achievements.Entries[0];
            Assert.That(entry.Achievement.Id, Is.EqualTo(k_FirstFlightId), "The first flight is not at the top");
            Assert.That(achievements.Entries[1].Achievement.Id, Is.EqualTo(k_ThirdCrashId),
                "The second reward to claim is not right after the first");
            int coins = SaveSystem.Data.Coins;
            yield return Tap(entry.ClaimButton);
            Assert.That(SaveSystem.Data.Coins, Is.EqualTo(coins + entry.Achievement.Reward),
                "The reward was not paid out");
            Assert.That(SaveSystem.Data.IsAchievementClaimed(k_FirstFlightId), Is.True,
                "The reward is not marked claimed");
            Assert.That(entry.ClaimButton.gameObject.activeInHierarchy, Is.False, "The reward can be claimed again");
            AssertSaveIsWritten("The claim was not saved");

            yield return Tap(achievements.BackButton);
            Assert.That(mainMenu.RewardsText.text, Is.EqualTo("1"), "The medal still counts the claimed reward");
            Assert.That(SaveSystem.Data.Coins, Is.EqualTo(coins + entry.Achievement.Reward),
                "Going back changed the coins");
        }

        // A save one coin short of the achievement for coins, and a coin put in the way of the ship, as the straight
        // way to the first obstacle has none. The first flight drops in as the run starts, the coins as soon as the
        // first flight has risen out again. What unlocks in the middle of the run is saved at once, and the longest run
        // counts while the run grows.
        [UnityTest]
        public IEnumerator AchievementsPopUpAsTheRunUnlocksThem()
        {
            Random.InitState(k_RandomSeed);
            yield return SceneManager.LoadSceneAsync(k_GameSceneName);
            yield return null;

            AchievementData coinsAchievement = FindAchievement(k_CoinsId);
            for (int i = 1; i < coinsAchievement.Target; i++)
            {
                SaveSystem.Data.RecordCoin();
            }

            GameMenuScreen gameMenu = FindScreen<GameMenuScreen>();
            RunScore runScore = Object.FindAnyObjectByType<RunScore>();
            Transform ship = Object.FindAnyObjectByType<PlayerMotor>().transform;
            m_continueButton = FindScreen<PauseMenuScreen>().ContinueButton;
            yield return Tap(FindScreen<MainMenuScreen>().StartButton);

            // The popup comes at the end of the frame of the tap and drops in from above while it fades in.
            yield return null;
            Assert.That(gameMenu.AchievementPopup.activeSelf, Is.True, "The first flight does not pop up");
            Assert.That(gameMenu.AchievementTitleText.text, Is.EqualTo(FindAchievement(k_FirstFlightId).Title),
                "The popup shows another achievement than the first flight");
            CanvasGroup popup = gameMenu.AchievementPopup.GetComponent<CanvasGroup>();
            RectTransform popupPlace = (RectTransform)gameMenu.AchievementPopup.transform;
            float startHeight = popupPlace.anchoredPosition.y;
            Assert.That(popup.alpha, Is.LessThan(0.5f), "The popup does not fade in");
            float dropEnd = Time.time + k_DropDuration;
            while (Time.time < dropEnd)
            {
                yield return NextFrame();
            }

            Assert.That(popup.alpha, Is.EqualTo(1f), "The popup does not finish fading in");
            Assert.That(startHeight - popupPlace.anchoredPosition.y, Is.GreaterThan(k_DropHeight / 2f),
                "The popup does not drop in from above");
            float restHeight = popupPlace.anchoredPosition.y;
            string firstFlightTitle = gameMenu.AchievementTitleText.text;
            bool hasRisenOut = false;

            Coin coin = Object.FindAnyObjectByType<Coin>();
            Assert.That(coin, Is.Not.Null, "No coin on the track");
            coin.transform.position = ship.position + Vector3.forward * k_CoinDistance;

            float startTime = Time.time;
            int lastDistance = 0;
            bool isCoinChecked = false;
            bool hasCoinsPoppedUp = false;
            while (GameManager.Instance.State != GameState.Dead)
            {
                Assert.That(Time.time - startTime, Is.LessThan(k_MaxTimeToCrash), "No crash flying straight");
                Assert.That(SaveSystem.Data.LongestRun, Is.GreaterThanOrEqualTo(lastDistance),
                    "The longest run does not count while the run grows");
                if (runScore.Coins > 0 && !isCoinChecked)
                {
                    isCoinChecked = true;
                    Assert.That(SaveSystem.Data.IsAchievementUnlocked(k_CoinsId), Is.True,
                        "The coin did not unlock its achievement in the run");
                    AssertSavedFileHas(k_CoinsId, "The achievement of the coin was not saved at once");
                }

                // Its time up, the first flight rises while it fades out, before the coins drop in.
                bool isFirstFlightUp = gameMenu.AchievementPopup.activeSelf
                    && gameMenu.AchievementTitleText.text == firstFlightTitle;
                hasRisenOut |= isFirstFlightUp && popup.alpha < 1f && popupPlace.anchoredPosition.y > restHeight;
                hasCoinsPoppedUp |= gameMenu.AchievementPopup.activeSelf
                    && gameMenu.AchievementTitleText.text == coinsAchievement.Title;
                lastDistance = runScore.Distance;
                yield return NextFrame();
            }

            Assert.That(isCoinChecked, Is.True, "The ship did not collect the coin in its way");
            Assert.That(hasRisenOut, Is.True, "The first flight does not rise out of sight");
            Assert.That(hasCoinsPoppedUp, Is.True, "The coins did not pop up after the first flight");
            DeathMenuScreen deathMenu = FindScreen<DeathMenuScreen>();
            Assert.That(deathMenu.UnlockedTitleText.text, Is.EqualTo(FindAchievement(k_FirstFlightId).Title),
                "The card does not show the first achievement of the run");
            Assert.That(deathMenu.UnlockedLabelText.text, Is.EqualTo("ACHIEVEMENT UNLOCKED · +1"),
                "The card does not count the achievement of the coin");
        }

        // Every screen but the main menu is switched off until the game gets there.
        // Through the settings to the colorways and back with the buttons of the game. A card shows its colorway at
        // once on the screen it is on, on the screens that come on later and on the button of the hangar, which
        // showed the colorway of before already. It keeps the choice in the save and shows it again after a reload.
        [UnityTest]
        public IEnumerator ColorwaysRecolorTheWholeUi()
        {
            yield return SceneManager.LoadSceneAsync(k_GameSceneName);
            yield return null;

            MainMenuScreen mainMenu = FindScreen<MainMenuScreen>();
            SettingsScreen settings = FindScreen<SettingsScreen>();
            ColorwaysScreen colorways = FindScreen<ColorwaysScreen>();
            ShopScreen shop = FindScreen<ShopScreen>();
            Assert.That(ColorwayManager.Current.Id, Is.EqualTo(k_DefaultColorwayId), "A new install has a colorway");

            yield return Tap(mainMenu.HangarButton);
            yield return null;
            yield return Tap(shop.BackButton);
            yield return null;
            yield return Tap(mainMenu.SettingsButton);
            yield return null;
            Assert.That(settings.ColorwayText.text, Is.EqualTo(ColorwayManager.Current.Title));
            yield return Tap(settings.ColorwayButton);
            yield return null;

            ColorwayCard chosenCard = null;
            foreach (ColorwayCard card in colorways.Cards)
            {
                if (card.Colorway.Id == k_ChosenColorwayId)
                {
                    chosenCard = card;
                }
            }

            ColorwayData chosen = chosenCard.Colorway;
            yield return Tap(chosenCard.Button);
            Assert.That(ColorwayManager.Current, Is.EqualTo(chosen), "The card did not choose its colorway");
            Assert.That(SaveSystem.Data.ColorwayId, Is.EqualTo(chosen.Id));
            AssertSaveIsWritten("The colorway was not saved");
            foreach (ColorwayCard card in colorways.Cards)
            {
                Assert.That(card.Check.activeSelf, Is.EqualTo(card == chosenCard), $"The check at {card.name}");
            }

            AssertShowsColorway(colorways, chosen);

            yield return null;
            yield return Tap(colorways.BackButton);
            yield return null;
            Assert.That(settings.ColorwayText.text, Is.EqualTo(chosen.Title));
            AssertShowsColorway(settings, chosen);
            yield return Tap(settings.BackButton);
            yield return null;
            AssertShowsColorway(mainMenu, chosen);
            yield return Tap(mainMenu.HangarButton);
            yield return WaitForScreens();
            AssertShowsColorway(shop, chosen);
            Color buttonColor = shop.SelectOrBuyLook.Plate.color;
            Assert.That(GetColorDifference(buttonColor, chosen.Glow), Is.LessThan(k_ColorTolerance),
                "The hangar shows its button in the colorway of before");

            GameManager oldGameManager = GameManager.Instance;
            oldGameManager.ReturnToMenu();
            yield return WaitForReload(oldGameManager);
            Assert.That(ColorwayManager.Current, Is.EqualTo(chosen), "The reload forgot the colorway");
            AssertShowsColorway(FindScreen<MainMenuScreen>(), chosen);
        }

        // The settings fade in over the main menu and out again. The achievements slide in from the left, the side of
        // their button, while the main menu fades out, and back out to the left while it fades in again. The screen
        // leaving takes no more taps. The scene starts behind the curtain, and the hangar opens behind it, which takes
        // the taps while it closes.
        [UnityTest]
        public IEnumerator ScreensSlideFadeAndChangeBehindTheCurtain()
        {
            yield return SceneManager.LoadSceneAsync(k_GameSceneName);
            ScreenManager screens = Object.FindAnyObjectByType<ScreenManager>();
            Assert.That(screens.Curtain.activeSelf, Is.True, "The scene does not start behind the curtain");
            yield return WaitForScreens();
            Assert.That(screens.Curtain.activeSelf, Is.False, "The curtain stays");

            MainMenuScreen mainMenu = FindScreen<MainMenuScreen>();
            SettingsScreen settings = FindScreen<SettingsScreen>();
            CanvasGroup settingsGroup = settings.GetComponent<CanvasGroup>();
            RectTransform settingsPlace = (RectTransform)settings.transform;
            yield return Tap(mainMenu.SettingsButton);
            Assert.That(mainMenu.gameObject.activeSelf, Is.True, "The main menu is gone before the settings are in");
            Assert.That(settingsGroup.alpha, Is.LessThan(1f), "The settings do not fade in");
            Assert.That(settingsPlace.anchoredPosition, Is.EqualTo(Vector2.zero),
                "The settings move instead of fading");
            AssertTapMisses(mainMenu.StartButton, "The main menu takes taps while it leaves");
            yield return WaitForScreens();
            Assert.That(mainMenu.gameObject.activeSelf, Is.False, "The main menu stays on under the settings");
            AssertAtRest(settings);

            yield return Tap(settings.BackButton);
            Assert.That(settingsGroup.alpha, Is.LessThan(1f), "The settings do not fade out");
            Assert.That(settingsPlace.anchoredPosition, Is.EqualTo(Vector2.zero),
                "The settings move instead of fading");
            Assert.That(mainMenu.gameObject.activeSelf, Is.True, "The main menu does not come back under the settings");
            AssertTapMisses(settings.BackButton, "The settings take taps while they leave");
            yield return WaitForScreens();
            Assert.That(settings.gameObject.activeSelf, Is.False, "The settings stay on");
            AssertAtRest(mainMenu);

            AchievementsScreen achievements = FindScreen<AchievementsScreen>();
            RectTransform achievementsPlace = (RectTransform)achievements.transform;
            yield return Tap(mainMenu.AchievementsButton);
            Assert.That(achievementsPlace.anchoredPosition.x, Is.LessThan(0f),
                "The achievements come not from the left");
            Assert.That(mainMenu.gameObject.activeSelf, Is.True,
                "The main menu is gone before the achievements are in");
            AssertTapMisses(mainMenu.StartButton, "The main menu takes taps while it leaves");
            yield return WaitForScreens();
            Assert.That(mainMenu.gameObject.activeSelf, Is.False, "The main menu stays on under the achievements");
            AssertAtRest(achievements);

            yield return Tap(achievements.BackButton);
            Assert.That(achievementsPlace.anchoredPosition.x, Is.LessThan(0f),
                "The achievements do not leave to the left");
            Assert.That(mainMenu.gameObject.activeSelf, Is.True,
                "The main menu does not come back under the achievements");
            AssertTapMisses(achievements.BackButton, "The achievements take taps while they leave");
            yield return WaitForScreens();
            Assert.That(achievements.gameObject.activeSelf, Is.False, "The achievements stay on");
            AssertAtRest(mainMenu);

            // The curtain takes taps once it has been drawn, a frame later, like a screen.
            yield return Tap(mainMenu.HangarButton);
            Assert.That(screens.Curtain.activeSelf, Is.True, "The hangar does not open behind the curtain");
            yield return null;
            Assert.That(GameManager.Instance.State, Is.EqualTo(GameState.Menu), "The hangar opens before the curtain");
            AssertTapMisses(mainMenu.StartButton, "The curtain lets taps through while it closes");
            CanvasGroup curtain = screens.Curtain.GetComponent<CanvasGroup>();
            float curtainBefore = 0f;
            float startTime = Time.realtimeSinceStartup;
            while (GameManager.Instance.State != GameState.Hangar)
            {
                Assert.That(Time.realtimeSinceStartup - startTime, Is.LessThan(k_MaxTimeToChangeScreens),
                    "The hangar does not open");
                curtainBefore = curtain.alpha;
                yield return null;
            }

            // In the frame before, the curtain was nearly closed, even at a frame rate the change only moves on with.
            Assert.That(curtainBefore, Is.GreaterThan(k_NearlyClosed), "The hangar opens before the curtain closed");
            Assert.That(curtain.alpha, Is.EqualTo(1f), "The hangar opens while the curtain does not cover it");
            Assert.That(mainMenu.gameObject.activeSelf, Is.False, "The main menu is still on behind the curtain");
            yield return WaitForScreens();
            AssertAtRest(FindScreen<ShopScreen>());
        }

        // The pause rises from below like a tablet while its dim fades in over the run, and sinks back down when the
        // run goes on. The screen leaving takes no more taps.
        [UnityTest]
        public IEnumerator ThePauseRisesFromBelowLikeATablet()
        {
            Random.InitState(k_RandomSeed);
            yield return SceneManager.LoadSceneAsync(k_GameSceneName);
            yield return null;

            GameMenuScreen gameMenu = FindScreen<GameMenuScreen>();
            PauseMenuScreen pauseMenu = FindScreen<PauseMenuScreen>();
            RectTransform sheet = Object.FindAnyObjectByType<ScreenManager>().PauseSheet;
            Vector2 sheetPlace = sheet.anchoredPosition;
            CanvasRenderer dim = pauseMenu.GetComponent<Graphic>().canvasRenderer;
            m_continueButton = pauseMenu.ContinueButton;
            yield return Tap(FindScreen<MainMenuScreen>().StartButton);

            yield return Tap(gameMenu.PauseButton);
            Assert.That(GameManager.Instance.State, Is.EqualTo(GameState.Paused), "The tap does not pause the run");
            Assert.That(sheet.anchoredPosition.y, Is.LessThan(sheetPlace.y), "The pause does not rise from below");
            Assert.That(sheet.anchoredPosition.x, Is.EqualTo(sheetPlace.x), "The pause does not rise straight up");
            Assert.That(dim.GetAlpha(), Is.LessThan(1f), "The dim of the pause does not fade in");
            Assert.That(gameMenu.gameObject.activeSelf, Is.True, "The run is gone before the pause is up");
            AssertTapMisses(gameMenu.PauseButton, "The run takes taps while it leaves");
            yield return WaitForScreens();
            Assert.That(sheet.anchoredPosition, Is.EqualTo(sheetPlace), "The pause does not rise into its place");
            Assert.That(dim.GetAlpha(), Is.EqualTo(1f), "The dim of the pause does not finish fading in");
            Assert.That(gameMenu.gameObject.activeSelf, Is.False, "The run stays on under the pause");
            AssertAtRest(pauseMenu);

            yield return Tap(pauseMenu.ContinueButton);
            Assert.That(GameManager.Instance.State, Is.EqualTo(GameState.Run), "The tap does not go on with the run");
            Assert.That(sheet.anchoredPosition.y, Is.LessThan(sheetPlace.y), "The pause does not sink back down");
            Assert.That(pauseMenu.gameObject.activeSelf, Is.True, "The pause is gone before it is down");
            AssertTapMisses(pauseMenu.ContinueButton, "The pause takes taps while it leaves");
            yield return WaitForScreens();
            Assert.That(pauseMenu.gameObject.activeSelf, Is.False, "The pause stays on");
            Assert.That(sheet.anchoredPosition, Is.EqualTo(sheetPlace), "The sheet stays out of its place");
            Assert.That(dim.GetAlpha(), Is.EqualTo(1f), "The dim stays faded for the next pause");
            AssertAtRest(gameMenu);
        }

        // A finger swipes the pause away downwards like a tablet put down. The sheet follows it but not above its
        // place. A short drag let go after a rest springs back and keeps the pause, a long one goes on down from where
        // the finger let go, and the run goes on. A drag reaches the pause also where the sheet shows only its ground.
        [UnityTest]
        public IEnumerator ThePauseCanBeSwipedAway()
        {
            Random.InitState(k_RandomSeed);
            yield return SceneManager.LoadSceneAsync(k_GameSceneName);
            yield return null;

            ScreenManager screens = Object.FindAnyObjectByType<ScreenManager>();
            RectTransform sheet = screens.PauseSheet;
            Vector2 sheetPlace = sheet.anchoredPosition;
            float scale = screens.GetComponent<Canvas>().scaleFactor;
            PauseMenuScreen pauseMenu = FindScreen<PauseMenuScreen>();
            GameObject pause = pauseMenu.gameObject;
            CanvasRenderer dim = pauseMenu.GetComponent<Graphic>().canvasRenderer;
            m_continueButton = pauseMenu.ContinueButton;
            yield return Tap(FindScreen<MainMenuScreen>().StartButton);
            yield return Tap(FindScreen<GameMenuScreen>().PauseButton);
            yield return WaitForScreens();

            Vector3 grabber = sheet.TransformPoint(new Vector3(0f, sheet.rect.yMax - k_GrabberDepth, 0f));
            PointerEventData finger = new PointerEventData(EventSystem.current)
            {
                position = RectTransformUtility.WorldToScreenPoint(null, grabber),
            };
            List<RaycastResult> hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(finger, hits);
            Assert.That(hits, Is.Not.Empty, "Nothing on the sheet takes a finger");
            Assert.That(ExecuteEvents.GetEventHandler<IDragHandler>(hits[0].gameObject), Is.EqualTo(pause),
                "A drag on the sheet does not reach the pause");

            ExecuteEvents.Execute(pause, finger, ExecuteEvents.beginDragHandler);
            yield return DragFinger(pause, finger, -k_ShortDrag, scale);
            Assert.That(sheet.anchoredPosition.y, Is.EqualTo(sheetPlace.y - k_ShortDrag).Within(k_FingerTolerance),
                "The sheet does not follow the finger");
            Assert.That(dim.GetAlpha(), Is.LessThan(1f), "The dim does not fade with the sheet");
            yield return new WaitForSecondsRealtime(k_FingerRest);
            ExecuteEvents.Execute(pause, finger, ExecuteEvents.endDragHandler);
            Assert.That(GameManager.Instance.State, Is.EqualTo(GameState.Paused), "A short drag swipes the pause away");
            yield return WaitForScreens();
            Assert.That(sheet.anchoredPosition, Is.EqualTo(sheetPlace),
                "The sheet does not spring back into its place");
            Assert.That(dim.GetAlpha(), Is.EqualTo(1f), "The dim does not come back with the sheet");

            ExecuteEvents.Execute(pause, finger, ExecuteEvents.beginDragHandler);
            yield return DragFinger(pause, finger, k_ShortDrag, scale);
            Assert.That(sheet.anchoredPosition, Is.EqualTo(sheetPlace), "The sheet goes above its place");
            ExecuteEvents.Execute(pause, finger, ExecuteEvents.endDragHandler);

            ExecuteEvents.Execute(pause, finger, ExecuteEvents.beginDragHandler);
            yield return DragFinger(pause, finger, -k_LongDrag, scale);
            float letGo = sheet.anchoredPosition.y;
            ExecuteEvents.Execute(pause, finger, ExecuteEvents.endDragHandler);
            Assert.That(GameManager.Instance.State, Is.EqualTo(GameState.Run),
                "A long drag does not swipe the pause away");
            yield return null;
            Assert.That(sheet.anchoredPosition.y, Is.LessThan(letGo),
                "The sheet does not go on down from where the finger let go");
            yield return WaitForScreens();
            Assert.That(pause.activeSelf, Is.False, "The pause stays on");
            Assert.That(sheet.anchoredPosition, Is.EqualTo(sheetPlace), "The sheet stays out of its place");
            AssertAtRest(FindScreen<GameMenuScreen>());
        }

        // The arrow slides the platform with the ship out of sight along the floor towards the bottom of the screen,
        // like on a conveyor belt, and the next family in from there. The family changes once the platform is out of
        // sight, and a tap on an arrow during the slide does nothing.
        [UnityTest]
        public IEnumerator TheHangarSlidesToTheNextFamily()
        {
            yield return SceneManager.LoadSceneAsync(k_GameSceneName);
            yield return null;
            yield return Tap(FindScreen<MainMenuScreen>().HangarButton);
            yield return WaitForScreens();

            ShopScreen shop = FindScreen<ShopScreen>();
            Hangar hangar = Object.FindAnyObjectByType<Hangar>();
            Camera view = hangar.ShopCamera;
            Transform platform = hangar.Platform;
            Vector3 platformRest = platform.position;
            Vector3 standRest = hangar.Stand.position;
            float restHeight = view.WorldToViewportPoint(platformRest).y;
            ShipFamily family = shop.ShownShip.Family;
            int familyCount = System.Enum.GetValues(typeof(ShipFamily)).Length;
            ShipFamily nextFamily = (ShipFamily)(((int)family + 1) % familyCount);

            yield return Tap(shop.NextFamilyButton);
            Assert.That(hangar.IsSliding, Is.True, "The platform does not slide");
            Assert.That(view.WorldToViewportPoint(platform.position).y, Is.LessThan(restHeight),
                "The platform does not leave towards the bottom of the screen");
            Assert.That(platform.position.y, Is.EqualTo(platformRest.y).Within(k_PlaceTolerance),
                "The platform leaves the floor");
            Assert.That(shop.ShownShip.Family, Is.EqualTo(family), "The family changes before the platform is out");
            yield return Tap(shop.NextFamilyButton);

            bool hasChanged = false;
            float startTime = Time.time;
            while (hangar.IsSliding)
            {
                Assert.That(Time.time - startTime, Is.LessThan(k_MaxTimeToSlide), "The platform does not stop");
                if (!hasChanged && shop.ShownShip.Family != family)
                {
                    hasChanged = true;
                    Assert.That(view.WorldToViewportPoint(platform.position).y, Is.LessThan(restHeight),
                        "The next family does not come in from the bottom of the screen");
                    AssertOutOfSight(hangar, "The family changes in sight");
                }

                yield return null;
            }

            Assert.That(hasChanged, Is.True, "The family does not change while the platform slides");
            Assert.That(shop.ShownShip.Family, Is.EqualTo(nextFamily), "The tap during the slide changed the family");
            Assert.That(Vector3.Distance(platform.position, platformRest), Is.LessThan(k_PlaceTolerance),
                "The platform does not come back to its place");
            Assert.That(Vector3.Distance(hangar.Stand.position, standRest), Is.LessThan(k_PlaceTolerance),
                "The stand does not come back to its place");
        }

        // A switch in the settings slides its knob over instead of letting it jump. Switched back at the end, so the
        // sounds stay as they were.
        [UnityTest]
        public IEnumerator SwitchesSlideTheirKnob()
        {
            yield return SceneManager.LoadSceneAsync(k_GameSceneName);
            yield return null;
            yield return Tap(FindScreen<MainMenuScreen>().SettingsButton);
            Toggle toggle = FindScreen<SettingsScreen>().ClassicModeToggle;
            RectTransform knob = toggle.GetComponent<ToggleSwitch>().Knob;
            yield return WaitForScreens();

            bool wasOn = toggle.isOn;
            float startPlace = knob.anchoredPosition.x;
            yield return Tap(toggle);
            Assert.That(toggle.isOn, Is.Not.EqualTo(wasOn), "The tap does not switch it");
            List<float> places = new List<float> { knob.anchoredPosition.x };
            float startTime = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - startTime < k_SwitchTime)
            {
                yield return null;
                places.Add(knob.anchoredPosition.x);
            }

            float endPlace = places[places.Count - 1];
            Assert.That(Mathf.Abs(endPlace - startPlace), Is.GreaterThan(k_KnobTolerance), "The knob does not move");
            Assert.That(places.Exists(place => Mathf.Abs(place - startPlace) > k_KnobTolerance
                && Mathf.Abs(place - endPlace) > k_KnobTolerance), Is.True, "The knob jumps instead of sliding");
            yield return Tap(toggle);
        }

        // Moritz, 25.11.2026: one tap switches the language. The button of the chosen one stands in the glow color, the
        // choice is saved right away, and a reload keeps it. Numbers take the separators of the language.
        [UnityTest]
        public IEnumerator TheLanguageButtonsSwitchTheLanguage()
        {
            SaveSystem.Data.AddCoins(k_GroupedCoins);
            yield return SceneManager.LoadSceneAsync(k_GameSceneName);
            yield return null;

            Assert.That(LanguageManager.Current.Id, Is.EqualTo(k_TestLanguageId), "The test is not in its language");
            Assert.That(FindScreen<MainMenuScreen>().CoinText.text, Is.EqualTo("1,234"),
                "The main menu does not group the coins");
            yield return Tap(FindScreen<MainMenuScreen>().SettingsButton);
            yield return null;
            SettingsScreen settings = FindScreen<SettingsScreen>();
            AssertShowsLanguage(settings, k_TestLanguageId);

            yield return Tap(GetLanguageButton(settings, k_OtherLanguageId).Button);
            Assert.That(LanguageManager.Current.Id, Is.EqualTo(k_OtherLanguageId), "The button did not choose it");
            Assert.That(SaveSystem.Data.LanguageId, Is.EqualTo(k_OtherLanguageId));
            AssertSaveIsWritten("The language was not saved");
            AssertShowsLanguage(settings, k_OtherLanguageId);

            GameManager oldGameManager = GameManager.Instance;
            oldGameManager.ReturnToMenu();
            yield return WaitForReload(oldGameManager);
            Assert.That(LanguageManager.Current.Id, Is.EqualTo(k_OtherLanguageId), "The reload forgot the language");
            Assert.That(FindScreen<MainMenuScreen>().CoinText.text, Is.EqualTo("1.234"),
                "The main menu does not group the coins as the language does");

            yield return Tap(FindScreen<MainMenuScreen>().SettingsButton);
            yield return null;
            settings = FindScreen<SettingsScreen>();
            AssertShowsLanguage(settings, k_OtherLanguageId);
            yield return Tap(GetLanguageButton(settings, k_TestLanguageId).Button);
            Assert.That(LanguageManager.Current.Id, Is.EqualTo(k_TestLanguageId), "The button did not choose it back");
            AssertShowsLanguage(settings, k_TestLanguageId);
        }

        private static T FindScreen<T>() where T : Object
        {
            T screen = Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);
            Assert.That(screen, Is.Not.Null, $"No {typeof(T).Name} in the scene");
            return screen;
        }

        // Taps the middle of the button at the start of the next frame, before any
        // Update(), as a real tap does: the EventSystem runs before the game scripts
        // (execution order -1000). A tap from the test itself would come after
        // Update(), too late to see a crash in the frame of the tap.
        private IEnumerator Tap(Selectable target)
        {
            yield return WaitForScreens();
            PointerEventData eventData = AimAt(target);
            Assert.That(GetTapped(eventData), Is.EqualTo(target.gameObject),
                $"A tap on {target.name} does not reach it");

            m_pendingTap = target.gameObject;
            m_pendingTapData = eventData;
            yield return null;
        }

        // The UI is a screen space overlay, so its points need no camera.
        private static PointerEventData AimAt(Selectable target)
        {
            RectTransform rectTransform = (RectTransform)target.transform;
            Vector3 center = rectTransform.TransformPoint(rectTransform.rect.center);
            return new PointerEventData(EventSystem.current)
            {
                position = RectTransformUtility.WorldToScreenPoint(null, center),
            };
        }

        // What a tap there would click: the first thing it hits, or what that lies in.
        private static GameObject GetTapped(PointerEventData eventData)
        {
            List<RaycastResult> hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, hits);
            return hits.Count > 0 ? ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) : null;
        }

        private static void AssertTapMisses(Selectable target, string message)
        {
            Assert.That(GetTapped(AimAt(target)), Is.Not.EqualTo(target.gameObject), message);
        }

        // Nothing of the platform and the ship on it lies in the view of the hangar camera.
        private static void AssertOutOfSight(Hangar hangar, string message)
        {
            Plane[] view = GeometryUtility.CalculateFrustumPlanes(hangar.ShopCamera);
            List<Renderer> renderers = new List<Renderer>(hangar.Platform.GetComponentsInChildren<Renderer>());
            renderers.AddRange(hangar.Stand.GetComponentsInChildren<Renderer>());
            foreach (Renderer renderer in renderers)
            {
                Assert.That(GeometryUtility.TestPlanesAABB(view, renderer.bounds), Is.False,
                    $"{message}: {renderer.name}");
            }
        }

        // A finger on the pause moves by this many units, a step per frame, the way the EventSystem passes it on.
        private static IEnumerator DragFinger(GameObject target, PointerEventData finger, float units, float scale)
        {
            for (int i = 0; i < k_DragSteps; i++)
            {
                finger.delta = new Vector2(0f, units * scale / k_DragSteps);
                finger.position += finger.delta;
                ExecuteEvents.Execute(target, finger, ExecuteEvents.dragHandler);
                yield return null;
            }
        }

        // Fully shown, in its place and taking taps, as the screen is saved.
        private static void AssertAtRest(Component screen)
        {
            CanvasGroup group = screen.GetComponent<CanvasGroup>();
            Assert.That(screen.gameObject.activeSelf, Is.True, $"{screen.name} is off");
            Assert.That(group.alpha, Is.EqualTo(1f), $"{screen.name} is not fully shown");
            Assert.That(group.blocksRaycasts, Is.True, $"{screen.name} takes no taps");
            Assert.That(((RectTransform)screen.transform).anchoredPosition, Is.EqualTo(Vector2.zero),
                $"{screen.name} is out of its place");
        }

        // Straight down the middle lane until the next obstacle. The death screen takes taps once it has been drawn,
        // a frame later.
        private IEnumerator FlyUntilCrash()
        {
            m_continueButton = FindScreen<PauseMenuScreen>().ContinueButton;
            float startTime = Time.time;
            while (GameManager.Instance.State != GameState.Dead)
            {
                Assert.That(Time.time - startTime, Is.LessThan(k_MaxTimeToCrash), "No crash flying straight");
                yield return NextFrame();
            }

            yield return null;
        }

        // Every element with a role on a screen that is on shows it in the colorway, a glow with the image of its
        // color.
        private static void AssertShowsColorway(Component screen, ColorwayData colorway)
        {
            ColorwayTint[] tints = screen.GetComponentsInChildren<ColorwayTint>();
            Assert.That(tints, Is.Not.Empty, $"Nothing on {screen.name} follows the colorway");
            foreach (ColorwayTint tint in tints)
            {
                Graphic graphic = tint.GetComponent<Graphic>();
                Assert.That(GetColorDifference(graphic.color, tint.GetColor(colorway)), Is.LessThan(k_ColorTolerance),
                    $"{tint.name} on {screen.name} is not in {colorway.Title}");
                if (tint.IsGlow)
                {
                    Assert.That(((Image)graphic).sprite, Is.EqualTo(tint.GetGlowImage(colorway)),
                        $"{tint.name} on {screen.name} shows the glow of another colorway");
                }
            }
        }

        private static LanguageButton GetLanguageButton(SettingsScreen settings, string languageId)
        {
            foreach (LanguageButton button in settings.LanguageButtons)
            {
                if (button.Language.Id == languageId)
                {
                    return button;
                }
            }

            Assert.Fail("The settings have no button for " + languageId);
            return null;
        }

        // The button of the language in the glow color, the others white, and every text of the screen in the language.
        private static void AssertShowsLanguage(SettingsScreen settings, string languageId)
        {
            foreach (LanguageButton button in settings.LanguageButtons)
            {
                bool isChosen = button.Language.Id == languageId;
                Color expected = isChosen ? ColorwayManager.Current.Glow : Color.white;
                Assert.That(GetColorDifference(button.Plate.color, expected), Is.LessThan(k_ColorTolerance),
                    $"The button of {button.Language.Id} is not {(isChosen ? "in the glow color" : "white")}");
            }

            foreach (LocalizedText text in settings.GetComponentsInChildren<LocalizedText>())
            {
                string expected = LanguageManager.Get(text.Key);
                if (expected.IndexOf('{') < 0)
                {
                    Assert.That(text.Text.text, Is.EqualTo(expected), $"{text.name} is not in {languageId}");
                }
            }
        }

        private static float GetColorDifference(Color a, Color b)
        {
            return Mathf.Max(Mathf.Max(Mathf.Abs(a.r - b.r), Mathf.Abs(a.g - b.g)),
                Mathf.Max(Mathf.Abs(a.b - b.b), Mathf.Abs(a.a - b.a)));
        }

        // The new scene wakes before the old one is gone, and goes on in its first frame.
        private static IEnumerator WaitForReload(GameManager oldGameManager)
        {
            float startTime = Time.realtimeSinceStartup;
            while (GameManager.Instance == oldGameManager)
            {
                Assert.That(Time.realtimeSinceStartup - startTime, Is.LessThan(k_MaxTimeToReload),
                    "The scene did not reload");
                yield return null;
            }

            yield return null;
        }

        // A player taps once the screen stands, after a screen has slid or faded in and the curtain has opened.
        private static IEnumerator WaitForScreens()
        {
            ScreenManager screens = Object.FindAnyObjectByType<ScreenManager>();
            float startTime = Time.realtimeSinceStartup;
            while (screens.IsChanging)
            {
                Assert.That(Time.realtimeSinceStartup - startTime, Is.LessThan(k_MaxTimeToChangeScreens),
                    "The screens do not stop changing");
                yield return null;
            }
        }

        // Losing focus in the editor pauses the run, as leaving the app does on a phone.
        // The test then taps Continue like a player would, once the pause menu has been drawn:
        // a screen switched on in this frame takes no taps yet.
        private IEnumerator NextFrame()
        {
            if (GameManager.Instance.State == GameState.Paused)
            {
                TestContext.WriteLine("Paused by a focus change, tapping Continue");
                yield return null;
                yield return Tap(m_continueButton);
            }
            else
            {
                yield return null;
            }
        }

        private void TapPendingButton()
        {
            if (m_pendingTap == null)
            {
                return;
            }

            GameObject button = m_pendingTap;
            m_pendingTap = null;
            ExecuteEvents.Execute(button, m_pendingTapData, ExecuteEvents.pointerClickHandler);
        }

        // A death writes coins and highscore right away, here into the test folder.
        private void AssertDeathIsSaved()
        {
            Assert.That(SaveSystem.Data.Highscore, Is.GreaterThan(0), "The death did not record the score");
            AssertSaveIsWritten("The death was not saved");
        }

        private void AssertSaveIsWritten(string message)
        {
            string path = Path.Combine(m_saveDirectory, k_SaveFileName);
            Assert.That(File.Exists(path), Is.True, message);
            Assert.That(File.ReadAllText(path), Is.EqualTo(SaveSystem.Data.ToJson()), message + ": out of date");
        }

        // What the achievements count after the crash of a run in the starter ship, the runs before counted too. The
        // test always flies straight into the first obstacle, so every run is about as long.
        private static void AssertRunIsCounted(RunScore runScore, int runs)
        {
            SaveData saveData = SaveSystem.Data;
            Assert.That(saveData.Flights, Is.EqualTo(runs), "The flights are not counted");
            Assert.That(saveData.FlightsInBoughtShips, Is.Zero, "The starter ship counted as bought");
            Assert.That(saveData.Crashes, Is.EqualTo(runs), "The crashes are not counted");
            Assert.That(saveData.LongestRun, Is.GreaterThanOrEqualTo(runScore.Distance),
                "The distance of the run is not counted");
            if (runs == 1)
            {
                Assert.That(saveData.LongestRun, Is.EqualTo(runScore.Distance), "The longest run is not this one");
                Assert.That(saveData.CoinsCollected, Is.EqualTo(runScore.Coins), "The coins are not counted");
            }
        }

        private static AchievementData FindAchievement(string id)
        {
            foreach (AchievementData achievement in Object.FindAnyObjectByType<AchievementTracker>().Achievements)
            {
                if (achievement.Id == id)
                {
                    return achievement;
                }
            }

            Assert.Fail("No achievement " + id);
            return null;
        }

        // The file on disk, not the save in memory, whose longest run grows with every meter of the run.
        private void AssertSavedFileHas(string achievementId, string message)
        {
            string path = Path.Combine(m_saveDirectory, k_SaveFileName);
            Assert.That(File.Exists(path), Is.True, message);
            SaveData saved = SaveData.FromJson(File.ReadAllText(path));
            Assert.That(saved.IsAchievementUnlocked(achievementId), Is.True, message);
        }

        private void FloatingOrigin_WorldShifted(Vector3 offset)
        {
            m_shiftedDistance -= offset.z;
            m_shiftCount++;
        }

        // The groups of the scene are the empty objects at the origin, apart from the tiles.
        private void FindTileManagersAndGroups()
        {
            m_tileManagers.Clear();
            m_groups.Clear();
            List<GameObject> tiles = new List<GameObject>();
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                foreach (TileManager tileManager in root.GetComponentsInChildren<TileManager>())
                {
                    m_tileManagers.Add(tileManager);
                    tiles.AddRange(tileManager.Tiles);
                }
            }

            foreach (GameObject root in roots)
            {
                if (root.GetComponentCount() == 1 && !tiles.Contains(root))
                {
                    m_groups.Add(root.transform);
                }
            }
        }

        // The z position as if the world had never moved back.
        private float GetPlace(Transform target)
        {
            return target.position.z + m_shiftedDistance;
        }

        // The next check compares with this frame.
        private void WatchFromHere()
        {
            m_lastShipPlace = GetPlace(m_ship);
            m_lastCameraPlace = GetPlace(m_camera);
        }

        // Once per frame. The ship stays near the origin, and everything else moves back
        // along with it, so nothing on screen jumps.
        private void AssertNothingJumped()
        {
            Assert.That(m_ship.position.z, Is.LessThan(k_FloatingOriginDistance + k_MaxStepPerFrame),
                "The world did not move back");

            float shipPlace = GetPlace(m_ship);
            float cameraPlace = GetPlace(m_camera);
            Assert.That(shipPlace - m_lastShipPlace, Is.InRange(0f, k_MaxStepPerFrame), "The ship jumped");
            Assert.That(Mathf.Abs(cameraPlace - m_lastCameraPlace), Is.LessThan(k_MaxStepPerFrame),
                "The camera jumped");
            m_lastShipPlace = shipPlace;
            m_lastCameraPlace = cameraPlace;

            foreach (TileManager tileManager in m_tileManagers)
            {
                AssertTilesLineUp(tileManager);
            }

            AssertWorldKeepsItsPlace();
        }

        // A tile manager spawns its tiles end to end, the next one where the last one ends.
        // A spawn point the floating origin left behind would open a gap, and the track would
        // not be extended until the ship has flown that far.
        private static void AssertTilesLineUp(TileManager tileManager)
        {
            IReadOnlyList<GameObject> tiles = tileManager.Tiles;
            float tileLength = tiles[1].transform.position.z - tiles[0].transform.position.z;
            for (int i = 2; i < tiles.Count; i++)
            {
                float gap = tiles[i].transform.position.z - tiles[i - 1].transform.position.z;
                Assert.That(gap, Is.EqualTo(tileLength).Within(k_PlaceTolerance),
                    $"The tiles of {tileManager.name} do not line up");
            }

            float nextGap = tileManager.NextSpawnZ - tiles[tiles.Count - 1].transform.position.z;
            Assert.That(nextGap, Is.EqualTo(tileLength).Within(k_PlaceTolerance),
                $"The next tile of {tileManager.name} would not start where the last one ends");
        }

        // Everything but the ship and the camera stands still, so its place stays the same
        // however often the world moves back. The groups stay at the origin while their
        // children move: if a group moved, the ship and the camera inside would build up
        // local positions as large as the distance flown, and lose the precision again.
        private void AssertWorldKeepsItsPlace()
        {
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.TryGetComponent(out Canvas _))
                {
                    continue;
                }

                if (!m_groups.Contains(root.transform))
                {
                    AssertKeepsItsPlace(root.transform);
                    continue;
                }

                Assert.That(root.transform.position, Is.EqualTo(Vector3.zero), $"{root.name} left the origin");
                foreach (Transform child in root.transform)
                {
                    if (child != m_ship && child != m_camera)
                    {
                        AssertKeepsItsPlace(child);
                    }
                }
            }
        }

        private void AssertKeepsItsPlace(Transform target)
        {
            float place = GetPlace(target);
            if (m_places.TryGetValue(target, out float firstPlace))
            {
                Assert.That(place, Is.EqualTo(firstPlace).Within(k_PlaceTolerance), $"{GetPath(target)} moved");
            }
            else
            {
                m_places.Add(target, place);
            }
        }

        private static string GetPath(Transform transform)
        {
            string path = transform.name;
            for (Transform parent = transform.parent; parent != null; parent = parent.parent)
            {
                path = parent.name + "/" + path;
            }

            return path;
        }

        private static PlayerLoopSystem AddToPreUpdate(PlayerLoopSystem playerLoop,
            PlayerLoopSystem.UpdateFunction update)
        {
            for (int i = 0; i < playerLoop.subSystemList.Length; i++)
            {
                if (playerLoop.subSystemList[i].type != typeof(PreUpdate))
                {
                    continue;
                }

                List<PlayerLoopSystem> systems = new List<PlayerLoopSystem>(playerLoop.subSystemList[i].subSystemList);
                systems.Add(new PlayerLoopSystem { type = typeof(RunTests), updateDelegate = update });
                playerLoop.subSystemList[i].subSystemList = systems.ToArray();
            }

            return playerLoop;
        }
    }
}
