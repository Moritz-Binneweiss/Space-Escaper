using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace SpaceEscaper.Tests
{
    public class ColorwayColorTests
    {
        private const float k_Tolerance = 0.0001f;

        // White at 7 % over the background, like the tiles and sections of the mockups.
        private const float k_TileOpacity = 0.07f;

        // Greyed out, like CSS opacity 0.4.
        private const float k_GreyedOutOpacity = 0.4f;

        // The sections of the settings lie between the background and what is greyed out on them.
        private const float k_SectionWhite = 0.07f;

        // Colorways of the test's own, so a change to the real ones does not change what it checks: the same glow over
        // the darkest background and over a brighter one.
        private static readonly Color s_darkBackground = new Color32(5, 6, 7, 255);
        private static readonly Color s_brightBackground = new Color32(21, 23, 28, 255);
        private static readonly Color s_glow = new Color32(87, 255, 92, 255);
        private static readonly Color s_starlight = new Color32(255, 173, 61, 255);

        private ColorwayData m_dark;
        private ColorwayData m_bright;

        [SetUp]
        public void CreateColorways()
        {
            m_dark = CreateColorway(s_darkBackground);
            m_bright = CreateColorway(s_brightBackground);
        }

        [TearDown]
        public void DestroyColorways()
        {
            Object.DestroyImmediate(m_dark);
            Object.DestroyImmediate(m_bright);
        }

        [Test]
        public void ARoleTakesItsColorFromTheColorway()
        {
            Assert.That(new ColorwayColor(ColorwayRole.Background).Resolve(m_dark), Is.EqualTo(s_darkBackground));
            Assert.That(new ColorwayColor(ColorwayRole.Background).Resolve(m_bright), Is.EqualTo(s_brightBackground));
            Assert.That(new ColorwayColor(ColorwayRole.Glow).Resolve(m_dark), Is.EqualTo(s_glow));
            Assert.That(new ColorwayColor(ColorwayRole.Starlight).Resolve(m_dark), Is.EqualTo(s_starlight));
        }

        [Test]
        public void AFixedColorIsTheSameInEveryColorway()
        {
            Color grey = new Color32(0xB7, 0xC0, 0xCF, 0xFF);
            ColorwayColor fixedColor = new ColorwayColor(ColorwayRole.Fixed, grey, 1f, false, false, 0f);

            Assert.That(fixedColor.Resolve(m_dark), Is.EqualTo(grey));
            Assert.That(fixedColor.Resolve(m_bright), Is.EqualTo(grey));
        }

        // Unity blends in linear light, a browser in sRGB. The alpha has to make the blend as bright as the browser's.
        [Test]
        public void SeeThroughLooksAsBrightAsInTheMockups()
        {
            Color white = Color.white;
            float alpha = ColorwayColor.GetLinearAlpha(white, s_darkBackground, k_TileOpacity);

            float browser = ColorwayColor.GetLuminance(Color.Lerp(s_darkBackground, white, k_TileOpacity));
            float unity = Mathf.Lerp(ColorwayColor.GetLuminance(s_darkBackground), ColorwayColor.GetLuminance(white),
                alpha);
            Assert.That(unity, Is.EqualTo(browser).Within(k_Tolerance));
            Assert.That(alpha, Is.LessThan(k_TileOpacity), "Linear light needs less alpha over a dark background");
            Assert.That(ColorwayColor.GetLinearAlpha(white, s_darkBackground, 0f), Is.EqualTo(0f));
            Assert.That(ColorwayColor.GetLinearAlpha(white, s_darkBackground, 1f), Is.EqualTo(1f).Within(k_Tolerance));
        }

        [Test]
        public void SeeThroughNeedsMoreAlphaOverABrighterBackground()
        {
            ColorwayColor tile = new ColorwayColor(ColorwayRole.Fixed, Color.white, k_TileOpacity, false, false, 0f);

            Color overDark = tile.Resolve(m_dark);
            Color overBright = tile.Resolve(m_bright);
            Assert.That(overDark.a, Is.EqualTo(ColorwayColor.GetLinearAlpha(Color.white, s_darkBackground,
                k_TileOpacity)).Within(k_Tolerance));
            Assert.That(overBright.a, Is.GreaterThan(overDark.a * 1.5f));
        }

        // The 3D world behind the menus and the run stays dark space in every colorway.
        [Test]
        public void OverTheWorldTheBackgroundOfTheColorwayDoesNotCount()
        {
            ColorwayColor chip = new ColorwayColor(ColorwayRole.Glow, Color.white, k_TileOpacity, false, true, 0f);

            Assert.That(chip.Resolve(m_bright).a, Is.EqualTo(chip.Resolve(m_dark).a).Within(k_Tolerance));
        }

        [Test]
        public void GreyedOutMixesWithWhatLiesBelowIntoAnOpaqueColor()
        {
            ColorwayColor greyedOut = new ColorwayColor(ColorwayRole.Glow, Color.white, k_GreyedOutOpacity, true, false,
                k_SectionWhite);

            foreach (ColorwayData colorway in new[] { m_dark, m_bright })
            {
                Color section = Color.Lerp(colorway.Background, Color.white, k_SectionWhite);
                Color expected = Color.Lerp(section, s_glow, k_GreyedOutOpacity);
                Color resolved = greyedOut.Resolve(colorway);
                Assert.That(resolved.r, Is.EqualTo(expected.r).Within(k_Tolerance));
                Assert.That(resolved.g, Is.EqualTo(expected.g).Within(k_Tolerance));
                Assert.That(resolved.b, Is.EqualTo(expected.b).Within(k_Tolerance));
                Assert.That(resolved.a, Is.EqualTo(1f));
            }

            Assert.That(greyedOut.Resolve(m_bright), Is.Not.EqualTo(greyedOut.Resolve(m_dark)));
        }

        private static ColorwayData CreateColorway(Color background)
        {
            ColorwayData colorway = ScriptableObject.CreateInstance<ColorwayData>();
            SerializedObject serialized = new SerializedObject(colorway);
            serialized.FindProperty("m_background").colorValue = background;
            serialized.FindProperty("m_glow").colorValue = s_glow;
            serialized.FindProperty("m_starlight").colorValue = s_starlight;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return colorway;
        }
    }
}
