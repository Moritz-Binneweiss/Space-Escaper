using TMPro;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Shows a text of the UI in the language the UI shows, when it comes on and when another language is chosen. The
    /// scene holds the text of the default language.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        [Tooltip("The key of the text in the files of the languages.")]
        [SerializeField] private string m_key;

        private TMP_Text m_text;

        /// <summary>
        /// The key of the text. Another key shows its text right away.
        /// </summary>
        public string Key
        {
            get => m_key;
            set
            {
                m_key = value;
                if (isActiveAndEnabled)
                {
                    Show();
                }
            }
        }

        private void Awake()
        {
            m_text = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;
            Show();
        }

        private void OnDisable()
        {
            LanguageManager.LanguageChanged -= LanguageManager_LanguageChanged;
        }

        private void LanguageManager_LanguageChanged()
        {
            Show();
        }

        private void Show()
        {
            if (LanguageManager.Current == null)
            {
                return;
            }

            m_text.SetText(LanguageManager.Get(m_key));
        }
    }
}
