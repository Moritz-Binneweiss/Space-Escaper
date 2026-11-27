using System;

namespace SpaceEscaper
{
    /// <summary>
    /// Fills up to two numbers into a format with the separators of a language: {0} is the first number rounded to a
    /// whole one, {1} the second, {1:00} has at least two digits, {0:0.0} one decimal. Whole parts from 1000 on group
    /// their thousands. TextMesh Pro's SetText knows neither the separators of a language nor groups.
    /// </summary>
    public static class NumberFormat
    {
        private const int k_MinBufferLength = 32;
        private const int k_MaxDecimals = 6;
        private const long k_SmallestGroupedNumber = 1000;

        /// <summary>
        /// Writes the format with its numbers into the buffer, which grows when it is too short, and returns the
        /// length of the text. A buffer that is long enough takes it without a new string.
        /// </summary>
        public static int Format(string format, float first, float second, char groupSeparator, char decimalSeparator,
            ref char[] buffer)
        {
            int length = 0;
            for (int i = 0; i < format.Length; i++)
            {
                int end = format[i] == '{' ? format.IndexOf('}', i) : -1;
                if (end < 0)
                {
                    Append(format[i], ref buffer, ref length);
                    continue;
                }

                int minDigits = 1;
                int decimals = 0;
                int colon = format.IndexOf(':', i, end - i);
                if (colon >= 0)
                {
                    int point = format.IndexOf('.', colon, end - colon);
                    minDigits = (point >= 0 ? point : end) - colon - 1;
                    decimals = point >= 0 ? end - point - 1 : 0;
                }

                float number = format[i + 1] == '1' ? second : first;
                AppendNumber(number, minDigits, decimals, groupSeparator, decimalSeparator, ref buffer, ref length);
                i = end;
            }

            return length;
        }

        /// <summary>
        /// The same text as a new string, for tests and the editor.
        /// </summary>
        public static string Format(string format, float first, float second, char groupSeparator,
            char decimalSeparator)
        {
            char[] buffer = null;
            int length = Format(format, first, second, groupSeparator, decimalSeparator, ref buffer);
            return new string(buffer ?? new char[0], 0, length);
        }

        private static void AppendNumber(float number, int minDigits, int decimals, char groupSeparator,
            char decimalSeparator, ref char[] buffer, ref int length)
        {
            if (float.IsNaN(number) || float.IsInfinity(number))
            {
                number = 0f;
            }

            decimals = Math.Min(decimals, k_MaxDecimals);
            long scale = 1;
            for (int i = 0; i < decimals; i++)
            {
                scale *= 10;
            }

            // In decimal like TextMesh Pro, as the float 1.4 is 1.39999998.
            long scaled = (long)Math.Round((decimal)Math.Abs(number) * scale, MidpointRounding.AwayFromZero);
            if (number < 0f && scaled != 0)
            {
                Append('-', ref buffer, ref length);
            }

            long whole = scaled / scale;
            int digits = Math.Max(minDigits, CountDigits(whole));
            for (int i = digits - 1; i >= 0; i--)
            {
                Append(GetDigit(whole, i), ref buffer, ref length);
                if (i > 0 && i % 3 == 0 && whole >= k_SmallestGroupedNumber)
                {
                    Append(groupSeparator, ref buffer, ref length);
                }
            }

            if (decimals > 0)
            {
                Append(decimalSeparator, ref buffer, ref length);
                for (int i = decimals - 1; i >= 0; i--)
                {
                    Append(GetDigit(scaled % scale, i), ref buffer, ref length);
                }
            }
        }

        private static int CountDigits(long number)
        {
            int digits = 1;
            for (; number >= 10; number /= 10)
            {
                digits++;
            }

            return digits;
        }

        private static char GetDigit(long number, int position)
        {
            for (int i = 0; i < position; i++)
            {
                number /= 10;
            }

            return (char)('0' + number % 10);
        }

        private static void Append(char character, ref char[] buffer, ref int length)
        {
            if (buffer == null || length == buffer.Length)
            {
                Array.Resize(ref buffer, Math.Max(k_MinBufferLength, length * 2));
            }

            buffer[length] = character;
            length++;
        }
    }
}
