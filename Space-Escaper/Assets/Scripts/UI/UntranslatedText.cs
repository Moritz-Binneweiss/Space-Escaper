using TMPro;
using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// Marks a text of the UI that stays the same in every language, like a name, the version or a license text. It
    /// has no key in the files of the languages.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class UntranslatedText : MonoBehaviour
    {
    }
}
