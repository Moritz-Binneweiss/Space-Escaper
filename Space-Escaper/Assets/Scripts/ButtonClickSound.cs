using UnityEngine;
using UnityEngine.UI;

// Plays the UI click sound when this button is pressed.
//
// Clicks used to be persistent OnClick calls straight into AudioSystem. Those
// break: AudioSystem lives on via DontDestroyOnLoad, while the scene copy the
// calls point at is destroyed on every reload. Going through
// AudioSystem.Instance always reaches the live one.
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
            AudioSystem.Instance.PlayButton();
    }
}
