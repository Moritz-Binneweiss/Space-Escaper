using NUnit.Framework;

namespace SpaceEscaper.Tests
{
    // Two screens fading into each other: the one above with the curve, the one below with what the one above leaves
    // of it, also through the ground of the one above. So nothing of the one below lingers at the end, and what lies
    // behind two nearly opaque grounds does not shine through halfway.
    public class ScreenFadeTests
    {
        // The ground of the settings, the colorways and the credits, as Unity blends it.
        private const float k_Ground = 0.982f;
        private const float k_Tolerance = 0.0001f;
        private const int k_Steps = 100;

        [TestCase(0f)]
        [TestCase(0.25f)]
        [TestCase(0.5f)]
        [TestCase(0.75f)]
        [TestCase(1f)]
        public void TheScreenBelowShowsWhatTheOneAboveDoesNot(float alphaAbove)
        {
            float alphaBelow = ScreenManager.GetAlphaBelow(alphaAbove, k_Ground);
            Assert.That(alphaBelow * (1f - k_Ground * alphaAbove), Is.EqualTo(1f - alphaAbove).Within(k_Tolerance));
        }

        [Test]
        public void WhatLiesBehindTwoGroundsDoesNotShineThrough()
        {
            for (int i = 0; i <= k_Steps; i++)
            {
                float alphaAbove = (float)i / k_Steps;
                float alphaBelow = ScreenManager.GetAlphaBelow(alphaAbove, k_Ground);
                float through = (1f - k_Ground * alphaBelow) * (1f - k_Ground * alphaAbove);
                Assert.That(through, Is.LessThanOrEqualTo(1f - k_Ground + k_Tolerance), $"At {alphaAbove:0.00} above");
            }
        }

        // A screen over the world has no ground, so the two simply crossfade.
        [Test]
        public void WithoutAGroundTheScreensCrossfade()
        {
            for (int i = 0; i <= k_Steps; i++)
            {
                float alphaAbove = (float)i / k_Steps;
                Assert.That(ScreenManager.GetAlphaBelow(alphaAbove, 0f),
                    Is.EqualTo(1f - alphaAbove).Within(k_Tolerance));
            }
        }
    }
}
