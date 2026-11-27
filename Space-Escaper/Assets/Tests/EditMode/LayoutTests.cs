using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceEscaper.Tests
{
    /// <summary>
    /// Lays the screens out the way displays of other shapes show them, from tablets in 4:3 to the narrow
    /// outer screen of a foldable phone, some with a camera cutout, in every language of the game.
    /// </summary>
    [TestFixtureSource(nameof(LanguageIds))]
    public class LayoutTests
    {
        private const string k_ScenePath = "Assets/Scenes/Game.unity";
        private const string k_LanguageCatalogPath = "Assets/Data/Languages/LanguageCatalog.asset";
        private const string k_UiRootName = "UI";
        private const string k_SettingsName = "Settings";
        private const string k_AchievementCatalogPath = "Assets/Data/Achievements/AchievementCatalog.asset";
        private const string k_GroupSeparatorKey = "number.groupSeparator";
        private const string k_DecimalSeparatorKey = "number.decimalSeparator";
        private const string k_ProgressKey = "achievements.progress";
        private const string k_RewardsBadgeLabelPath = "RewardsBadge/Label";

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

        // Moritz, 27.11.2026: a score, a highscore and the coins up to six digits, a distance up to five, as runs of
        // half an hour and years of coins bring them, with a multiplier up to x99.9 and up to 59:59 minutes. The coins
        // of a run grow with its distance, about 6,000 in 99,999 m. Texts that share a key are told apart by name.
        private static readonly LongestNumbers[] s_LongestNumbers =
        {
            new LongestNumbers("number.whole", "ScoreText", 999999f),
            new LongestNumbers("number.whole", "HighscoreText", 999999f),
            new LongestNumbers("number.whole", "CoinText", 999999f),
            new LongestNumbers("number.gain", "CoinText", 9999f),
            new LongestNumbers("run.pointsToGo", null, 999999f),
            new LongestNumbers("common.meters", null, 99999f),
            new LongestNumbers("number.multiplier", null, 99.9f),
            new LongestNumbers("number.time", null, 59f, 59f),
            new LongestNumbers("pause.time", null, 59f, 59f),
            new LongestNumbers("death.runEnded", null, 59f, 59f),
            new LongestNumbers("number.percent", null, 100f),
        };

        private readonly string m_languageId;
        private Scene m_scene;
        private bool m_isOpenedByTest;
        private Scene m_layoutScene;
        private GameObject m_ui;
        private int m_achievementCount;

        // Every language of the catalog lays the screens out, so a new one comes with the others.
        private static IEnumerable<string> LanguageIds => AssetDatabase
            .LoadAssetAtPath<LanguageCatalog>(k_LanguageCatalogPath).Languages.Select(language => language.Id);

        public LayoutTests(string languageId)
        {
            m_languageId = languageId;
        }

        // The test lays out a copy of the UI in a preview scene, so the game scene stays as it is, also
        // when it is open in the editor. On a canvas in world space the size is free, so the copy can
        // take the size the CanvasScaler gives it on any display.
        [OneTimeSetUp]
        public void CopyUi()
        {
            // Until the next compile the editor keeps the texts of a language it has shown, also after a change to its
            // file, so the test reads them again.
            LanguageCatalog languages = AssetDatabase.LoadAssetAtPath<LanguageCatalog>(k_LanguageCatalogPath);
            LanguageManager.Reload(languages, languages.Find(m_languageId, SystemLanguage.Unknown));
            m_achievementCount = AssetDatabase.LoadAssetAtPath<AchievementCatalog>(k_AchievementCatalogPath)
                .Achievements.Count;

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

            ShowLanguage(new LanguageTexts(languages.Default.Texts.text));
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
        // covering the whole screen reach under it on purpose. What a mask cuts off is neither shown nor
        // tapped, like the end of the list in the settings until it is scrolled up.
        [TestCaseSource(nameof(s_Devices))]
        public void EverythingShownLiesInTheSafeArea(Device device)
        {
            Rect safeArea = LayOut(device);
            List<string> problems = new List<string>();
            AddShownOutside(problems, safeArea);
            Assert.That(problems, Is.Empty);
        }

        [TestCaseSource(nameof(s_Devices))]
        public void EveryTextFitsItsBox(Device device)
        {
            LayOut(device);
            List<string> problems = new List<string>();
            AddTextsTooLarge(problems);
            Assert.That(problems, Is.Empty);
        }

        // Without texts of its own the copy would show the scene, English, in every language and measure nothing new.
        [Test]
        public void EveryTextShowsTheLanguage()
        {
            Assert.That(LanguageManager.Current.Id, Is.EqualTo(m_languageId), "The test shows another language");
            List<string> problems = new List<string>();
            foreach (LocalizedText text in m_ui.GetComponentsInChildren<LocalizedText>(true))
            {
                string expected = LanguageManager.Get(text.Key);
                Regex pattern = TextWithNumbers.GetPattern(expected, GetSeparator(k_GroupSeparatorKey),
                    GetSeparator(k_DecimalSeparatorKey));
                if (!pattern.IsMatch(text.Text.text))
                {
                    problems.Add(GetPath(text.transform) + " shows \"" + text.Text.text + "\" instead of \"" +
                        expected + "\"");
                }
            }

            Assert.That(problems, Is.Empty);
        }

        // The examples in the scene are short, a long run or years of coins bring longer numbers. Each text shows its
        // longest ones here, laid out on every display like the examples.
        [TestCaseSource(nameof(s_Devices))]
        public void EveryNumberFitsAtItsLongest(Device device)
        {
            LocalizedText[] texts = m_ui.GetComponentsInChildren<LocalizedText>(true);
            string[] savedTexts = System.Array.ConvertAll(texts, text => text.Text.text);
            try
            {
                foreach (LocalizedText text in texts)
                {
                    ShowLongestNumbers(text);
                }

                Rect safeArea = LayOut(device);
                List<string> problems = new List<string>();
                AddTextsTooLarge(problems);
                AddShownOutside(problems, safeArea);
                Assert.That(problems, Is.Empty);
            }
            finally
            {
                for (int i = 0; i < texts.Length; i++)
                {
                    texts[i].Text.text = savedTexts[i];
                }
            }
        }

        // The card on the death screen shows one achievement at a time and counts the others on its label, at
        // most all of them. The scene keeps only the first. The card is as wide on every display, so one is enough.
        [Test]
        public void EveryAchievementFitsTheCardOfTheDeathScreen()
        {
            LayOut(s_Devices[0]);
            DeathMenuScreen deathMenu = m_ui.GetComponentInChildren<DeathMenuScreen>(true);
            RectTransform card = (RectTransform)deathMenu.UnlockedCard.transform;
            TMP_Text[] texts = card.GetComponentsInChildren<TMP_Text>(true);
            string[] savedTexts = System.Array.ConvertAll(texts, text => text.text);
            LocalizedText label = deathMenu.UnlockedLabelText.GetComponent<LocalizedText>();
            string labelKey = label.Key;
            AchievementCatalog catalog = AssetDatabase.LoadAssetAtPath<AchievementCatalog>(k_AchievementCatalogPath);
            List<string> problems = new List<string>();
            try
            {
                foreach (AchievementData achievement in catalog.Achievements)
                {
                    deathMenu.ShowUnlocked(achievement, catalog.Achievements.Count);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(card);
                    foreach (TMP_Text text in texts)
                    {
                        text.ForceMeshUpdate(true, true);
                        AddIfTooLarge(problems, text);
                    }
                }
            }
            finally
            {
                // For more than one achievement the label shows a key of its own.
                label.Show(labelKey);
                for (int i = 0; i < texts.Length; i++)
                {
                    texts[i].text = savedTexts[i];
                }
            }

            Assert.That(problems, Is.Empty);
        }

        // The popup in the run shows one achievement at a time, the scene keeps the first. The reward grows to the left
        // with its number, toward the label. The popup is as wide on every display, so one of them is enough.
        [Test]
        public void EveryAchievementFitsThePopupOfTheRun()
        {
            LayOut(s_Devices[0]);
            GameMenuScreen gameMenu = m_ui.GetComponentInChildren<GameMenuScreen>(true);
            RectTransform popup = (RectTransform)gameMenu.AchievementPopup.transform;
            TMP_Text label = popup.Find("LabelText").GetComponent<TMP_Text>();
            RectTransform reward = (RectTransform)popup.Find("Reward");
            TMP_Text[] texts = popup.GetComponentsInChildren<TMP_Text>(true);
            string[] savedTexts = System.Array.ConvertAll(texts, text => text.text);
            AchievementCatalog catalog = AssetDatabase.LoadAssetAtPath<AchievementCatalog>(k_AchievementCatalogPath);
            List<string> problems = new List<string>();
            try
            {
                foreach (AchievementData achievement in catalog.Achievements)
                {
                    gameMenu.ShowAchievement(achievement);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(reward);
                    foreach (TMP_Text text in texts)
                    {
                        text.ForceMeshUpdate(true, true);
                        AddIfTooLarge(problems, text);
                    }

                    float gap = GetRect(reward).xMin - GetShownRect(label).xMax;
                    if (gap < -k_Tolerance)
                    {
                        problems.Add(string.Format(CultureInfo.InvariantCulture,
                            "The reward of {0} reaches {1:0.#} into the label", achievement.Id, -gap));
                    }
                }
            }
            finally
            {
                for (int i = 0; i < texts.Length; i++)
                {
                    texts[i].text = savedTexts[i];
                }
            }

            Assert.That(problems, Is.Empty);
        }

        // The card hangs under the tiles and the buttons stand on the bottom, so a short display brings them
        // together.
        [TestCaseSource(nameof(s_Devices))]
        public void TheCardOfTheDeathScreenStaysAboveItsButtons(Device device)
        {
            LayOut(device);
            DeathMenuScreen deathMenu = m_ui.GetComponentInChildren<DeathMenuScreen>(true);
            float cardBottom = GetRect((RectTransform)deathMenu.UnlockedCard.transform).yMin;
            float buttonsTop = GetRect((RectTransform)deathMenu.ReviveButton.transform).yMax;
            string message = string.Format(CultureInfo.InvariantCulture,
                "The card reaches {0:0.#} into the revive button", buttonsTop - cardBottom);
            Assert.That(cardBottom, Is.GreaterThanOrEqualTo(buttonsTop - k_Tolerance), message);
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

        // Nothing in the settings should hide below the screen until someone scrolls. Their list lies in a
        // ScrollRect all the same, for the Invincible switch that test builds add to it.
        [TestCaseSource(nameof(s_Devices))]
        public void TheSettingsFitWithoutScrolling(Device device)
        {
            LayOut(device);
            ScrollRect list = m_ui.transform.Find(k_SettingsName).GetComponentInChildren<ScrollRect>(true);
            float height = list.content.rect.height;
            float room = list.viewport.rect.height;
            Assert.That(height, Is.LessThanOrEqualTo(room + k_Tolerance), string.Format(CultureInfo.InvariantCulture,
                "The settings are {0:0.#} high, {1:0.#} fit below the title", height, room));
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

        private void AddShownOutside(List<string> problems, Rect safeArea)
        {
            Rect screen = GetRect((RectTransform)m_ui.transform);
            foreach (Graphic graphic in m_ui.GetComponentsInChildren<Graphic>(true))
            {
                string path = GetPath(graphic.transform);
                Rect box = GetRect(graphic.rectTransform);
                if (!graphic.enabled || IsSameRect(box, screen))
                {
                    continue;
                }

                bool isTapArea = graphic.raycastTarget && graphic.GetComponentInParent<Selectable>(true) != null;
                if (isTapArea && TryClip(graphic, box, out Rect tapArea))
                {
                    ReportOutside(problems, path + " (tap area)", tapArea, safeArea);
                }

                if (IsShown(graphic) && TryClip(graphic, GetShownRect(graphic), out Rect shown))
                {
                    ReportOutside(problems, path, shown, safeArea);
                }
            }
        }

        private void AddTextsTooLarge(List<string> problems)
        {
            foreach (TMP_Text text in m_ui.GetComponentsInChildren<TMP_Text>(true))
            {
                AddIfTooLarge(problems, text);
            }
        }

        // Unity wakes no script in edit mode, so the copy shows the scene, in the default language. Each text shows its
        // key in the language of the test, with the numbers of its example.
        private void ShowLanguage(LanguageTexts defaultTexts)
        {
            char groupSeparator = defaultTexts.Get(k_GroupSeparatorKey)[0];
            char decimalSeparator = defaultTexts.Get(k_DecimalSeparatorKey)[0];
            foreach (LocalizedText text in m_ui.GetComponentsInChildren<LocalizedText>(true))
            {
                TextWithNumbers.TryRead(text.Text.text, defaultTexts.Get(text.Key), groupSeparator, decimalSeparator,
                    out float first, out float second);
                text.Show(text.Key, first, second);
            }
        }

        // A progress is one less than its target at most, and the medal counts all achievements. Rewards and prices
        // show their data, as the scene does.
        private void ShowLongestNumbers(LocalizedText text)
        {
            TextWithNumbers.TryRead(text.Text.text, LanguageManager.Get(text.Key), GetSeparator(k_GroupSeparatorKey),
                GetSeparator(k_DecimalSeparatorKey), out float first, out float second);
            string path = GetPath(text.transform);
            if (text.Key.StartsWith(k_ProgressKey))
            {
                first = second - 1f;
            }
            else if (path.EndsWith("/" + k_RewardsBadgeLabelPath))
            {
                first = m_achievementCount;
            }

            foreach (LongestNumbers longest in s_LongestNumbers)
            {
                if (longest.Key == text.Key && (longest.Name == null || path.EndsWith("/" + longest.Name)))
                {
                    first = longest.First;
                    second = longest.Second;
                }
            }

            text.Show(text.Key, first, second);
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

        // A text that needs more lines than its box has runs into what is next to it. Without wrapping it runs out at
        // the side, which TextMesh Pro does not report, so this measures the glyphs.
        private static void AddIfTooLarge(List<string> problems, TMP_Text text)
        {
            if (text.textInfo.characterCount == 0)
            {
                return;
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
                    "{0} \"{1}\" is {2:0.#} wide in a box of {3:0.#}", GetPath(text.transform), text.text, glyphs.width,
                    box.width));
            }
        }

        private static char GetSeparator(string key)
        {
            return LanguageManager.Get(key)[0];
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

        // A RectMask2D draws its children only inside its own rectangle and takes taps on them only there.
        // Returns false if nothing is left.
        private static bool TryClip(Graphic graphic, Rect rect, out Rect clipped)
        {
            clipped = rect;
            for (Transform parent = graphic.transform.parent; parent != null; parent = parent.parent)
            {
                RectMask2D mask = parent.GetComponent<RectMask2D>();
                if (mask == null || !mask.enabled)
                {
                    continue;
                }

                Rect window = GetRect(mask.rectTransform);
                float xMin = Mathf.Max(clipped.xMin, window.xMin);
                float yMin = Mathf.Max(clipped.yMin, window.yMin);
                float xMax = Mathf.Min(clipped.xMax, window.xMax);
                float yMax = Mathf.Min(clipped.yMax, window.yMax);
                if (xMax <= xMin || yMax <= yMin)
                {
                    return false;
                }

                clipped = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
            }

            return true;
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

        /// <summary>
        /// The longest numbers a text with a key shows. A name picks one of the texts that share a key.
        /// </summary>
        private class LongestNumbers
        {
            public LongestNumbers(string key, string name, float first, float second = 0f)
            {
                Key = key;
                Name = name;
                First = first;
                Second = second;
            }

            public string Key { get; }

            public string Name { get; }

            public float First { get; }

            public float Second { get; }
        }
    }
}
