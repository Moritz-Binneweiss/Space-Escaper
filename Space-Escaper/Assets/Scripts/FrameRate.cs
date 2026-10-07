using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Sets the frame rate the game renders at, once at startup.
    /// </summary>
    public static class FrameRate
    {
        // Android renders at 30 fps unless a target is set, and ignores vSync. A
        // target that does not divide the display refresh rate gets rounded, so a
        // 90 Hz display ends up at 45 fps.
        private const int k_TargetFrameRate = 60;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ApplyTargetFrameRate()
        {
            Application.targetFrameRate = k_TargetFrameRate;
        }
    }
}
