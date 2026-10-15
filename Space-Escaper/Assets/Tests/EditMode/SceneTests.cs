using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceEscaper.Tests
{
    public class SceneTests
    {
        private const string k_ScenePath = "Assets/Scenes/Game.unity";
        private const string k_UiRootName = "UI";

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
        // animator would rebuild the screen in every frame.
        [Test]
        public void EveryScreenHasItsOwnCanvasAndRaycasterButNoAnimator()
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

                if (screen.GetComponent<Animator>() != null)
                {
                    problems.Add(screen.name + " has an Animator");
                }
            }

            Assert.That(problems, Is.Empty);
        }

        // ScreenManager switches every screen it knows off when it shows another one.
        // A screen it does not know is never switched at all.
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
                    && property.propertyPath != k_ScriptProperty)
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
    }
}
