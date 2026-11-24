using NUnit.Framework;

namespace SpaceEscaper.Tests
{
    // A touch becomes a drag after the touch slop of Android, 8 of its pixels at 160 to the inch, and never before
    // Unity's default of 10 pixels, as for the mouse in the editor.
    public class DragThresholdTests
    {
        [TestCase(420f, 21)]
        [TestCase(560f, 28)]
        [TestCase(160f, 10)]
        [TestCase(96f, 10)]
        public void ATouchBecomesADragAfterTheTouchSlopOfAndroid(float dpi, int pixels)
        {
            Assert.That(DragThreshold.GetPixels(dpi), Is.EqualTo(pixels));
        }

        // Some screens tell no dpi, they count as a typical phone.
        [Test]
        public void AScreenWithoutDpiCountsAsATypicalPhone()
        {
            Assert.That(DragThreshold.GetPixels(0f), Is.EqualTo(DragThreshold.GetPixels(420f)));
        }
    }
}
