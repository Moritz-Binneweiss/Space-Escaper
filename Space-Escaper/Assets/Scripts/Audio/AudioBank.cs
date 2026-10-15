using UnityEngine;

namespace SpaceEscaper
{
    /// <summary>
    /// One complete set of sounds for a single audio style.
    /// </summary>
    /// <remarks>
    /// The game ships two of these: one holding the original 2020 sounds ("Classic")
    /// and one holding the new ones. Switching style at runtime means swapping which
    /// bank the <see cref="AudioSystem"/> reads from - no code change, no extra
    /// fields per sound.
    ///
    /// Adding a new sound to the game means adding one field and its property here.
    /// Both banks then offer that slot automatically. A slot that is still empty in
    /// the active bank falls back to the other bank, so a sound that only exists in
    /// one style still plays instead of going silent.
    /// </remarks>
    [CreateAssetMenu(fileName = "AudioBank", menuName = "Space Escaper/Audio Bank")]
    public class AudioBank : ScriptableObject
    {
        [Tooltip("Only shown in the inspector, to tell the banks apart.")]
        [SerializeField] private string m_styleName = "Classic";

        [Header("Music")]
        [SerializeField] private AudioClip m_menuMusic;
        [SerializeField] private AudioClip m_gameMusic;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip m_buttonClick;
        [SerializeField] private AudioClip m_coinPickup;
        [SerializeField] private AudioClip m_explosion;
        [SerializeField] private AudioClip m_engine;
        [SerializeField] private AudioClip m_shipSelect;
        [SerializeField] private AudioClip m_shipDeselect;
        [SerializeField] private AudioClip m_shipPurchase;

        public string StyleName => m_styleName;
        public AudioClip MenuMusic => m_menuMusic;
        public AudioClip GameMusic => m_gameMusic;
        public AudioClip ButtonClick => m_buttonClick;
        public AudioClip CoinPickup => m_coinPickup;
        public AudioClip Explosion => m_explosion;
        public AudioClip Engine => m_engine;
        public AudioClip ShipSelect => m_shipSelect;
        public AudioClip ShipDeselect => m_shipDeselect;
        public AudioClip ShipPurchase => m_shipPurchase;
    }
}
