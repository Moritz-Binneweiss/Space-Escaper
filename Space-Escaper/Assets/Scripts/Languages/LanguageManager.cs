using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Knows the language the UI shows and tells the UI when another one is chosen. Until a language is chosen, it is
    /// the one the phone is set to, if the game has it, otherwise the default one. It wakes before the rest of the
    /// scene, so every text finds its language when it comes on.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class LanguageManager : MonoBehaviour
    {
        [SerializeField] private LanguageCatalog m_catalog;

        // Read when the language changes, so a reload of the scene keeps them.
        private static LanguageTexts s_texts;

        /// <summary>
        /// The language the UI shows, from the save or the system as the scene wakes.
        /// </summary>
        public static LanguageData Current { get; private set; }

        public IReadOnlyList<LanguageData> Languages => m_catalog.Languages;

        /// <summary>
        /// Raised when another language is chosen, once <see cref="Current"/> is that one.
        /// </summary>
        public static event Action LanguageChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            s_texts = null;
            Current = null;
            LanguageChanged = null;
        }

        private void Awake()
        {
            Show(m_catalog, m_catalog.Find(SaveSystem.Data.LanguageId, Application.systemLanguage));
        }

        /// <summary>
        /// The text under a key in the language the UI shows, or in the default language if this one lacks it. A key
        /// of no text gives the key itself, so a missing text shows where it is missing, and so does every key before
        /// the scene wakes.
        /// </summary>
        public static string Get(string key)
        {
            return s_texts != null ? s_texts.Get(key) : key;
        }

        /// <summary>
        /// Shows a language everywhere and keeps it in the save.
        /// </summary>
        public void Choose(LanguageData language)
        {
            SaveSystem.Data.LanguageId = language.Id;
            SaveSystem.Save();
            Show(m_catalog, language);
        }

        /// <summary>
        /// Shows a language of a catalog, for tests also without a scene. The texts the language lacks come from the
        /// default of the catalog.
        /// </summary>
        internal static void Show(LanguageCatalog catalog, LanguageData language)
        {
            if (language == Current)
            {
                return;
            }

            LanguageTexts defaultTexts = new LanguageTexts(catalog.Default.Texts.text);
            s_texts = language == catalog.Default
                ? defaultTexts
                : new LanguageTexts(language.Texts.text, defaultTexts);
            Current = language;
            OnLanguageChanged();
        }

        /// <summary>
        /// Reads the texts of a language again and shows them, for tests that need its file as it is now. Until the
        /// next compile the editor keeps the texts of a language it has shown, also after a change to the file.
        /// </summary>
        internal static void Reload(LanguageCatalog catalog, LanguageData language)
        {
            Current = null;
            Show(catalog, language);
        }

        private static void OnLanguageChanged()
        {
            LanguageChanged?.Invoke();
        }
    }
}
