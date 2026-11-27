using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace SpaceEscaper.Tests
{
    /// <summary>
    /// Reads a text with numbers in its placeholders as a language writes them, like 1,938 m in English and 1.938 m in
    /// German.
    /// </summary>
    internal static class TextWithNumbers
    {
        /// <summary>
        /// A placeholder of a text, like {0}, {1:00} or {0:0.0}.
        /// </summary>
        public static readonly Regex Placeholder = new Regex(@"\{[^}]*\}");

        /// <summary>
        /// Matches what a text shows with numbers in its placeholders, their whole part grouped in thousands. A text
        /// without placeholders matches only itself.
        /// </summary>
        public static Regex GetPattern(string text, char groupSeparator, char decimalSeparator)
        {
            string number = @"([-+]?\d{1,3}(?:" + Regex.Escape(groupSeparator.ToString()) + @"\d{3})*(?:" +
                Regex.Escape(decimalSeparator.ToString()) + @"\d+)?)";
            return new Regex("^" + string.Join(number, Placeholder.Split(text).Select(Regex.Escape)) + "$");
        }

        /// <summary>
        /// Reads the numbers a text shows in its placeholders, the first for {0} and the second for {1}. Returns false
        /// if it shows something else.
        /// </summary>
        public static bool TryRead(string shown, string text, char groupSeparator, char decimalSeparator,
            out float first, out float second)
        {
            first = 0f;
            second = 0f;
            Match match = GetPattern(text, groupSeparator, decimalSeparator).Match(shown);
            if (!match.Success)
            {
                return false;
            }

            MatchCollection placeholders = Placeholder.Matches(text);
            for (int i = 0; i < placeholders.Count; i++)
            {
                string digits = match.Groups[i + 1].Value.Replace(groupSeparator.ToString(), string.Empty)
                    .Replace(decimalSeparator, '.');
                float number = float.Parse(digits, CultureInfo.InvariantCulture);
                if (placeholders[i].Value[1] == '1')
                {
                    second = number;
                }
                else
                {
                    first = number;
                }
            }

            return true;
        }
    }
}
