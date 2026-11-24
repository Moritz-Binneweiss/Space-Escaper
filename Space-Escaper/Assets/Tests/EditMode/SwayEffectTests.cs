using NUnit.Framework;

namespace SpaceEscaper.Tests
{
    // The sway of the swipe hint as the keyframes of the mockup set it: on the left at 0 % and 100 %, on the right at
    // 50 %, easing in and out on each way, so it passes the middle at a quarter and three quarters of the period.
    public class SwayEffectTests
    {
        private const float k_Period = 1.2f;
        private const float k_Distance = 5f;
        private const float k_Tolerance = 0.001f;

        [TestCase(0f, -k_Distance)]
        [TestCase(0.25f, 0f)]
        [TestCase(0.5f, k_Distance)]
        [TestCase(0.75f, 0f)]
        [TestCase(1f, -k_Distance)]
        [TestCase(2.5f, k_Distance)]
        public void TheSwayFollowsTheKeyframesOfTheMockup(float periods, float offset)
        {
            Assert.That(SwayEffect.GetOffset(periods * k_Period, k_Period, k_Distance),
                Is.EqualTo(offset).Within(k_Tolerance));
        }

        // Halfway between a side and the middle in time it has not come halfway yet, as it eases out of the side.
        [Test]
        public void TheSwayEasesOutOfEachSide()
        {
            float offset = SwayEffect.GetOffset(0.125f * k_Period, k_Period, k_Distance);
            Assert.That(offset, Is.LessThan(-k_Distance / 2f));
        }
    }
}
