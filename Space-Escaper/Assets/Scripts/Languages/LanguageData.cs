using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// One language of the game: the ID the save keeps, the language of a phone that starts in it and its texts. One
    /// asset per language, all of them listed in the <see cref="LanguageCatalog"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "LanguageData", menuName = "Space Escaper/Language Data")]
    public class LanguageData : ScriptableObject
    {
        [Tooltip("The save stores the chosen language under this ID, its language tag. Changing it loses the choice " +
            "in existing saves.")]
        [SerializeField] private string m_id;
        [Tooltip("Until a language is chosen, the game shows this one on a phone set to this language.")]
        [SerializeField] private SystemLanguage m_systemLanguage;
        [Tooltip("The texts of the language under their keys, a flat JSON object of strings.")]
        [SerializeField] private TextAsset m_texts;

        public string Id => m_id;
        public SystemLanguage SystemLanguage => m_systemLanguage;
        public TextAsset Texts => m_texts;
    }
}
