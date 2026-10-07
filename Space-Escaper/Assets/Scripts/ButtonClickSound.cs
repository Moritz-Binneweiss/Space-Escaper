using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// Plays the UI click sound when this button is pressed.
    /// </summary>
    /// <remarks>
    /// Not a persistent OnClick call into AudioSystem: those break, because
    /// AudioSystem lives on via DontDestroyOnLoad while the scene copy the calls
    /// point at is destroyed on every reload. Going through AudioSystem.Instance
    /// always reaches the live one.
    /// </remarks>
    [RequireComponent(typeof(Button))]
    public class ButtonClickSound : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(PlayClick);
        }

        private static void PlayClick()
        {
            if (AudioSystem.Instance != null)
            {
                AudioSystem.Instance.PlayButtonClick();
            }
        }
    }
}
