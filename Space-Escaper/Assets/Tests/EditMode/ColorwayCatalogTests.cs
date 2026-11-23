using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace SpaceEscaper.Tests
{
    public class ColorwayCatalogTests
    {
        private const string k_CatalogPath = "Assets/Data/Colorways/ColorwayCatalog.asset";

        // A new installation looks like the game did before the colorways.
        private const string k_DefaultId = "Toxic";

        // WCAG AA for text of normal size.
        private const float k_MinTextContrast = 4.5f;

        // The descriptions under titles and the labels of numbers, the same in every colorway.
        private static readonly Color s_descriptionColor = new Color32(0xB7, 0xC0, 0xCF, 0xFF);
        private static readonly Color s_labelColor = new Color32(0x9A, 0xA7, 0xBD, 0xFF);

        private ColorwayCatalog m_catalog;

        [SetUp]
        public void LoadCatalog()
        {
            m_catalog = AssetDatabase.LoadAssetAtPath<ColorwayCatalog>(k_CatalogPath);
        }

        [Test]
        public void CatalogListsEveryColorwayOnce()
        {
            string[] assets = AssetDatabase.FindAssets("t:" + nameof(ColorwayData), new[] { "Assets" });
            HashSet<ColorwayData> listed = new HashSet<ColorwayData>(m_catalog.Colorways);

            Assert.That(m_catalog.Colorways, Has.None.Null);
            Assert.That(listed.Count, Is.EqualTo(m_catalog.Colorways.Count), "A colorway is listed twice.");
            Assert.That(listed.Count, Is.EqualTo(assets.Length), "A colorway asset is missing in the catalog.");
        }

        [Test]
        public void ColorwayIdsAreSetAndUnique()
        {
            HashSet<string> ids = new HashSet<string>();
            foreach (ColorwayData colorway in m_catalog.Colorways)
            {
                Assert.That(colorway.Id, Is.Not.Empty, $"{colorway.name} has no ID");
                Assert.That(ids.Add(colorway.Id), Is.True, $"The ID {colorway.Id} is used twice");
            }
        }

        // Titles are capitals, like everything else in Audiowide. The colors are drawn as they are, see-through
        // ones come from the roles of the elements.
        [Test]
        public void EveryColorwayHasTitleOpaqueColorsAndBothGlows()
        {
            foreach (ColorwayData colorway in m_catalog.Colorways)
            {
                Assert.That(colorway.Title, Is.Not.Empty, $"{colorway.name} has no title");
                Assert.That(colorway.Title, Is.EqualTo(colorway.Title.ToUpperInvariant()),
                    $"The title of {colorway.name} is not in capitals");
                foreach (Color color in GetColors(colorway))
                {
                    Assert.That(color.a, Is.EqualTo(1f), $"{colorway.name} has a see-through color");
                }

                Assert.That(colorway.GlowImage, Is.Not.Null, $"{colorway.name} has no image for its glow");
                Assert.That(colorway.StarlightGlowImage, Is.Not.Null,
                    $"{colorway.name} has no image for the glow of its starlight");
            }
        }

        [Test]
        public void TheDefaultIsTheLookOfBeforeTheColorways()
        {
            Assert.That(m_catalog.Default.Id, Is.EqualTo(k_DefaultId));
        }

        // A save from before the colorways has no ID, and one could name a colorway that is gone.
        [Test]
        public void AnIdOfNoColorwayFindsTheDefault()
        {
            foreach (ColorwayData colorway in m_catalog.Colorways)
            {
                Assert.That(m_catalog.Find(colorway.Id), Is.EqualTo(colorway));
            }

            Assert.That(m_catalog.Find(""), Is.EqualTo(m_catalog.Default));
            Assert.That(m_catalog.Find("NoSuchColorway"), Is.EqualTo(m_catalog.Default));
        }

        [Test]
        public void TextIsReadableInEveryColorway()
        {
            List<string> problems = new List<string>();
            foreach (ColorwayData colorway in m_catalog.Colorways)
            {
                CheckContrast(problems, colorway, "text on the glow", colorway.TextOnGlow, colorway.Glow);
                CheckContrast(problems, colorway, "text on the starlight", colorway.TextOnStarlight,
                    colorway.Starlight);
                CheckContrast(problems, colorway, "glow on the background", colorway.Glow, colorway.Background);
                CheckContrast(problems, colorway, "starlight on the background", colorway.Starlight,
                    colorway.Background);
                CheckContrast(problems, colorway, "white on the background", Color.white, colorway.Background);
                CheckContrast(problems, colorway, "descriptions on the background", s_descriptionColor,
                    colorway.Background);
                CheckContrast(problems, colorway, "labels on the background", s_labelColor, colorway.Background);
            }

            Assert.That(problems, Is.Empty);
        }

        private static IEnumerable<Color> GetColors(ColorwayData colorway)
        {
            yield return colorway.Background;
            yield return colorway.Glow;
            yield return colorway.GlowShadow;
            yield return colorway.TextOnGlow;
            yield return colorway.Starlight;
            yield return colorway.TextOnStarlight;
        }

        private static void CheckContrast(List<string> problems, ColorwayData colorway, string what, Color text,
            Color below)
        {
            float textLuminance = ColorwayColor.GetLuminance(text);
            float belowLuminance = ColorwayColor.GetLuminance(below);
            float contrast = (Mathf.Max(textLuminance, belowLuminance) + 0.05f) /
                (Mathf.Min(textLuminance, belowLuminance) + 0.05f);
            if (contrast < k_MinTextContrast)
            {
                problems.Add($"{colorway.name}: {what} has a contrast of {contrast:0.00}:1");
            }
        }
    }
}
