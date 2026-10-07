using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// Central audio controller. Survives scene reloads via DontDestroyOnLoad and
    /// plays everything through three sources: one-shot SFX, the looping engine,
    /// and music.
    /// </summary>
    /// <remarks>
    /// Individual clips are not referenced here - they come from an
    /// <see cref="AudioBank"/> per style, so switching between the classic 2020
    /// sounds and the new ones is simply a matter of reading from the other bank.
    /// Adding a sound to the game means adding one slot to AudioBank, not two
    /// fields here.
    /// </remarks>
    public class AudioSystem : MonoBehaviour
    {
        // Which track the game currently wants to hear. Remembered so a style
        // switch or an unmute can restart the right one.
        private enum Track
        {
            None,
            Menu,
            Game,
        }

        // How much quieter the very bottom of the slider is than the top. 40 dB
        // means every quarter of the travel roughly halves the perceived loudness.
        // Raise it for a slider that reaches "almost silent" sooner.
        private const float k_VolumeRangeDb = 40f;

        // Keeps a style switch from seeking to the very end of a shorter track.
        private const float k_TrackEndMargin = 0.05f;

        [Header("Sound Banks")]
        [SerializeField] private AudioBank m_classicBank;
        [SerializeField] private AudioBank m_newBank;

        [Header("Mixer Routing (optional)")]
        [SerializeField] private AudioMixerGroup m_musicGroup;
        [SerializeField] private AudioMixerGroup m_sfxGroup;

        [Header("UI - lives in the scene, re-adopted on every scene load")]
        [SerializeField] private Button m_sfxButton;
        [SerializeField] private Sprite m_sfxOnSprite;
        [SerializeField] private Sprite m_sfxOffSprite;
        [SerializeField] private Button m_musicButton;
        [SerializeField] private Sprite m_musicOnSprite;
        [SerializeField] private Sprite m_musicOffSprite;
        [SerializeField] private Toggle m_newSoundsToggle;
        [SerializeField] private Slider m_volumeSlider;

        private AudioSource m_sfxSource;
        private AudioSource m_engineSource;
        private AudioSource m_musicSource;

        // Stand-in used when a bank slot is empty, so clip lookups never need a
        // null check.
        private AudioBank m_emptyBank;

        private bool m_useNewSounds = true;
        private bool m_isMusicMuted;
        private bool m_isSfxMuted;
        private float m_masterVolume = 1f;
        private Track m_currentTrack;

        // Set while the death screen is up. The track is paused rather than
        // stopped, so a revive picks it up exactly where it was.
        private bool m_isMusicOnHold;

        public static AudioSystem Instance { get; private set; }

        private AudioBank EmptyBank
        {
            get
            {
                if (m_emptyBank == null)
                {
                    m_emptyBank = ScriptableObject.CreateInstance<AudioBank>();
                }

                return m_emptyBank;
            }
        }

        private AudioBank NewSounds => m_newBank != null ? m_newBank : EmptyBank;

        private AudioBank ClassicSounds => m_classicBank != null ? m_classicBank : EmptyBank;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // A reloaded scene brings its own copy of this object along. That
                // copy's inspector references point at the NEW scene's buttons,
                // while the surviving instance still points at the destroyed ones.
                // Hand them over before throwing the copy away - otherwise the mute
                // buttons and the style toggle stop working after the first reload.
                Instance.AdoptSceneReferencesFrom(this);
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            CreateSources();
            LoadPreferences();

            m_engineSource.mute = m_isSfxMuted;
            BindUi();

            SceneManager.sceneLoaded += SceneManager_SceneLoaded;
        }

        private void Start()
        {
            PlayMenuMusic();
        }

        private void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            SceneManager.sceneLoaded -= SceneManager_SceneLoaded;
            Instance = null;
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                SaveSystem.Save();
            }
        }

        private void OnApplicationQuit()
        {
            SaveSystem.Save();
        }

        /// <summary>
        /// Overall volume for everything the game plays, on top of the individual
        /// mute switches.
        /// </summary>
        /// <remarks>
        /// AudioListener.volume is global, so this covers any source, not only the
        /// three owned by this class.
        ///
        /// The slider position is NOT used as the amplitude directly. Hearing is
        /// logarithmic, so on a linear scale the bottom few percent swallow most of
        /// the audible change (0.1 -> 0.2 is 6 dB) while the top third is barely
        /// distinguishable (0.7 -> 1.0 is only 3 dB). Mapping the travel onto a
        /// fixed decibel range instead makes every step of the slider sound equally
        /// big. Note that squaring the value does NOT fix this - a power curve
        /// stretches the whole dB range evenly and leaves the imbalance intact.
        /// </remarks>
        public void SetMasterVolume(float sliderPosition)
        {
            m_masterVolume = Mathf.Clamp01(sliderPosition);
            AudioListener.volume = ConvertSliderToAmplitude(m_masterVolume);

            // Deliberately no SaveSystem.Save() here - a slider fires this on every
            // frame while being dragged. Saved in OnApplicationPause/Quit instead.
            SaveSystem.Data.MasterVolume = m_masterVolume;
        }

        public void PlayButtonClick() => PlaySfx(PickClip(NewSounds.ButtonClick, ClassicSounds.ButtonClick));

        public void PlayCoinPickup() => PlaySfx(PickClip(NewSounds.CoinPickup, ClassicSounds.CoinPickup));

        public void PlayExplosion() => PlaySfx(PickClip(NewSounds.Explosion, ClassicSounds.Explosion));

        public void PlayShipSelect() => PlaySfx(PickClip(NewSounds.ShipSelect, ClassicSounds.ShipSelect));

        public void PlayShipDeselect() => PlaySfx(PickClip(NewSounds.ShipDeselect, ClassicSounds.ShipDeselect));

        public void PlayShipPurchase() => PlaySfx(PickClip(NewSounds.ShipPurchase, ClassicSounds.ShipPurchase));

        public void StartEngine()
        {
            AudioClip clip = PickClip(NewSounds.Engine, ClassicSounds.Engine);
            if (clip == null)
            {
                return;
            }

            if (m_engineSource.clip != clip)
            {
                m_engineSource.Stop();
                m_engineSource.clip = clip;
            }

            m_engineSource.mute = m_isSfxMuted;
            if (!m_engineSource.isPlaying)
            {
                m_engineSource.Play();
            }
        }

        public void StopEngine()
        {
            if (m_engineSource.isPlaying)
            {
                m_engineSource.Stop();
            }
        }

        public void PlayMenuMusic()
        {
            m_isMusicOnHold = false;
            PlayTrack(Track.Menu);
        }

        public void PlayGameMusic()
        {
            m_isMusicOnHold = false;
            PlayTrack(Track.Game);
        }

        /// <summary>
        /// Holds the current track, e.g. on death. Unlike <see cref="StopMusic"/> the
        /// position is kept, and unmuting does not end the hold - only
        /// <see cref="ResumeMusic"/> or starting another track does.
        /// </summary>
        public void PauseMusic()
        {
            m_isMusicOnHold = true;
            m_musicSource.Pause();
        }

        public void ResumeMusic()
        {
            m_isMusicOnHold = false;
            PlayTrack(m_currentTrack);
        }

        /// <summary>
        /// Stops music entirely. Use <see cref="PlayMenuMusic"/> or
        /// <see cref="PlayGameMusic"/> to switch tracks - they handle the swap on
        /// their own.
        /// </summary>
        public void StopMusic()
        {
            m_currentTrack = Track.None;
            m_musicSource.Stop();
        }

        /// <summary>
        /// Switches between the classic and the new sound set. Public so the style
        /// can also be changed from somewhere other than the toggle.
        /// </summary>
        public void SetUseNewSounds(bool useNewSounds)
        {
            if (m_useNewSounds == useNewSounds)
            {
                return;
            }

            m_useNewSounds = useNewSounds;
            SaveSystem.Data.UseNewSounds = useNewSounds;
            SaveSystem.Save();

            SwapRunningMusicToCurrentStyle();
            SwapRunningEngineToCurrentStyle();
            RefreshUi();
        }

        public void ToggleSfxMuted()
        {
            m_isSfxMuted = !m_isSfxMuted;
            SaveSystem.Data.IsSfxMuted = m_isSfxMuted;
            SaveSystem.Save();

            m_engineSource.mute = m_isSfxMuted;
            RefreshUi();
        }

        public void ToggleMusicMuted()
        {
            m_isMusicMuted = !m_isMusicMuted;
            SaveSystem.Data.IsMusicMuted = m_isMusicMuted;
            SaveSystem.Save();

            // Pausing genuinely stops decoding, unlike volume 0, which keeps
            // burning CPU and battery on a phone while you hear nothing.
            if (m_isMusicMuted)
            {
                m_musicSource.Pause();
            }
            else
            {
                PlayTrack(m_currentTrack);
            }

            RefreshUi();
        }

        // The game only ever reloads back into the main menu, so reset to that state.
        private void SceneManager_SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StopEngine();
            PlayMenuMusic();
        }

        private void CreateSources()
        {
            m_sfxSource = gameObject.AddComponent<AudioSource>();
            m_sfxSource.playOnAwake = false;
            m_sfxSource.outputAudioMixerGroup = m_sfxGroup;

            m_engineSource = gameObject.AddComponent<AudioSource>();
            m_engineSource.playOnAwake = false;
            m_engineSource.loop = true;
            m_engineSource.outputAudioMixerGroup = m_sfxGroup;

            m_musicSource = gameObject.AddComponent<AudioSource>();
            m_musicSource.playOnAwake = false;
            m_musicSource.loop = true;
            m_musicSource.outputAudioMixerGroup = m_musicGroup;
        }

        private void LoadPreferences()
        {
            SaveData saveData = SaveSystem.Data;
            m_useNewSounds = saveData.UseNewSounds;
            m_isMusicMuted = saveData.IsMusicMuted;
            m_isSfxMuted = saveData.IsSfxMuted;
            m_masterVolume = Mathf.Clamp01(saveData.MasterVolume);
            AudioListener.volume = ConvertSliderToAmplitude(m_masterVolume);
        }

        // Prefers the active style, but falls back to the other bank so a sound
        // that exists in only one style is still heard rather than silently missing.
        private AudioClip PickClip(AudioClip fromNew, AudioClip fromClassic)
        {
            if (m_useNewSounds)
            {
                return fromNew != null ? fromNew : fromClassic;
            }

            return fromClassic != null ? fromClassic : fromNew;
        }

        private void PlaySfx(AudioClip clip)
        {
            // Muted SFX are not played at all - cheaper than playing them at zero volume.
            if (m_isSfxMuted || clip == null)
            {
                return;
            }

            m_sfxSource.PlayOneShot(clip);
        }

        private void PlayTrack(Track track)
        {
            m_currentTrack = track;

            AudioClip clip = GetTrackClip(track);
            if (clip == null)
            {
                m_musicSource.Stop();
                return;
            }

            if (m_musicSource.clip != clip)
            {
                m_musicSource.Stop();
                m_musicSource.clip = clip;
            }

            // Stay silent while muted or on hold; ToggleMusicMuted and ResumeMusic
            // continue from the paused position.
            if (m_isMusicMuted || m_isMusicOnHold)
            {
                return;
            }

            if (!m_musicSource.isPlaying)
            {
                m_musicSource.Play();
            }
        }

        private AudioClip GetTrackClip(Track track)
        {
            switch (track)
            {
                case Track.Menu:
                    return PickClip(NewSounds.MenuMusic, ClassicSounds.MenuMusic);

                case Track.Game:
                    return PickClip(NewSounds.GameMusic, ClassicSounds.GameMusic);

                default:
                    return null;
            }
        }

        // Keeps the playback position when the track changes, so switching style
        // mid-song is not jarring.
        private void SwapRunningMusicToCurrentStyle()
        {
            AudioClip clip = GetTrackClip(m_currentTrack);
            if (clip == null || m_musicSource.clip == clip)
            {
                return;
            }

            float position = m_musicSource.time;
            bool wasPlaying = m_musicSource.isPlaying;

            m_musicSource.Stop();
            m_musicSource.clip = clip;
            m_musicSource.time = Mathf.Clamp(position, 0f, Mathf.Max(0f, clip.length - k_TrackEndMargin));

            if (wasPlaying && !m_isMusicMuted)
            {
                m_musicSource.Play();
            }
        }

        private void SwapRunningEngineToCurrentStyle()
        {
            if (!m_engineSource.isPlaying)
            {
                return;
            }

            AudioClip clip = PickClip(NewSounds.Engine, ClassicSounds.Engine);
            if (clip == null || m_engineSource.clip == clip)
            {
                return;
            }

            m_engineSource.clip = clip;
            m_engineSource.Play();
        }

        private void BindUi()
        {
            if (m_newSoundsToggle != null)
            {
                m_newSoundsToggle.onValueChanged.RemoveListener(SetUseNewSounds);
                m_newSoundsToggle.SetIsOnWithoutNotify(m_useNewSounds);
                m_newSoundsToggle.onValueChanged.AddListener(SetUseNewSounds);
            }

            if (m_sfxButton != null)
            {
                m_sfxButton.onClick.RemoveListener(ToggleSfxMuted);
                m_sfxButton.onClick.AddListener(ToggleSfxMuted);
            }

            if (m_musicButton != null)
            {
                m_musicButton.onClick.RemoveListener(ToggleMusicMuted);
                m_musicButton.onClick.AddListener(ToggleMusicMuted);
            }

            if (m_volumeSlider != null)
            {
                m_volumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
                m_volumeSlider.SetValueWithoutNotify(m_masterVolume);
                m_volumeSlider.onValueChanged.AddListener(SetMasterVolume);
            }

            RefreshUi();
        }

        // Called on the surviving instance when a reloaded scene brings a fresh copy.
        private void AdoptSceneReferencesFrom(AudioSystem sceneCopy)
        {
            m_sfxButton = sceneCopy.m_sfxButton;
            m_sfxOnSprite = sceneCopy.m_sfxOnSprite;
            m_sfxOffSprite = sceneCopy.m_sfxOffSprite;
            m_musicButton = sceneCopy.m_musicButton;
            m_musicOnSprite = sceneCopy.m_musicOnSprite;
            m_musicOffSprite = sceneCopy.m_musicOffSprite;
            m_newSoundsToggle = sceneCopy.m_newSoundsToggle;
            m_volumeSlider = sceneCopy.m_volumeSlider;

            if (sceneCopy.m_classicBank != null)
            {
                m_classicBank = sceneCopy.m_classicBank;
            }

            if (sceneCopy.m_newBank != null)
            {
                m_newBank = sceneCopy.m_newBank;
            }

            BindUi();
        }

        private void RefreshUi()
        {
            ShowMuteState(m_sfxButton, m_sfxOnSprite, m_sfxOffSprite, m_isSfxMuted);
            ShowMuteState(m_musicButton, m_musicOnSprite, m_musicOffSprite, m_isMusicMuted);

            if (m_newSoundsToggle != null)
            {
                m_newSoundsToggle.SetIsOnWithoutNotify(m_useNewSounds);
            }

            if (m_volumeSlider != null)
            {
                m_volumeSlider.SetValueWithoutNotify(m_masterVolume);
            }
        }

        private static void ShowMuteState(Button button, Sprite onSprite, Sprite offSprite, bool isMuted)
        {
            if (button == null || onSprite == null || offSprite == null)
            {
                return;
            }

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = isMuted ? offSprite : onSprite;
            }
        }

        private static float ConvertSliderToAmplitude(float sliderPosition)
        {
            if (sliderPosition <= 0f)
            {
                return 0f;
            }

            return Mathf.Pow(10f, (sliderPosition - 1f) * k_VolumeRangeDb / 20f);
        }
    }
}
