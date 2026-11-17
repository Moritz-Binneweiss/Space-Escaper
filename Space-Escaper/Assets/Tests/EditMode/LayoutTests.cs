using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceEscaper.Tests
{
    /// <summary>
    /// Lays the screens out the way displays of other shapes show them, from tablets in 4:3 to the narrow
    /// outer screen of a foldable phone, some with a camera cutout.
    /// </summary>
    public class LayoutTests
    {
        private const string k_ScenePath = "Assets/Scenes/Game.unity";
        private const string k_UiRootName = "UI";

        // Android asks for touch targets of at least 48 dp. The narrowest common phones are 360 dp wide.
        private const float k_MinTapSizeDp = 48f;
        private const float k_NarrowPhoneWidthDp = 360f;

        // Fitting rectangles leaves small differences in the float math, far below a pixel.
        private const float k_Tolerance = 0.5f;

        // Screens in portrait, in pixels. Safe areas count from the bottom left like in Unity, the ones with
        // a cutout are those of the phones and tablets in the Device Simulator.
        private static readonly Device[] s_Devices =
        {
            new Device("16:9", 1080, 1920),
            new Device("20:9", 1080, 2400),
            new Device("21:9", 1080, 2520),
            new Device("Punch hole right", 1080, 2280, new Rect(0, 0, 1080, 2164)),
            new Device("Punch hole left", 1080, 2340, new Rect(0, 0, 1080, 2204)),
            new Device("Wide pill cutout", 1200, 2640, new Rect(0, 0, 1200, 2499)),
            new Device("Foldable closed", 960, 2658, new Rect(0, 0, 960, 2566)),
            new Device("Foldable open", 1856, 2160),
            new Device("Tablet 4:3", 1536, 2048),
            new Device("Tablet with a bar at the bottom", 2048, 2732, new Rect(0, 40, 2048, 2692)),
        };

        private Scene m_scene;
        private bool m_isOpenedByTest;
        private Scene m_layoutScene;
        private GameObject m_ui;

        // The test lays out a copy of the UI in a preview scene, so the game scene stays as it is, also
        // when it is open in the editor. On a canvas in world space the size is free, so the copy can
        // take the size the CanvasScaler gives it on any display.
        [OneTimeSetUp]
        public void CopyUi()
        {
            m_scene = SceneManager.GetSceneByPath(k_ScenePath);
            m_isOpenedByTest = !m_scene.isLoaded;
            if (m_isOpenedByTest)
            {
                m_scene = EditorSceneManager.OpenScene(k_ScenePath, OpenSceneMode.Additive);
            }

            m_layoutScene = EditorSceneManager.NewPreviewScene();
            m_ui = Object.Instantiate(GetUiRoot(), new InstantiateParameters { scene = m_layoutScene });
            m_ui.name = k_UiRootName;

            // TextMesh Pro only lays out a text that has woken up, which never happens while it is switched
            // off. Each screen and each family of skins is shown at some point, so the copy has all of them on.
            foreach (Transform child in m_ui.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.SetActive(true);
            }

            m_ui.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            RectTransform root = (RectTransform)m_ui.transform;
            root.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            root.localScale = Vector3.one;
            root.pivot = new Vector2(0.5f, 0.5f);
        }

        [OneTimeTearDown]
        public void RemoveCopy()
        {
            EditorSceneManager.ClosePreviewScene(m_layoutScene);
            if (m_isOpenedByTest)
            {
                EditorSceneManager.CloseScene(m_scene, true);
            }
        }

        // The game draws under a camera cutout, but nothing there should be needed to play. Backgrounds
        // covering the whole screen reach under it on purpose.
        [TestCaseSource(nameof(s_Devices))]
        public void EverythingShownLiesInTheSafeArea(Device device)
        {
            Rect safeArea = LayOut(device);
            Rect screen = GetRect((RectTransform)m_ui.transform);
            List<string> problems = new List<string>();
            foreach (Graphic graphic in m_ui.GetComponentsInChildren<Graphic>(true))
            {
                string path = GetPath(graphic.transform);
                Rect box = GetRect(graphic.rectTransform);
                if (!graphic.enabled || IsSameRect(box, screen))
                {
                    continue;
                }

                if (graphic.raycastTarget && graphic.GetComponentInParent<Selectable>(true) != null)
                {
                    ReportOutside(problems, path + " (tap area)", box, safeArea);
                }

                if (IsShown(graphic))
                {
                    ReportOutside(problems, path, GetShownRect(graphic), safeArea);
                }
            }

            Assert.That(problems, Is.Empty);
        }

        // A text that needs more lines than its box has runs into what is next to it. Without wrapping it
        // runs out at the side, which TextMesh Pro does not report, so the test measures the glyphs.
        [TestCaseSource(nameof(s_Devices))]
        public void EveryTextFitsItsBox(Device device)
        {
            LayOut(device);
            List<string> problems = new List<string>();
            foreach (TMP_Text text in m_ui.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.textInfo.characterCount == 0)
                {
                    continue;
                }

                if (text.isTextOverflowing)
                {
                    problems.Add(GetPath(text.transform) + " needs more lines than its box has");
                }

                Rect box = GetRect(text.rectTransform);
                Rect glyphs = GetShownRect(text);
                if (glyphs.xMin < box.xMin - k_Tolerance || glyphs.xMax > box.xMax + k_Tolerance)
                {
                    problems.Add(string.Format(CultureInfo.InvariantCulture,
                        "{0} is {1:0.#} wide in a box of {2:0.#}", GetPath(text.transform), glyphs.width, box.width));
                }
            }

            Assert.That(problems, Is.Empty);
        }

        // The UI is 450 units wide on every phone, so a unit is smallest on the narrowest ones. A control
        // takes taps on any graphic of its own that is a raycast target, the largest one counts.
        [Test]
        public void EveryControlIsBigEnoughToTap()
        {
            LayOut(s_Devices[0]);
            float unitsPerDp = GetRect((RectTransform)m_ui.transform).width / k_NarrowPhoneWidthDp;
            float minTapSize = k_MinTapSizeDp * unitsPerDp;
            List<string> problems = new List<string>();
            foreach (Selectable selectable in m_ui.GetComponentsInChildren<Selectable>(true))
            {
                Vector2 largest = Vector2.zero;
                foreach (Graphic graphic in selectable.GetComponentsInChildren<Graphic>(true))
                {
                    Vector2 size = GetRect(graphic.rectTransform).size;
                    bool isLarger = Mathf.Min(size.x, size.y) > Mathf.Min(largest.x, largest.y);
                    if (graphic.enabled && graphic.raycastTarget && isLarger)
                    {
                        largest = size;
                    }
                }

                if (Mathf.Min(largest.x, largest.y) < minTapSize - k_Tolerance)
                {
                    problems.Add(string.Format(CultureInfo.InvariantCulture,
                        "{0} takes taps on {1:0.#} x {2:0.#}, needs {3:0.#}",
                        GetPath(selectable.transform), largest.x, largest.y, minTapSize));
                }
            }

            Assert.That(problems, Is.Empty);
        }

        // Sizes the copy as the CanvasScaler would on the device and fits the safe areas of the screens.
        // Returns the safe area in units of the UI, which has its origin in the middle of the screen.
        private Rect LayOut(Device device)
        {
            float scaleFactor = GetScaleFactor(m_ui.GetComponent<CanvasScaler>(), device.ScreenSize);
            Vector2 canvasSize = (Vector2)device.ScreenSize / scaleFactor;
            ((RectTransform)m_ui.transform).sizeDelta = canvasSize;
            foreach (SafeArea safeArea in m_ui.GetComponentsInChildren<SafeArea>(true))
            {
                safeArea.Fit(device.SafeAreaInPixels, device.ScreenSize);
            }

            // The game lays out its layout groups before it draws. The copy does it here, so the texts in a group
            // get the boxes the group gives them before TextMesh Pro fills them.
            foreach (LayoutGroup group in m_ui.GetComponentsInChildren<LayoutGroup>(true))
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)group.transform);
            }

            foreach (TMP_Text text in m_ui.GetComponentsInChildren<TMP_Text>(true))
            {
                text.ForceMeshUpdate(true, true);
            }

            return new Rect(device.SafeAreaInPixels.position / scaleFactor - canvasSize / 2f,
                device.SafeAreaInPixels.size / scaleFactor);
        }

        private GameObject GetUiRoot()
        {
            foreach (GameObject root in m_scene.GetRootGameObjects())
            {
                if (root.name == k_UiRootName)
                {
                    return root;
                }
            }

            Assert.Fail("No " + k_UiRootName + " in " + k_ScenePath);
            return null;
        }

        // The same math as in the CanvasScaler, which reads the size of the real screen.
        private static float GetScaleFactor(CanvasScaler scaler, Vector2Int screenSize)
        {
            Assert.That(scaler.uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
            Vector2 reference = scaler.referenceResolution;
            float widthScale = screenSize.x / reference.x;
            float heightScale = screenSize.y / reference.y;
            switch (scaler.screenMatchMode)
            {
                case CanvasScaler.ScreenMatchMode.Expand:
                    return Mathf.Min(widthScale, heightScale);
                case CanvasScaler.ScreenMatchMode.Shrink:
                    return Mathf.Max(widthScale, heightScale);
                case CanvasScaler.ScreenMatchMode.MatchWidthOrHeight:
                    return Mathf.Pow(2f, Mathf.Lerp(Mathf.Log(widthScale, 2f), Mathf.Log(heightScale, 2f),
                        scaler.matchWidthOrHeight));
                default:
                    Assert.Fail("Unknown screen match mode " + scaler.screenMatchMode);
                    return 1f;
            }
        }

        private static bool IsShown(Graphic graphic)
        {
            TMP_Text text = graphic as TMP_Text;
            if (text != null && text.textInfo.characterCount == 0)
            {
                return false;
            }

            return graphic.color.a > 0f;
        }

        // A text shows its glyphs, which can reach out of its box. Everything else shows its box.
        private static Rect GetShownRect(Graphic graphic)
        {
            TMP_Text text = graphic as TMP_Text;
            if (text == null)
            {
                return GetRect(graphic.rectTransform);
            }

            Bounds bounds = text.textBounds;
            Vector3 min = text.rectTransform.TransformPoint(bounds.min);
            Vector3 max = text.rectTransform.TransformPoint(bounds.max);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        // The copy lies at the origin of its scene, unscaled and unrotated, so world space is the space of
        // the UI.
        private static Rect GetRect(RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }

        private static bool IsSameRect(Rect a, Rect b)
        {
            return Mathf.Abs(a.xMin - b.xMin) < k_Tolerance && Mathf.Abs(a.xMax - b.xMax) < k_Tolerance
                && Mathf.Abs(a.yMin - b.yMin) < k_Tolerance && Mathf.Abs(a.yMax - b.yMax) < k_Tolerance;
        }

        private static void ReportOutside(List<string> problems, string name, Rect rect, Rect safeArea)
        {
            List<string> sides = new List<string>();
            AddSide(sides, "left", safeArea.xMin - rect.xMin);
            AddSide(sides, "right", rect.xMax - safeArea.xMax);
            AddSide(sides, "bottom", safeArea.yMin - rect.yMin);
            AddSide(sides, "top", rect.yMax - safeArea.yMax);
            if (sides.Count > 0)
            {
                problems.Add(name + " lies outside by " + string.Join(", ", sides));
            }
        }

        private static void AddSide(List<string> sides, string side, float overlap)
        {
            if (overlap > k_Tolerance)
            {
                sides.Add(string.Format(CultureInfo.InvariantCulture, "{0:0.#} on the {1}", overlap, side));
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

        /// <summary>
        /// A display in portrait, with the safe area that is left between its cutouts.
        /// </summary>
        public class Device
        {
            public Device(string name, int width, int height)
                : this(name, width, height, new Rect(0, 0, width, height))
            {
            }

            public Device(string name, int width, int height, Rect safeAreaInPixels)
            {
                Name = name;
                ScreenSize = new Vector2Int(width, height);
                SafeAreaInPixels = safeAreaInPixels;
            }

            public string Name { get; }

            public Vector2Int ScreenSize { get; }

            public Rect SafeAreaInPixels { get; }

            // The test runner shows this in the name of each case.
            public override string ToString()
            {
                return Name + " " + ScreenSize.x + "x" + ScreenSize.y;
            }
        }
    }
}
