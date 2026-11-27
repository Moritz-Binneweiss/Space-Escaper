using TMPro;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Shows a text of the UI in the language the UI shows, when it comes on and when another language is chosen. A
    /// text with placeholders like {0} shows the numbers a script gives it. The scene holds the text of the default
    /// language.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        private const string k_GroupSeparatorKey = "number.groupSeparator";
        private const string k_DecimalSeparatorKey = "number.decimalSeparator";

        [Tooltip("The key of the text in the files of the languages.")]
        [SerializeField] private string m_key;

        private TMP_Text m_text;
        private float m_firstNumber;
        private float m_secondNumber;
        private char[] m_buffer;

        public string Key => m_key;

        /// <summary>
        /// The text it shows in, for scripts that color it.
        /// </summary>
        public TMP_Text Text
        {
            get
            {
                // A screen can set its texts as it comes on, before their Awake.
                if (m_text == null)
                {
                    m_text = GetComponent<TMP_Text>();
                }

                return m_text;
            }
        }

        private void OnEnable()
        {
            LanguageManager.LanguageChanged += LanguageManager_LanguageChanged;
            ShowText();
        }

        private void OnDisable()
        {
            LanguageManager.LanguageChanged -= LanguageManager_LanguageChanged;
        }

        /// <summary>
        /// Shows the text of another key.
        /// </summary>
        public void Show(string key)
        {
            Show(key, 0f, 0f);
        }

        /// <summary>
        /// Shows the text of another key with numbers in its placeholders, {0} the first and {1} the second, like
        /// {0}:{1:00} for a time.
        /// </summary>
        public void Show(string key, float firstNumber, float secondNumber = 0f)
        {
            m_key = key;
            ShowNumbers(firstNumber, secondNumber);
        }

        /// <summary>
        /// Shows its text with other numbers in its placeholders.
        /// </summary>
        public void ShowNumbers(float firstNumber, float secondNumber = 0f)
        {
            m_firstNumber = firstNumber;
            m_secondNumber = secondNumber;
            if (isActiveAndEnabled)
            {
                ShowText();
            }
        }

        private void LanguageManager_LanguageChanged()
        {
            ShowText();
        }

        private void ShowText()
        {
            if (LanguageManager.Current == null || string.IsNullOrEmpty(m_key))
            {
                return;
            }

            string text = LanguageManager.Get(m_key);
            if (text.IndexOf('{') >= 0)
            {
                // TextMesh Pro takes the buffer as it is, without a new string. Its SetText knows neither the
                // separators of a language nor groups of thousands.
                int length = NumberFormat.Format(text, m_firstNumber, m_secondNumber,
                    GetSeparator(k_GroupSeparatorKey, ','), GetSeparator(k_DecimalSeparatorKey, '.'), ref m_buffer);
                Text.SetCharArray(m_buffer, 0, length);
            }
            else
            {
                // Unlike SetText, the property takes a text it already shows without building it again.
                Text.text = text;
            }
        }

        private static char GetSeparator(string key, char fallback)
        {
            string separator = LanguageManager.Get(key);
            return separator.Length == 1 ? separator[0] : fallback;
        }
    }
}
