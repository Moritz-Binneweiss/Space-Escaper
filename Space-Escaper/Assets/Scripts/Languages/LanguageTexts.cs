using System.Collections.Generic;

namespace SpaceEscaper
{
    /// <summary>
    /// The texts of a language under their keys, with the texts of another language behind them for keys it lacks.
    /// </summary>
    public class LanguageTexts
    {
        private readonly Dictionary<string, string> m_texts;
        private readonly LanguageTexts m_fallback;

        /// <param name="json">A flat JSON object of strings, like {"settings.title": "SETTINGS"}.</param>
        /// <param name="fallback">The texts for keys this language lacks, or null.</param>
        public LanguageTexts(string json, LanguageTexts fallback = null)
        {
            m_texts = FlatJson.Parse(json);
            m_fallback = fallback;
        }

        /// <summary>
        /// The keys of this language, without those of the fallback.
        /// </summary>
        public IReadOnlyCollection<string> Keys => m_texts.Keys;

        public bool Contains(string key)
        {
            return key != null && m_texts.ContainsKey(key);
        }

        /// <summary>
        /// The text under a key, from the fallback if this language has none. A key that neither has gives the key
        /// itself, so a missing text shows where it is missing.
        /// </summary>
        public string Get(string key)
        {
            if (key != null && m_texts.TryGetValue(key, out string text))
            {
                return text;
            }

            return m_fallback != null ? m_fallback.Get(key) : key;
        }
    }
}
