using System.Collections.Generic;
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
                CompareText(problems, path, serialized, "m_descriptionText", achievement.Description);
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

        // What scripts color by state takes the colorway from roles of its own instead of a ColorwayTint.
        private HashSet<Object> GetGraphicsColoredByScripts()
        {
            HashSet<Object> graphics = new HashSet<Object>();
            foreach (MonoBehaviour behaviour in GetUiRoot().GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (!(behaviour is AchievementEntry || behaviour is OnOffButton || behaviour is SelectOrBuyButton ||
                    behaviour is ToggleSwitch))
                {
                    continue;
                }

                SerializedProperty property = new SerializedObject(behaviour).GetIterator();
                while (property.Next(true))
                {
                    if (property.propertyType == SerializedPropertyType.ObjectReference &&
                        property.objectReferenceValue is Graphic)
                    {
                        graphics.Add(property.objectReferenceValue);
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
            TMP_Text text = (TMP_Text)entry.FindProperty(field).objectReferenceValue;
            if (text.text != expected)
            {
                problems.Add(path + "." + field + " shows \"" + text.text + "\" instead of \"" + expected + "\"");
            }
        }
    }
}
