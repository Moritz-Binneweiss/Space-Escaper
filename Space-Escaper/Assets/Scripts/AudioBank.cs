using UnityEngine;

/// <summary>
/// One complete set of sounds for a single audio style.
///
/// The game ships two of these: one holding the original 2020 sounds ("Classic")
/// and one holding the new ones. Switching style at runtime means swapping which
/// bank the <see cref="AudioSystem"/> reads from - no code change, no extra
/// fields per sound.
///
/// Adding a new sound to the game means adding ONE field here. Both banks then
/// offer that slot automatically. A slot that is still empty in the active bank
/// falls back to the other bank, so a sound that only exists in one style still
/// plays instead of going silent.
/// </summary>
[CreateAssetMenu(fileName = "AudioBank", menuName = "Space Escaper/Audio Bank")]
public class AudioBank : ScriptableObject
{
    [Tooltip("Only shown in the inspector, to tell the banks apart.")]
    public string styleName = "Classic";

    [Header("Music")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;

    [Header("Sound Effects")]
    public AudioClip button;
    public AudioClip coin;
    public AudioClip explosion;
    public AudioClip engine;
    public AudioClip selectShip;
    public AudioClip deselectShip;
    public AudioClip buyShip;
}
