using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SpaceEscaper
{
    /// <summary>
    /// Reads a flat JSON object of strings, the format of the files of the languages. Unity's JsonUtility reads no
    /// dictionaries, and a package for JSON would come into every build.
    /// </summary>
    public static class FlatJson
    {
        private const int k_UnicodeEscapeLength = 4;

        /// <summary>
        /// The strings of a JSON object under their keys, like {"settings.title": "SETTINGS"}. Anything else throws a
        /// <see cref="FormatException"/> that names the line: a value that is no string, a key that comes twice or
        /// text after the object.
        /// </summary>
        public static Dictionary<string, string> Parse(string json)
        {
            return new Parser(json ?? "").ParseObject();
        }

        private class Parser
        {
            private readonly string m_json;
            private readonly StringBuilder m_builder = new StringBuilder();
            private int m_index;

            public Parser(string json)
            {
                m_json = json;
            }

            public Dictionary<string, string> ParseObject()
            {
                Dictionary<string, string> strings = new Dictionary<string, string>();
                SkipWhitespace();
                Expect('{');
                SkipWhitespace();
                if (!TrySkip('}'))
                {
                    do
                    {
                        SkipWhitespace();
                        int keyIndex = m_index;
                        string key = ReadString();
                        SkipWhitespace();
                        Expect(':');
                        SkipWhitespace();
                        string value = ReadString();
                        if (strings.ContainsKey(key))
                        {
                            throw CreateException(keyIndex, $"the key \"{key}\" comes twice");
                        }

                        strings.Add(key, value);
                        SkipWhitespace();
                    }
                    while (TrySkip(','));

                    Expect('}');
                }

                SkipWhitespace();
                if (m_index < m_json.Length)
                {
                    throw CreateException(m_index, "text after the object");
                }

                return strings;
            }

            // A byte order mark counts as whitespace, some editors write one at the start.
            private void SkipWhitespace()
            {
                while (m_index < m_json.Length && (char.IsWhiteSpace(m_json[m_index]) || m_json[m_index] == '\uFEFF'))
                {
                    m_index++;
                }
            }

            private bool TrySkip(char expected)
            {
                if (m_index < m_json.Length && m_json[m_index] == expected)
                {
                    m_index++;
                    return true;
                }

                return false;
            }

            private void Expect(char expected)
            {
                if (!TrySkip(expected))
                {
                    throw CreateException(m_index, $"expected '{expected}'");
                }
            }

            private string ReadString()
            {
                int start = m_index;
                if (!TrySkip('"'))
                {
                    throw CreateException(m_index, "expected a string in double quotes");
                }

                m_builder.Clear();
                while (m_index < m_json.Length)
                {
                    char character = m_json[m_index++];
                    switch (character)
                    {
                        case '"':
                            return m_builder.ToString();
                        case '\\':
                            ReadEscape();
                            break;
                        default:
                            if (character < ' ')
                            {
                                throw CreateException(m_index - 1, "a line break or control character in a string, " +
                                    "write \\n for a new line");
                            }

                            m_builder.Append(character);
                            break;
                    }
                }

                throw CreateException(start, "a string that never ends");
            }

            private void ReadEscape()
            {
                int start = m_index - 1;
                char character = m_index < m_json.Length ? m_json[m_index++] : '\0';
                switch (character)
                {
                    case '"':
                    case '\\':
                    case '/':
                        m_builder.Append(character);
                        break;
                    case 'b':
                        m_builder.Append('\b');
                        break;
                    case 'f':
                        m_builder.Append('\f');
                        break;
                    case 'n':
                        m_builder.Append('\n');
                        break;
                    case 'r':
                        m_builder.Append('\r');
                        break;
                    case 't':
                        m_builder.Append('\t');
                        break;
                    case 'u':
                        if (m_index + k_UnicodeEscapeLength > m_json.Length || !int.TryParse(
                            m_json.Substring(m_index, k_UnicodeEscapeLength), NumberStyles.AllowHexSpecifier,
                            CultureInfo.InvariantCulture, out int code))
                        {
                            throw CreateException(start, "\\u needs four hex digits");
                        }

                        m_builder.Append((char)code);
                        m_index += k_UnicodeEscapeLength;
                        break;
                    default:
                        throw CreateException(start, "an unknown escape in a string");
                }
            }

            private FormatException CreateException(int index, string problem)
            {
                int line = 1;
                int lineStart = 0;
                for (int i = 0; i < index && i < m_json.Length; i++)
                {
                    if (m_json[i] == '\n')
                    {
                        line++;
                        lineStart = i + 1;
                    }
                }

                return new FormatException($"Line {line}, column {index - lineStart + 1}: {problem}");
            }
        }
    }
}
