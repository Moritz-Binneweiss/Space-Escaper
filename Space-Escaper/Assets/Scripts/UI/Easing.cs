using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// The timing curves of the mockups, the cubic Bézier curves that CSS calls ease-in, ease-out and ease-in-out.
    /// Each takes the share of the time gone by and gives the share of the way, both from 0 to 1.
    /// </summary>
    public static class Easing
    {
        private const int k_NewtonSteps = 8;
        private const int k_HalvingSteps = 32;
        private const float k_Precision = 1e-6f;

        public static float EaseIn(float time)
        {
            return CubicBezier(0.42f, 0f, 1f, 1f, time);
        }

        public static float EaseOut(float time)
        {
            return CubicBezier(0f, 0f, 0.58f, 1f, time);
        }

        public static float EaseInOut(float time)
        {
            return CubicBezier(0.42f, 0f, 0.58f, 1f, time);
        }

        /// <summary>
        /// The curve from (0, 0) to (1, 1) with the control points (x1, y1) and (x2, y2), like cubic-bezier() in CSS.
        /// </summary>
        internal static float CubicBezier(float x1, float y1, float x2, float y2, float time)
        {
            if (time <= 0f)
            {
                return 0f;
            }

            if (time >= 1f)
            {
                return 1f;
            }

            return Sample(y1, y2, FindParameter(x1, x2, time));
        }

        // The curve gives x and y for a parameter, not y for x. Newton's method finds the parameter of the time in a
        // few steps, as browsers do, and halving the interval takes over where the curve is too flat for it.
        private static float FindParameter(float x1, float x2, float time)
        {
            float parameter = time;
            for (int i = 0; i < k_NewtonSteps; i++)
            {
                float error = Sample(x1, x2, parameter) - time;
                if (Mathf.Abs(error) < k_Precision)
                {
                    return parameter;
                }

                float slope = GetSlope(x1, x2, parameter);
                if (Mathf.Abs(slope) < k_Precision)
                {
                    break;
                }

                parameter -= error / slope;
            }

            float low = 0f;
            float high = 1f;
            parameter = time;
            for (int i = 0; i < k_HalvingSteps; i++)
            {
                float x = Sample(x1, x2, parameter);
                if (Mathf.Abs(x - time) < k_Precision)
                {
                    break;
                }

                if (x < time)
                {
                    low = parameter;
                }
                else
                {
                    high = parameter;
                }

                parameter = (low + high) * 0.5f;
            }

            return parameter;
        }

        // One coordinate of a curve whose ends lie at 0 and 1: 3(1 - t)²t p1 + 3(1 - t)t² p2 + t³.
        private static float Sample(float p1, float p2, float parameter)
        {
            float rest = 1f - parameter;
            return 3f * rest * rest * parameter * p1 + 3f * rest * parameter * parameter * p2
                + parameter * parameter * parameter;
        }

        private static float GetSlope(float p1, float p2, float parameter)
        {
            float rest = 1f - parameter;
            return 3f * rest * rest * p1 + 6f * rest * parameter * (p2 - p1) + 3f * parameter * parameter * (1f - p2);
        }
    }
}
