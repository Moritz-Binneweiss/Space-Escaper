using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace SpaceEscaper.Tests
{
    public class NumberFormatTests
    {
        private const char k_Comma = ',';
        private const char k_Point = '.';
        private const string k_LanguageCatalogPath = "Assets/Data/Languages/LanguageCatalog.asset";

        // Moritz, 18.11.2026: large numbers get separators like 9,142 in the mockups.
        [TestCase(0f, "0")]
        [TestCase(-0.4f, "0")]
        [TestCase(999f, "999")]
        [TestCase(1000f, "1,000")]
        [TestCase(9142f, "9,142")]
        [TestCase(1234567f, "1,234,567")]
        [TestCase(-1250f, "-1,250")]
        public void GroupsTheThousandsOfWholeNumbers(float number, string expected)
        {
            Assert.That(NumberFormat.Format("{0}", number, 0f, k_Comma, k_Point), Is.EqualTo(expected));
        }

        // In German 9.142 and ×1,4.
        [Test]
        public void TakesTheSeparatorsOfTheLanguage()
        {
            Assert.That(NumberFormat.Format("{0} / {1}", 9142f, 1234567f, k_Point, k_Comma),
                Is.EqualTo("9.142 / 1.234.567"));
            Assert.That(NumberFormat.Format("×{0:0.0}", 1.4f, 0f, k_Point, k_Comma), Is.EqualTo("×1,4"));
        }

        [TestCase("RUN ENDED · {0}:{1:00}", 2f, 5f, "RUN ENDED · 2:05")]
        [TestCase("{0}:{1:00}", 12f, 34f, "12:34")]
        [TestCase("×{0:0.0}", 1.4f, 0f, "×1.4")]
        [TestCase("×{0:0.0}", 1.25f, 0f, "×1.3")]
        [TestCase("{0} m", 2.5f, 0f, "3 m")]
        [TestCase("+{1} {", 0f, 7f, "+7 {")]
        public void FillsInTheNumbersAsTheFormatSays(string format, float first, float second, string expected)
        {
            Assert.That(NumberFormat.Format(format, first, second, k_Comma, k_Point), Is.EqualTo(expected));
        }

        [Test]
        public void GrowsItsBuffer()
        {
            char[] buffer = new char[2];
            int length = NumberFormat.Format("Collect {0} coins in total", 1000f, 0f, k_Comma, k_Point, ref buffer);
            Assert.That(new string(buffer, 0, length), Is.EqualTo("Collect 1,000 coins in total"));
        }

        // A text with a key fills in its numbers with the separators of the language it shows.
        [Test]
        public void LocalizedTextsShowTheirNumbersInTheLanguage()
        {
            LanguageCatalog catalog = AssetDatabase.LoadAssetAtPath<LanguageCatalog>(k_LanguageCatalogPath);
            LanguageData previous = LanguageManager.Current;
            GameObject gameObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            try
            {
                LocalizedText text = gameObject.AddComponent<LocalizedText>();
                AssertShows(text, catalog, "en", "1,938 m");
                AssertShows(text, catalog, "de", "1.938 m");
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
                LanguageManager.Show(catalog, previous != null ? previous : catalog.Default);
            }
        }

        private static void AssertShows(LocalizedText text, LanguageCatalog catalog, string languageId, string expected)
        {
            LanguageManager.Show(catalog, catalog.Find(languageId, SystemLanguage.Unknown));
            text.Show("common.meters", 1938f);
            Assert.That(text.Text.text, Is.EqualTo(expected), "In " + languageId);
        }
    }
}
