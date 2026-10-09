using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SpaceEscaper.Tests
{
    public class SceneTests
    {
        private const string k_ScenePath = "Assets/Scenes/Game.unity";
        private const string k_UiRootName = "UI";

        // Serialized names in Unity's own components, which have no other way in.
        private const string k_ScriptProperty = "m_Script";
        private const string k_ButtonCallsProperty = "m_OnClick.m_PersistentCalls.m_Calls";
        private const string k_CallTargetProperty = "m_Target";
        private const string k_CallMethodProperty = "m_MethodName";
        private const string k_CallModeProperty = "m_Mode";
        private const string k_CallStateProperty = "m_CallState";
        private const string k_CallArgumentsProperty = "m_Arguments";
        private const string k_ObjectArgumentProperty = "m_ObjectArgument";
        private const string k_ObjectArgumentTypeProperty = "m_ObjectArgumentAssemblyTypeName";

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

        // The scene stores each button call as the name of a method. After a rename
        // in code the button does nothing, without an error.
        [Test]
        public void EveryButtonCallFindsItsTargetAndMethod()
        {
            List<string> brokenCalls = new List<string>();
            foreach (Button button in GetComponentsInScene<Button>())
            {
                SerializedProperty calls = new SerializedObject(button).FindProperty(k_ButtonCallsProperty);
                for (int i = 0; i < calls.arraySize; i++)
                {
                    string problem = FindProblem(calls.GetArrayElementAtIndex(i));
                    if (problem != null)
                    {
                        brokenCalls.Add(GetPath(button.transform) + ": " + problem);
                    }
                }
            }

            Assert.That(brokenCalls, Is.Empty);
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

        private static string FindProblem(SerializedProperty call)
        {
            Object target = call.FindPropertyRelative(k_CallTargetProperty).objectReferenceValue;
            string methodName = call.FindPropertyRelative(k_CallMethodProperty).stringValue;
            if (target == null)
            {
                return "no target for " + methodName;
            }

            string callName = target.GetType().Name + "." + methodName;
            PersistentListenerMode mode =
                (PersistentListenerMode)call.FindPropertyRelative(k_CallModeProperty).intValue;
            SerializedProperty arguments = call.FindPropertyRelative(k_CallArgumentsProperty);
            if (UnityEventBase.GetValidMethodInfo(target, methodName, GetArgumentTypes(mode, arguments)) == null)
            {
                return callName + " not found";
            }

            if (mode == PersistentListenerMode.Object
                && arguments.FindPropertyRelative(k_ObjectArgumentProperty).objectReferenceValue == null)
            {
                return callName + " without its argument";
            }

            UnityEventCallState state = (UnityEventCallState)call.FindPropertyRelative(k_CallStateProperty).intValue;
            if (state == UnityEventCallState.Off)
            {
                return callName + " switched off";
            }

            return null;
        }

        private static Type[] GetArgumentTypes(PersistentListenerMode mode, SerializedProperty arguments)
        {
            switch (mode)
            {
                case PersistentListenerMode.Object:
                    string typeName = arguments.FindPropertyRelative(k_ObjectArgumentTypeProperty).stringValue;
                    return new[] { Type.GetType(typeName, false) ?? typeof(Object) };
                case PersistentListenerMode.Int:
                    return new[] { typeof(int) };
                case PersistentListenerMode.Float:
                    return new[] { typeof(float) };
                case PersistentListenerMode.String:
                    return new[] { typeof(string) };
                case PersistentListenerMode.Bool:
                    return new[] { typeof(bool) };
                default:
                    // Void, or the arguments of the event itself, of which a click has none.
                    return Type.EmptyTypes;
            }
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
