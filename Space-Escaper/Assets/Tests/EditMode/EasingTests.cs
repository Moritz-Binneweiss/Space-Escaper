using System;
using NUnit.Framework;

namespace SpaceEscaper.Tests
{
    // The curves of the mockups against the values a browser shows for them, found here by halving the interval on
    // the curve with exact fractions. The code finds them another way, mostly with Newton's method.
    public class EasingTests
    {
        private const float k_Tolerance = 0.0001f;
        private const int k_Steps = 100;

        private static readonly float[] s_times = { 0.1f, 0.25f, 0.5f, 0.75f, 0.9f };

        [Test]
        public void EaseInIsTheCurveOfTheBrowser()
        {
            AssertCurve(Easing.EaseIn, 0.017027f, 0.093465f, 0.315357f, 0.621862f, 0.839428f);
        }

        [Test]
        public void EaseOutIsTheCurveOfTheBrowser()
        {
            AssertCurve(Easing.EaseOut, 0.160572f, 0.378138f, 0.684643f, 0.906535f, 0.982973f);
        }

        [Test]
        public void EaseInOutIsTheCurveOfTheBrowser()
        {
            AssertCurve(Easing.EaseInOut, 0.019722f, 0.129162f, 0.5f, 0.870838f, 0.980278f);
        }

        // A motion starts and ends where it should, also with a time a frame took it past either end, and never goes
        // back on its way.
        [TestCase(nameof(Easing.EaseIn))]
        [TestCase(nameof(Easing.EaseOut))]
        [TestCase(nameof(Easing.EaseInOut))]
        public void EveryCurveRunsFromZeroToOneAndOnlyForward(string name)
        {
            Func<float, float> curve = GetCurve(name);
            Assert.That(curve(-0.5f), Is.EqualTo(0f), "Before the start");
            Assert.That(curve(0f), Is.EqualTo(0f), "At the start");
            Assert.That(curve(1f), Is.EqualTo(1f), "At the end");
            Assert.That(curve(1.5f), Is.EqualTo(1f), "After the end");

            float last = 0f;
            for (int i = 1; i <= k_Steps; i++)
            {
                float way = curve((float)i / k_Steps);
                Assert.That(way, Is.GreaterThanOrEqualTo(last), $"Back on its way at {(float)i / k_Steps}");
                last = way;
            }
        }

        [Test]
        public void ALinearCurveKeepsTheTime()
        {
            foreach (float time in s_times)
            {
                Assert.That(Easing.CubicBezier(0f, 0f, 1f, 1f, time), Is.EqualTo(time).Within(k_Tolerance));
            }
        }

        private static void AssertCurve(Func<float, float> curve, params float[] expected)
        {
            for (int i = 0; i < s_times.Length; i++)
            {
                Assert.That(curve(s_times[i]), Is.EqualTo(expected[i]).Within(k_Tolerance), $"At {s_times[i]}");
            }
        }

        private static Func<float, float> GetCurve(string name)
        {
            switch (name)
            {
                case nameof(Easing.EaseIn):
                    return Easing.EaseIn;

                case nameof(Easing.EaseOut):
                    return Easing.EaseOut;

                default:
                    return Easing.EaseInOut;
            }
        }
    }
}
