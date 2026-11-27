using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceEscaper.Tests
{
    public class SceneTests
    {
        private const string k_ScenePath = "Assets/Scenes/Game.unity";
        private const string k_UiRootName = "UI";
        private const string k_AchievementCatalogPath = "Assets/Data/Achievements/AchievementCatalog.asset";
        private const string k_ColorwayCatalogPath = "Assets/Data/Colorways/ColorwayCatalog.asset";
        private const string k_LanguageCatalogPath = "Assets/Data/Languages/LanguageCatalog.asset";

        // Colors in the scene are floats, and the roles of the elements were worked out from their 8-bit values.
        private const float k_ColorTolerance = 2f / 255f;

        // Serialized names in Unity's own components, which have no other way in.
        private const string k_ScriptProperty = "m_Script";
        private const string k_StoredCallsProperty = "m_PersistentCalls.m_Calls";

        // A number as TextMesh Pro writes it into a placeholder like {0} or {1:00}.
        private const string k_NumberPattern = @"[-+]?\d+(?:\.\d+)?";

        private static readonly Regex s_placeholder = new Regex(@"\{[^}]*\}");

        private Scene m_scene;
        private bool m_isOpenedByTest;

        // The test runner swaps the open scenes for an empty one while it runs and
        // brings them back afterwards. So the game scene is opened next to it and
        // closed again, unless it is loaded already.
        [OneTimeSetUp]
        public void OpenScene()
        {
            m_scene = SceneManager.GetSceneByPath(k_ScenePath);
            m_isOpenedByTest = !m_scene.isLoaded;
            if (m_isOpenedByTest)
            {
                m_scene = EditorSceneManager.OpenScene(k_ScenePath, OpenSceneMode.Additive);
            }
        }

        [OneTimeTearDown]
        public void CloseScene()
        {
            if (m_isOpenedByTest)
            {
                EditorSceneManager.CloseScene(m_scene, true);
            }
        }

        [Test]
        public void NoScriptIsMissing()
        {
            List<string> objectsWithMissingScripts = new List<string>();
            foreach (Transform transform in GetComponentsInScene<Transform>())
            {
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) > 0)
                {
                    objectsWithMissingScripts.Add(GetPath(transform));
                }
            }

            Assert.That(objectsWithMissingScripts, Is.Empty);
        }

        // Buttons and the other controls are connected in code. A call stored in the scene would
        // run on top of that, and after a rename in code it does nothing, without an error.
        [Test]
        public void NoEventCallIsStoredInTheScene()
        {
            List<string> storedCalls = new List<string>();
            foreach (MonoBehaviour behaviour in GetComponentsInScene<MonoBehaviour>())
            {
                // A missing script comes back as null, NoScriptIsMissing reports it.
                if (behaviour == null)
                {
                    continue;
                }

                SerializedProperty property = new SerializedObject(behaviour).GetIterator();
                while (property.Next(true))
                {
                    if (property.propertyPath.EndsWith(k_StoredCallsProperty) && property.arraySize > 0)
                    {
                        storedCalls.Add(GetPath(behaviour.transform) + ": " + behaviour.GetType().Name + "."
                            + property.propertyPath);
                    }
                }
            }

            Assert.That(storedCalls, Is.Empty);
        }

        // Our components need every reference they show in the Inspector. A gap only
        // shows once the game gets there, as an error in the middle of a run.
        [Test]
        public void EveryReferenceOfOurComponentsIsSet()
        {
            List<string> unsetReferences = new List<string>();
            foreach (MonoBehaviour behaviour in GetComponentsInScene<MonoBehaviour>())
            {
                // A missing script comes back as null, NoScriptIsMissing reports it.
                if (behaviour == null || behaviour.GetType().Assembly != typeof(GameManager).Assembly)
                {
                    continue;
                }

                SerializedProperty property = new SerializedObject(behaviour).GetIterator();
                while (property.NextVisible(true))
                {
                    if (property.propertyType == SerializedPropertyType.ObjectReference
                        && property.propertyPath != k_ScriptProperty
                        && property.objectReferenceValue == null)
                    {
                        unsetReferences.Add(GetPath(behaviour.transform) + ": " + behaviour.GetType().Name + "."
                            + property.propertyPath);
                    }
                }
            }

            Assert.That(unsetReferences, Is.Empty);
        }

        // A canvas of its own keeps a change from rebuilding the other screens, and
        // without a raycaster of its own no button on the screen takes a click. An
        // animator would rebuild the screen in every frame. The screen manager fades
        // and blocks a screen through its canvas group, which is saved at rest.
        [Test]
        public void EveryScreenHasItsOwnCanvasRaycasterAndCanvasGroupButNoAnimator()
        {
            List<string> problems = new List<string>();
            foreach (Transform screen in GetUiRoot().transform)
            {
                if (screen.GetComponent<Canvas>() == null)
                {
                    problems.Add(screen.name + " has no Canvas");
                }

                if (screen.GetComponent<GraphicRaycaster>() == null)
                {
                    problems.Add(screen.name + " has no GraphicRaycaster");
                }

                CanvasGroup group = screen.GetComponent<CanvasGroup>();
                if (group == null)
                {
                    problems.Add(screen.name + " has no CanvasGroup");
                }
                else if (group.alpha < 1f || !group.blocksRaycasts || !group.interactable || group.ignoreParentGroups)
                {
                    problems.Add(screen.name + " has its CanvasGroup saved faded or blocked");
                }

                if (screen.GetComponent<Animator>() != null)
                {
                    problems.Add(screen.name + " has an Animator");
                }
            }

            Assert.That(problems, Is.Empty);
        }

        // ScreenManager switches every screen it knows off when it shows another one.
        // A screen it does not know is never switched at all. The curtain it knows too. Of the
        // pause it slides only the sheet, which has to lie in the pause.
        [Test]
        public void ScreenManagerKnowsEveryScreenOnce()
        {
            GameObject uiRoot = GetUiRoot();
            ScreenManager screenManager = uiRoot.GetComponent<ScreenManager>();
            Assert.That(screenManager, Is.Not.Null, "No ScreenManager on " + k_UiRootName);

            List<Object> knownScreens = new List<Object>();
            SerializedProperty property = new SerializedObject(screenManager).GetIterator();
            while (property.NextVisible(true))
            {
                if (property.propertyType == SerializedPropertyType.ObjectReference
                    && property.objectReferenceValue is GameObject)
                {
                    knownScreens.Add(property.objectReferenceValue);
                }
            }

            List<Object> screens = new List<Object>();
            foreach (Transform screen in uiRoot.transform)
            {
                screens.Add(screen.gameObject);
            }

            Assert.That(knownScreens, Is.EquivalentTo(screens));
            Transform pause = uiRoot.GetComponentInChildren<PauseMenuScreen>(true).transform;
            Assert.That(screenManager.PauseSheet.IsChildOf(pause), Is.True, "The sheet of the pause lies outside it");
        }

        // The game starts in the main menu, and the scene is saved that way. Another screen
        // switched on would wake up with the scene, maybe before what it shows is ready, and
        // only then would the ScreenManager switch it off.
        [Test]
        public void OnlyTheMainMenuIsSwitchedOn()
        {
            List<string> problems = new List<string>();
            foreach (Transform screen in GetUiRoot().transform)
            {
                bool isMainMenu = screen.GetComponent<MainMenuScreen>() != null;
                if (screen.gameObject.activeSelf != isMainMenu)
                {
                    problems.Add(screen.name + (isMainMenu ? " is off" : " is on"));
                }
            }

            Assert.That(problems, Is.Empty);
        }

        // Hits of the raycasters of different screens are sorted by the sorting of their canvas first, then by when
        // the raycaster came on. The curtain comes on after the screen it covers, so only a sorting above the screens
        // lets it take the taps on them, and it draws over them too.
        [Test]
        public void TheCurtainCoversTheScreensAndTakesTheirTaps()
        {
            GameObject uiRoot = GetUiRoot();
            SerializedObject screenManager = new SerializedObject(uiRoot.GetComponent<ScreenManager>());
            GameObject curtain = (GameObject)screenManager.FindProperty("m_curtain").objectReferenceValue;
            Assert.That(curtain, Is.Not.Null, "The screen manager has no curtain");

            Canvas canvas = curtain.GetComponent<Canvas>();
            Assert.That(canvas.overrideSorting, Is.True, "The curtain sorts with the screens");
            Assert.That(canvas.sortingOrder, Is.GreaterThan(uiRoot.GetComponent<Canvas>().sortingOrder),
                "The curtain does not sort above the screens");

            Graphic graphic = curtain.GetComponent<Graphic>();
            RectTransform place = (RectTransform)curtain.transform;
            Assert.That(graphic != null && graphic.raycastTarget, Is.True, "The curtain has nothing to take taps");
            Assert.That(place.anchorMin == Vector2.zero && place.anchorMax == Vector2.one, Is.True,
                "The curtain does not cover the whole screen");
            Assert.That(place.offsetMin == Vector2.zero && place.offsetMax == Vector2.zero, Is.True,
                "The curtain does not cover the whole screen");
        }

        // All text is TextMesh Pro, which the translations build on. A legacy text would also
        // rebuild its font texture during a run, with almost every new number.
        [Test]
        public void NoLegacyTextIsInTheScene()
        {
            List<string> legacyTexts = new List<string>();
            foreach (Text text in GetComponentsInScene<Text>())
            {
                legacyTexts.Add(GetPath(text.transform));
            }

            Assert.That(legacyTexts, Is.Empty);
        }

        // Asteroids and coins only turn while a SpinManager runs. They come with the tiles at
        // runtime, so a missing manager would only show in the game.
        [Test]
        public void SceneHasOneSpinManager()
        {
            Assert.That(GetComponentsInScene<SpinManager>(), Has.Count.EqualTo(1));
        }

        // Without a FloatingOrigin the world never moves back, and nothing shows until a run
        // gets very long.
        [Test]
        public void SceneHasOneFloatingOrigin()
        {
            Assert.That(GetComponentsInScene<FloatingOrigin>(), Has.Count.EqualTo(1));
        }

        // Without it a tap that wobbles a little on a phone drags the pause or a list, and its button does not click.
        [Test]
        public void TheEventSystemWaitsForTheTouchSlopBeforeADrag()
        {
            List<EventSystem> eventSystems = GetComponentsInScene<EventSystem>();
            Assert.That(eventSystems, Has.Count.EqualTo(1));
            Assert.That(eventSystems[0].GetComponent<DragThreshold>(), Is.Not.Null);
        }

        // The game fills each entry from its achievement when the screen opens. The scene keeps the same texts, so
        // LayoutTests measures the ones the game shows, and an achievement without an entry would not show at all.
        [Test]
        public void EveryAchievementHasOneEntryThatShowsIt()
        {
            LanguageTexts texts = new LanguageTexts(GetDefaultLanguage().Texts.text);
            AchievementCatalog catalog = AssetDatabase.LoadAssetAtPath<AchievementCatalog>(k_AchievementCatalogPath);
            List<AchievementData> shown = new List<AchievementData>();
            List<string> problems = new List<string>();
            foreach (AchievementEntry entry in GetComponentsInScene<AchievementEntry>())
            {
                SerializedObject serialized = new SerializedObject(entry);
                AchievementData achievement = entry.Achievement;
                shown.Add(achievement);
                string path = GetPath(entry.transform);
                CompareText(problems, path, serialized, "m_titleText", achievement.Title);
                CompareKey(problems, path, serialized, "m_descriptionText", achievement.DescriptionKey);
                CompareText(problems, path, serialized, "m_descriptionText", texts.Get(achievement.DescriptionKey)
                    .Replace("{0}", achievement.Target.ToString(CultureInfo.InvariantCulture)));
                CompareKey(problems, path, serialized, "m_progressText", achievement.ProgressKey);
                CompareText(problems, path, serialized, "m_rewardText", "+" + achievement.Reward);
                CompareText(problems, path, serialized, "m_claimText", achievement.Reward.ToString());
                Image icon = (Image)serialized.FindProperty("m_icon").objectReferenceValue;
                if (icon.sprite != achievement.Icon)
                {
                    problems.Add(path + " shows the icon " + icon.sprite.name + " instead of " + achievement.Icon.name);
                }
            }

            Assert.That(shown, Is.EquivalentTo(catalog.Achievements), "Not one entry per achievement");
            Assert.That(problems, Is.Empty);
        }

        // An element in a color of the default colorway without a role keeps that color in every other one. Left out
        // are the elements that scripts color by their state and the cards of the colorways screen, each in the colors
        // of its own colorway.
        [Test]
        public void EveryElementInAColorOfTheColorwayHasARole()
        {
            ColorwayData colorway = AssetDatabase.LoadAssetAtPath<ColorwayCatalog>(k_ColorwayCatalogPath).Default;
            HashSet<Object> coloredByScripts = GetGraphicsColoredByScripts();
            Color[] colors =
            {
                colorway.Background, colorway.Glow, colorway.GlowShadow, colorway.TextOnGlow, colorway.Starlight,
                colorway.TextOnStarlight,
            };
            List<string> problems = new List<string>();
            foreach (Graphic graphic in GetUiRoot().GetComponentsInChildren<Graphic>(true))
            {
                if (graphic.GetComponent<ColorwayTint>() != null || coloredByScripts.Contains(graphic) ||
                    graphic.GetComponentInParent<ColorwayCard>(true) != null)
                {
                    continue;
                }

                foreach (Color color in colors)
                {
                    if (GetDifference(graphic.color, color) <= k_ColorTolerance)
                    {
                        problems.Add(GetPath(graphic.transform) + " is in a color of the colorway, but has no role");
                        break;
                    }
                }
            }

            Assert.That(problems, Is.Empty);
        }

        // The scene holds the default colorway, so the editor and the tests see what a new installation shows.
        [Test]
        public void EveryRoleShowsTheDefaultColorway()
        {
            ColorwayData colorway = AssetDatabase.LoadAssetAtPath<ColorwayCatalog>(k_ColorwayCatalogPath).Default;
            List<ColorwayTint> tints = GetComponentsInScene<ColorwayTint>();
            List<string> problems = new List<string>();
            foreach (ColorwayTint tint in tints)
            {
                Graphic graphic = tint.GetComponent<Graphic>();
                Color expected = tint.GetColor(colorway);
                if (GetDifference(graphic.color, expected) > k_ColorTolerance ||
                    Mathf.Abs(graphic.color.a - expected.a) > k_ColorTolerance)
                {
                    problems.Add(GetPath(tint.transform) + " is #" + ColorUtility.ToHtmlStringRGBA(graphic.color) +
                        " instead of #" + ColorUtility.ToHtmlStringRGBA(expected));
                }

                if (tint.IsGlow && ((Image)graphic).sprite != tint.GetGlowImage(colorway))
                {
                    problems.Add(GetPath(tint.transform) + " does not show the glow of " + colorway.name);
                }
            }

            Assert.That(tints, Is.Not.Empty, "Nothing in the scene follows the colorway");
            Assert.That(problems, Is.Empty);
        }

        [Test]
        public void EveryColorwayHasOneCardThatShowsIt()
        {
            ColorwayCatalog catalog = AssetDatabase.LoadAssetAtPath<ColorwayCatalog>(k_ColorwayCatalogPath);
            List<ColorwaysScreen> screens = GetComponentsInScene<ColorwaysScreen>();
            Assert.That(screens, Has.Count.EqualTo(1), "Not one colorways screen");
            List<ColorwayManager> managers = GetComponentsInScene<ColorwayManager>();
            Assert.That(managers, Has.Count.EqualTo(1), "Not one colorway manager");
            Assert.That(new SerializedObject(managers[0]).FindProperty("m_catalog").objectReferenceValue,
                Is.EqualTo(catalog), "The colorway manager does not know the catalog");

            List<ColorwayData> shown = new List<ColorwayData>();
            List<string> problems = new List<string>();
            foreach (ColorwayCard card in screens[0].Cards)
            {
                ColorwayData colorway = card.Colorway;
                shown.Add(colorway);
                SerializedObject serialized = new SerializedObject(card);
                string path = GetPath(card.transform);
                CompareText(problems, path, serialized, "m_titleText", colorway.Title);
                CompareColor(problems, path, serialized, "m_background", colorway.Background);
                CompareColor(problems, path, serialized, "m_glowSwatch", colorway.Glow);
                CompareColor(problems, path, serialized, "m_starlightSwatch", colorway.Starlight);
                CompareColor(problems, path, serialized, "m_checkPlate", colorway.Glow);
                CompareColor(problems, path, serialized, "m_checkIcon", colorway.TextOnGlow);
                bool isDefault = colorway == catalog.Default;
                if (card.Check.activeSelf != isDefault)
                {
                    problems.Add(path + (isDefault ? " shows no check" : " shows the check"));
                }
            }

            Assert.That(shown, Is.EqualTo(catalog.Colorways), "Not one card per colorway in the order of the catalog");
            Assert.That(problems, Is.Empty);
        }

        [Test]
        public void SceneHasOneLanguageManagerThatKnowsTheCatalog()
        {
            List<LanguageManager> managers = GetComponentsInScene<LanguageManager>();
            Assert.That(managers, Has.Count.EqualTo(1), "Not one language manager");
            Assert.That(new SerializedObject(managers[0]).FindProperty("m_catalog").objectReferenceValue,
                Is.EqualTo(AssetDatabase.LoadAssetAtPath<LanguageCatalog>(k_LanguageCatalogPath)),
                "The language manager does not know the catalog");
        }

        // The scene holds the default language, so the editor and the tests see what an English phone shows. A key
        // that the default language does not have would show itself. A text with numbers holds an example of them,
        // like RUN ENDED · 2:05.
        [Test]
        public void EveryLocalizedTextShowsTheDefaultLanguage()
        {
            LanguageData language = GetDefaultLanguage();
            LanguageTexts texts = new LanguageTexts(language.Texts.text);
            List<string> problems = new List<string>();
            foreach (LocalizedText localized in GetComponentsInScene<LocalizedText>())
            {
                string path = GetPath(localized.transform);
                if (!texts.Contains(localized.Key))
                {
                    problems.Add(path + " has the key \"" + localized.Key + "\", which " + language.name +
                        " does not have");
                    continue;
                }

                string shown = localized.Text.text;
                if (!MatchesText(shown, texts.Get(localized.Key)))
                {
                    problems.Add(path + " shows \"" + shown + "\" instead of \"" + texts.Get(localized.Key) +
                        "\" of " + localized.Key);
                }
            }

            Assert.That(problems, Is.Empty);
        }

        // The settings have one button per language, in the order of the catalog, with the tag of its language on it.
        [Test]
        public void EveryLanguageHasOneButtonThatChoosesIt()
        {
            LanguageCatalog catalog = AssetDatabase.LoadAssetAtPath<LanguageCatalog>(k_LanguageCatalogPath);
            SerializedProperty buttons = new SerializedObject(GetComponentsInScene<SettingsScreen>()[0])
                .FindProperty("m_languageButtons");
            List<LanguageData> shown = new List<LanguageData>();
            List<string> problems = new List<string>();
            for (int i = 0; i < buttons.arraySize; i++)
            {
                LanguageButton button = (LanguageButton)buttons.GetArrayElementAtIndex(i).objectReferenceValue;
                shown.Add(button.Language);
                string label = button.GetComponentInChildren<TMP_Text>(true).text;
                if (label != button.Language.Id.ToUpperInvariant())
                {
                    problems.Add(GetPath(button.transform) + " shows " + label + " for " + button.Language.Id);
                }
            }

            Assert.That(GetComponentsInScene<LanguageButton>(), Has.Count.EqualTo(buttons.arraySize),
                "A language button the settings do not know");
            Assert.That(shown, Is.EqualTo(catalog.Languages),
                "Not one button per language in the order of the catalog");
            Assert.That(problems, Is.Empty);
        }

        // TextMesh Pro draws a character its font lacks from LiberationSans, in another look, like the capital ẞ in
        // Audiowide. Every text has its characters in its own font, in every language, and a key that only scripts
        // show has them in every font of the texts with a key.
        [Test]
        public void EveryCharacterOfEveryTextIsInItsFont()
        {
            LanguageCatalog catalog = AssetDatabase.LoadAssetAtPath<LanguageCatalog>(k_LanguageCatalogPath);
            LanguageTexts defaultTexts = new LanguageTexts(catalog.Default.Texts.text);
            List<LanguageTexts> languages = catalog.Languages
                .Select(language => new LanguageTexts(language.Texts.text, defaultTexts))
                .ToList();
            HashSet<string> shownKeys = new HashSet<string>();
            HashSet<TMP_FontAsset> fonts = new HashSet<TMP_FontAsset>();
            List<string> problems = new List<string>();
            foreach (TMP_Text text in GetComponentsInScene<TMP_Text>())
            {
                LocalizedText localized = text.GetComponent<LocalizedText>();
                if (localized == null)
                {
                    AddMissingCharacters(problems, GetPath(text.transform), text.text, text.font);
                    continue;
                }

                shownKeys.Add(localized.Key);
                fonts.Add(text.font);
                for (int i = 0; i < languages.Count; i++)
                {
                    string name = GetPath(text.transform) + " in " + catalog.Languages[i].Id;
                    AddMissingCharacters(problems, name, languages[i].Get(localized.Key), text.font);
                }
            }

            foreach (string key in defaultTexts.Keys.Where(key => !shownKeys.Contains(key)))
            {
                for (int i = 0; i < languages.Count; i++)
                {
                    foreach (TMP_FontAsset font in fonts)
                    {
                        string name = key + " in " + catalog.Languages[i].Id;
                        AddMissingCharacters(problems, name, languages[i].Get(key), font);
                    }
                }
            }

            Assert.That(problems, Is.Empty);
        }

        // A text without a key shows English in every language. Names and license texts stay the same in every
        // language and say so with UntranslatedText.
        [Test]
        public void EveryTextWithLettersHasAKeyOrStaysTheSame()
        {
            List<string> problems = new List<string>();
            foreach (TMP_Text text in GetComponentsInScene<TMP_Text>())
            {
                bool hasKey = text.GetComponent<LocalizedText>() != null;
                bool staysTheSame = text.GetComponent<UntranslatedText>() != null;
                if (hasKey && staysTheSame)
                {
                    problems.Add(GetPath(text.transform) + " has a key, but stays the same in every language");
                }
                else if (!hasKey && !staysTheSame && text.text.Any(char.IsLetter))
                {
                    problems.Add(GetPath(text.transform) + " has no key for \"" + text.text + "\"");
                }
            }

            Assert.That(problems, Is.Empty);
        }

        private static LanguageData GetDefaultLanguage()
        {
            return AssetDatabase.LoadAssetAtPath<LanguageCatalog>(k_LanguageCatalogPath).Default;
        }

        // The placeholders of a format stand for numbers, which every font has.
        private static void AddMissingCharacters(List<string> problems, string name, string text, TMP_FontAsset font)
        {
            if (font == null)
            {
                problems.Add(name + " has no font");
                return;
            }

            string shown = s_placeholder.Replace(text ?? string.Empty, "0");
            SortedSet<string> missing = new SortedSet<string>();
            for (int i = 0; i < shown.Length; i += char.IsSurrogatePair(shown, i) ? 2 : 1)
            {
                int character = char.ConvertToUtf32(shown, i);
                if (!char.IsControl(shown, i) && !font.characterLookupTable.ContainsKey((uint)character))
                {
                    missing.Add(char.ConvertFromUtf32(character));
                }
            }

            if (missing.Count > 0)
            {
                problems.Add(name + " lacks " + string.Join(" ", missing) + " in " + font.name);
            }
        }

        private static bool MatchesText(string shown, string text)
        {
            if (!s_placeholder.IsMatch(text))
            {
                return shown == text;
            }

            string pattern = string.Join(k_NumberPattern, s_placeholder.Split(text).Select(Regex.Escape));
            return Regex.IsMatch(shown, "^" + pattern + "$");
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

        private List<T> GetComponentsInScene<T>() where T : Component
        {
            List<T> components = new List<T>();
            foreach (GameObject root in m_scene.GetRootGameObjects())
            {
                components.AddRange(root.GetComponentsInChildren<T>(true));
            }

            return components;
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

        // What scripts color by state takes the colorway from roles of its own instead of a ColorwayTint, also a text
        // they reach through its LocalizedText.
        private HashSet<Object> GetGraphicsColoredByScripts()
        {
            HashSet<Object> graphics = new HashSet<Object>();
            foreach (MonoBehaviour behaviour in GetUiRoot().GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (!(behaviour is AchievementEntry || behaviour is OnOffButton || behaviour is SelectOrBuyButton ||
                    behaviour is ToggleSwitch || behaviour is LanguageButton))
                {
                    continue;
                }

                SerializedProperty property = new SerializedObject(behaviour).GetIterator();
                while (property.Next(true))
                {
                    if (property.propertyType != SerializedPropertyType.ObjectReference)
                    {
                        continue;
                    }

                    if (property.objectReferenceValue is Graphic)
                    {
                        graphics.Add(property.objectReferenceValue);
                    }
                    else if (property.objectReferenceValue is LocalizedText localized)
                    {
                        graphics.Add(localized.Text);
                    }
                }
            }

            return graphics;
        }

        // The largest difference in red, green or blue.
        private static float GetDifference(Color a, Color b)
        {
            return Mathf.Max(Mathf.Abs(a.r - b.r), Mathf.Max(Mathf.Abs(a.g - b.g), Mathf.Abs(a.b - b.b)));
        }

        private static void CompareColor(List<string> problems, string path, SerializedObject card, string field,
            Color expected)
        {
            Graphic graphic = (Graphic)card.FindProperty(field).objectReferenceValue;
            if (GetDifference(graphic.color, expected) > k_ColorTolerance)
            {
                problems.Add(path + "." + field + " is #" + ColorUtility.ToHtmlStringRGB(graphic.color) +
                    " instead of #" + ColorUtility.ToHtmlStringRGB(expected));
            }
        }

        private static void CompareText(List<string> problems, string path, SerializedObject entry, string field,
            string expected)
        {
            Object value = entry.FindProperty(field).objectReferenceValue;
            TMP_Text text = value is LocalizedText localized ? localized.Text : (TMP_Text)value;
            if (text.text != expected)
            {
                problems.Add(path + "." + field + " shows \"" + text.text + "\" instead of \"" + expected + "\"");
            }
        }

        private static void CompareKey(List<string> problems, string path, SerializedObject entry, string field,
            string expected)
        {
            LocalizedText text = (LocalizedText)entry.FindProperty(field).objectReferenceValue;
            if (text.Key != expected)
            {
                problems.Add(path + "." + field + " shows " + text.Key + " instead of " + expected);
            }
        }
    }
}
