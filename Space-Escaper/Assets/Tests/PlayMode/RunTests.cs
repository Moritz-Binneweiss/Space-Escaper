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

        // The fresh track after a revive starts at least 35 units ahead of the ship, so
        // a crash within this distance can only come from the old one.
        private const float k_DistanceAfterRevive = 20f;

        private string m_saveDirectory;
        private PlayerLoopSystem m_originalPlayerLoop;
        private GameObject m_pendingTap;
        private PointerEventData m_pendingTapData;
        private Button m_continueButton;

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

            // Played silently with the volume slider all the way down. Music and sounds
            // still go through the same code as in the game.
            SaveSystem.Data.MasterVolume = 0f;

#if UNITY_EDITOR || UNITY_ENABLE_CHECKS
            // The ship has to crash, whatever the test menu says.
            PlayerMotor.IsInvincible = false;
#endif

            m_originalPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoop.SetPlayerLoop(AddToPreUpdate(PlayerLoop.GetCurrentPlayerLoop(), TapPendingButton));
        }

        // The save stays in the test folder: leaving Play Mode saves once more, and that
        // must not reach the real save. The next Play Mode loads the real one again.
        [TearDown]
        public void TearDown()
        {
            PlayerLoop.SetPlayerLoop(m_originalPlayerLoop);
        }

        // Start, crash and revive through the buttons of the game. The revive must not
        // crash the ship again: the obstacle that killed it is only destroyed at the end
        // of the frame, and the ship flies on in that same frame.
        [UnityTest]
        public IEnumerator ShipCrashesAndFliesOnAfterRevive()
        {
            Random.InitState(k_RandomSeed);
            yield return SceneManager.LoadSceneAsync(k_GameSceneName);
            yield return null;

            GameManager gameManager = GameManager.Instance;
            Transform ship = Object.FindAnyObjectByType<PlayerMotor>().transform;
            Button playButton = FindButtonCalling<GameManager>(nameof(GameManager.Play));
            Button pauseButton = FindButtonCalling<PauseMenu>(nameof(PauseMenu.Pause));
            Button reviveButton = FindButtonCalling<GameManager>(nameof(GameManager.RequestRevive));
            m_continueButton = FindButtonCalling<PauseMenu>(nameof(PauseMenu.Continue));

            yield return Tap(playButton);
            Assert.That(gameManager.IsRunActive, Is.True, "Play did not start a run");
            Assert.That(pauseButton.gameObject.activeInHierarchy, Is.True, "No game menu during the run");

            // Straight down the middle lane until the first obstacle.
            float startTime = Time.time;
            while (gameManager.IsRunActive)
            {
                Assert.That(Time.time - startTime, Is.LessThan(k_MaxTimeToCrash), "No crash flying straight");
                yield return NextFrame();
            }

            float crashZ = ship.position.z;
            TestContext.WriteLine($"Crashed at z = {crashZ:0.0} after {Time.time - startTime:0.0} s");
            Assert.That(reviveButton.gameObject.activeInHierarchy, Is.True, "No death menu with the revive button");
            AssertDeathIsSaved();

            yield return new WaitForSeconds(k_TimeOnDeathScreen);
            yield return Tap(reviveButton);

            float reviveTime = Time.time;
            while (ship.position.z < crashZ + k_DistanceAfterRevive)
            {
                float distance = ship.position.z - crashZ;
                Assert.That(gameManager.IsRunActive, Is.True, $"Crashed again {distance:0.0} units after the revive");
                Assert.That(Time.time - reviveTime, Is.LessThan(k_MaxTimeToFlyOn), "Did not fly on after the revive");
                yield return NextFrame();
            }

            Assert.That(pauseButton.gameObject.activeInHierarchy, Is.True, "No game menu after the revive");
        }

        private static Button FindButtonCalling<T>(string methodName) where T : Object
        {
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (Button button in root.GetComponentsInChildren<Button>(true))
                {
                    for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                    {
                        if (button.onClick.GetPersistentTarget(i) is T
                            && button.onClick.GetPersistentMethodName(i) == methodName)
                        {
                            return button;
                        }
                    }
                }
            }

            Assert.Fail($"No button calls {typeof(T).Name}.{methodName}");
            return null;
        }

        // Taps the middle of the button at the start of the next frame, before any
        // Update(), as a real tap does: the EventSystem runs before the game scripts
        // (execution order -1000). A tap from the test itself would come after
        // Update(), too late to see a crash in the frame of the tap.
        private IEnumerator Tap(Button button)
        {
            // The UI is a screen space overlay, so its points need no camera.
            RectTransform rectTransform = (RectTransform)button.transform;
            Vector3 center = rectTransform.TransformPoint(rectTransform.rect.center);
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = RectTransformUtility.WorldToScreenPoint(null, center),
            };

            List<RaycastResult> hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, hits);
            GameObject tapped = hits.Count > 0
                ? ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject)
                : null;
            Assert.That(tapped, Is.EqualTo(button.gameObject), $"A tap on {button.name} does not reach it");

            m_pendingTap = button.gameObject;
            m_pendingTapData = eventData;
            yield return null;
        }

        // Losing focus in the editor pauses the run, as leaving the app does on a phone.
        // The test then taps Continue like a player would.
        private IEnumerator NextFrame()
        {
            if (PauseMenu.IsPaused)
            {
                TestContext.WriteLine("Paused by a focus change, tapping Continue");
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
            string path = Path.Combine(m_saveDirectory, k_SaveFileName);
            Assert.That(File.Exists(path), Is.True, "The death was not saved");
            Assert.That(SaveSystem.Data.Highscore, Is.GreaterThan(0), "The death did not record the score");
            Assert.That(File.ReadAllText(path), Is.EqualTo(SaveSystem.Data.ToJson()), "The save file is out of date");
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
