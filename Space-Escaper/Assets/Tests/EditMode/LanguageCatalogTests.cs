using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace SpaceEscaper.Tests
{
    public class LanguageCatalogTests
    {
        private const string k_CatalogPath = "Assets/Data/Languages/LanguageCatalog.asset";

        // English stands in for every language the game does not have and for every text another language lacks.
        private const string k_DefaultId = "en";
        private const string k_GermanId = "de";

        private LanguageCatalog m_catalog;

        [SetUp]
        public void LoadCatalog()
        {
            m_catalog = AssetDatabase.LoadAssetAtPath<LanguageCatalog>(k_CatalogPath);
        }

        [Test]
        public void CatalogListsEveryLanguageOnce()
        {
            string[] assets = AssetDatabase.FindAssets("t:" + nameof(LanguageData), new[] { "Assets" });
            HashSet<LanguageData> listed = new HashSet<LanguageData>(m_catalog.Languages);

            Assert.That(m_catalog.Languages, Has.None.Null);
            Assert.That(listed.Count, Is.EqualTo(m_catalog.Languages.Count), "A language is listed twice.");
            Assert.That(listed.Count, Is.EqualTo(assets.Length), "A language asset is missing in the catalog.");
        }

        // A second language for the same system language would never start on a phone.
        [Test]
        public void LanguageIdsAndSystemLanguagesAreSetAndUnique()
        {
            HashSet<string> ids = new HashSet<string>();
            HashSet<SystemLanguage> systemLanguages = new HashSet<SystemLanguage>();
            foreach (LanguageData language in m_catalog.Languages)
            {
                Assert.That(language.Id, Is.Not.Empty, $"{language.name} has no ID");
                Assert.That(ids.Add(language.Id), Is.True, $"The ID {language.Id} is used twice");
                Assert.That(language.SystemLanguage, Is.Not.EqualTo(SystemLanguage.Unknown),
                    $"{language.name} has no system language");
                Assert.That(systemLanguages.Add(language.SystemLanguage), Is.True,
                    $"The system language {language.SystemLanguage} is used twice");
            }
        }

        [Test]
        public void TheDefaultIsEnglish()
        {
            Assert.That(m_catalog.Default.Id, Is.EqualTo(k_DefaultId));
            Assert.That(m_catalog.Default.SystemLanguage, Is.EqualTo(SystemLanguage.English));
        }

        // Moritz, 25.11.2026: "Deutsche Handys starten auf Deutsch, alle anderen auf Englisch."
        [Test]
        public void UntilALanguageIsChosenGermanPhonesStartInGermanAndAllOthersInEnglish()
        {
            Assert.That(m_catalog.Find("", SystemLanguage.German).Id, Is.EqualTo(k_GermanId));
            Assert.That(m_catalog.Find("", SystemLanguage.English).Id, Is.EqualTo(k_DefaultId));
            Assert.That(m_catalog.Find("", SystemLanguage.French).Id, Is.EqualTo(k_DefaultId));
            Assert.That(m_catalog.Find("", SystemLanguage.Unknown).Id, Is.EqualTo(k_DefaultId));
            Assert.That(m_catalog.Find("NoSuchLanguage", SystemLanguage.German).Id, Is.EqualTo(k_GermanId));
        }

        [Test]
        public void TheChosenLanguageWinsOverThePhone()
        {
            foreach (LanguageData language in m_catalog.Languages)
            {
                foreach (LanguageData phone in m_catalog.Languages)
                {
                    Assert.That(m_catalog.Find(language.Id, phone.SystemLanguage), Is.EqualTo(language));
                }

                Assert.That(m_catalog.Find(language.Id, SystemLanguage.Unknown), Is.EqualTo(language));
            }
        }

        [Test]
        public void EveryLanguageHasTheTextsOfTheDefaultAndNoOthers()
        {
            LanguageTexts defaultTexts = Read(m_catalog.Default);
            List<string> problems = new List<string>();
            foreach (LanguageData language in m_catalog.Languages)
            {
                LanguageTexts texts = Read(language);
                foreach (string key in defaultTexts.Keys.Where(key => !texts.Contains(key)))
                {
                    problems.Add($"{language.name} has no text for {key}");
                }

                foreach (string key in texts.Keys.Where(key => !defaultTexts.Contains(key)))
                {
                    problems.Add($"{language.name} has a text for {key}, which {m_catalog.Default.name} does not have");
                }

                foreach (string key in texts.Keys.Where(key => texts.Get(key).Trim().Length == 0))
                {
                    problems.Add($"{language.name} has an empty text for {key}");
                }
            }

            Assert.That(problems, Is.Empty);
        }

        // The scripts fill them in with numbers, in whatever order a language puts them.
        [Test]
        public void EveryTextHasThePlaceholdersOfTheDefault()
        {
            LanguageTexts defaultTexts = Read(m_catalog.Default);
            List<string> problems = new List<string>();
            foreach (LanguageData language in m_catalog.Languages)
            {
                LanguageTexts texts = Read(language);
                foreach (string key in texts.Keys.Where(defaultTexts.Contains))
                {
                    string expected = string.Join(" ", GetPlaceholders(defaultTexts.Get(key)));
                    string placeholders = string.Join(" ", GetPlaceholders(texts.Get(key)));
                    if (placeholders != expected)
                    {
                        problems.Add($"{language.name}: {key} has \"{placeholders}\" instead of \"{expected}\"");
                    }
                }
            }

            Assert.That(problems, Is.Empty);
        }

        // Titles, buttons and labels are capitals in every language. Audiowide has no capital ẞ, so German writes
        // SS there, and a small ß would stand out.
        [Test]
        public void TextsInCapitalsAreInCapitalsInEveryLanguage()
        {
            LanguageTexts defaultTexts = Read(m_catalog.Default);
            List<string> problems = new List<string>();
            foreach (LanguageData language in m_catalog.Languages)
            {
                LanguageTexts texts = Read(language);
                foreach (string key in texts.Keys.Where(key => IsInCapitals(defaultTexts.Get(key))))
                {
                    string text = texts.Get(key);
                    if (text.Any(char.IsLower))
                    {
                        problems.Add($"{language.name}: {key} has small letters in \"{text}\"");
                    }
                }
            }

            Assert.That(problems, Is.Empty);
        }

        private static LanguageTexts Read(LanguageData language)
        {
            Assert.That(language.Texts, Is.Not.Null, $"{language.name} has no texts");
            return new LanguageTexts(language.Texts.text);
        }

        private static IEnumerable<string> GetPlaceholders(string text)
        {
            return TextWithNumbers.Placeholder.Matches(text).Cast<Match>().Select(match => match.Value)
                .OrderBy(value => value, StringComparer.Ordinal);
        }

        private static bool IsInCapitals(string text)
        {
            return text.Any(char.IsLetter) && !text.Any(char.IsLower);
        }
    }
}
