using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceEscaper.Tests
{
    /// <summary>
    /// The credits at the top of the README are those of the game, so a change to the one needs the other.
    /// </summary>
    public class CreditsTests
    {
        private const string k_ScenePath = "Assets/Scenes/Game.unity";
        private const string k_UiRootName = "UI";
        private const string k_ContentPath = "Credits/SafeArea/Roll/Content";
        private const string k_HeaderText = "Header";
        private const string k_Entry = "Entry";
        private const string k_NameText = "Name";
        private const string k_RoleText = "Role";
        private const string k_ReadmeFileName = "README.md";
        private const string k_ReadmeHeadingStart = "## ";
        private const string k_ReadmeHeadingEnd = "**Credits**";
        private const string k_Separator = " - ";

        // A section in the README is "### **Current Team**", an entry "- **Name** - Role", the role may be missing.
        private static readonly Regex s_ReadmeSection = new Regex(@"^### \*\*(.+)\*\*$");
        private static readonly Regex s_ReadmeEntry = new Regex(@"^- \*\*(.+?)\*\*(?: - (.+))?$");

        private Scene m_scene;
        private bool m_isOpenedByTest;

        // Opened next to the empty scene of the test runner and closed again, like in SceneTests.
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
        public void TheReadmeListsTheCreditsOfTheGame()
        {
            Assert.That(ReadReadme(), Is.EqualTo(ReadCreditsScreen()));
        }

        // Each section with entries as its header in capitals, then a line per entry. The licenses have no
        // entries, the README leaves their full texts to the files next to the fonts and icons.
        private List<string> ReadCreditsScreen()
        {
            GameObject uiRoot = System.Array.Find(m_scene.GetRootGameObjects(), root => root.name == k_UiRootName);
            Assert.That(uiRoot, Is.Not.Null, "No " + k_UiRootName + " in " + k_ScenePath);
            Transform content = uiRoot.transform.Find(k_ContentPath);
            Assert.That(content, Is.Not.Null, "No " + k_ContentPath + " under " + k_UiRootName);

            List<string> lines = new List<string>();
            foreach (Transform section in content)
            {
                Transform header = section.Find(k_HeaderText);
                if (header == null)
                {
                    continue;
                }

                List<string> entries = new List<string>();
                foreach (Transform entry in section)
                {
                    if (entry.name != k_Entry)
                    {
                        continue;
                    }

                    string name = entry.Find(k_NameText).GetComponent<TMP_Text>().text;
                    Transform role = entry.Find(k_RoleText);
                    entries.Add(role != null ? name + k_Separator + role.GetComponent<TMP_Text>().text : name);
                }

                if (entries.Count > 0)
                {
                    lines.Add(header.GetComponent<TMP_Text>().text);
                    lines.AddRange(entries);
                }
            }

            return lines;
        }

        private static List<string> ReadReadme()
        {
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", k_ReadmeFileName));
            List<string> lines = new List<string>();
            bool isInCredits = false;
            foreach (string line in File.ReadAllLines(path))
            {
                if (line.StartsWith(k_ReadmeHeadingStart))
                {
                    isInCredits = line.EndsWith(k_ReadmeHeadingEnd);
                    continue;
                }

                if (!isInCredits)
                {
                    continue;
                }

                Match section = s_ReadmeSection.Match(line);
                if (section.Success)
                {
                    lines.Add(section.Groups[1].Value.ToUpperInvariant());
                    continue;
                }

                Match entry = s_ReadmeEntry.Match(line);
                if (entry.Success)
                {
                    lines.Add(entry.Groups[2].Success
                        ? entry.Groups[1].Value + k_Separator + entry.Groups[2].Value
                        : entry.Groups[1].Value);
                }
            }

            return lines;
        }
    }
}
