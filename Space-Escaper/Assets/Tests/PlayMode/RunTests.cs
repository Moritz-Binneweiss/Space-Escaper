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

        private readonly Dictionary<Transform, float> m_places = new Dictionary<Transform, float>();
        private readonly List<TileManager> m_tileManagers = new List<TileManager>();
        private readonly List<Transform> m_groups = new List<Transform>();
        private string m_saveDirectory;
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

            // Played silently with the volume slider all the way down. Music and sounds
            // still go through the same code as in the game.
            SaveSystem.Data.MasterVolume = 0f;

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
        }

        // Start, crash and revive through the buttons of the game. The score stands still on
        // the death screen, and the revive must not crash the ship again: the obstacle that
        // killed it is only destroyed at the end of the frame, and the ship flies on in that
        // same frame. All along, the world moves back to the origin every few units, and
        // nothing on screen may jump.
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
            Button reviveButton = FindScreen<DeathMenuScreen>().ReviveButton;
            m_continueButton = FindScreen<PauseMenuScreen>().ContinueButton;

            yield return Tap(startButton);
            Assert.That(gameManager.State, Is.EqualTo(GameState.Run), "Play did not start a run");
            Assert.That(pauseButton.gameObject.activeInHierarchy, Is.True, "No game menu during the run");

            // Straight down the middle lane until the first obstacle.
            float startTime = Time.time;
            WatchFromHere();
            while (gameManager.State != GameState.Dead)
            {
                Assert.That(Time.time - startTime, Is.LessThan(k_MaxTimeToCrash), "No crash flying straight");
                AssertNothingJumped();
                yield return NextFrame();
            }

            float crashPlace = GetPlace(m_ship);
            TestContext.WriteLine($"Crashed at z = {crashPlace:0.0} after {Time.time - startTime:0.0} s, "
                + $"the world moved back {m_shiftCount} times");
            Assert.That(reviveButton.gameObject.activeInHierarchy, Is.True, "No death menu with the revive button");
            AssertDeathIsSaved();

            int deathScore = runScore.Score;
            yield return new WaitForSeconds(k_TimeOnDeathScreen);
            Assert.That(runScore.Score, Is.EqualTo(deathScore), "The score counted on after the crash");
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

        // Every screen but the main menu is switched off until the game gets there.
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
            if (GameManager.Instance.State == GameState.Paused)
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
