using System.Collections.Generic;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// All languages of the game. The first is the default one, for phones set to a language the game does not have,
    /// and its texts stand in for any text another language lacks.
    /// </summary>
    [CreateAssetMenu(fileName = "LanguageCatalog", menuName = "Space Escaper/Language Catalog")]
    public class LanguageCatalog : ScriptableObject
    {
        [SerializeField] private List<LanguageData> m_languages;

        public IReadOnlyList<LanguageData> Languages => m_languages;
        public LanguageData Default => m_languages[0];

        /// <summary>
        /// The language with an ID. For an ID no language has, like the empty one before a language is chosen, the
        /// language the system is set to if the game has it, otherwise the default one.
        /// </summary>
        public LanguageData Find(string id, SystemLanguage systemLanguage)
        {
            foreach (LanguageData language in m_languages)
            {
                if (language.Id == id)
                {
                    return language;
                }
            }

            foreach (LanguageData language in m_languages)
            {
                if (language.SystemLanguage == systemLanguage)
                {
                    return language;
                }
            }

            return Default;
        }
    }
}
