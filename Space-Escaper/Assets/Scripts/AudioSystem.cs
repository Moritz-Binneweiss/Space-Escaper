using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Central audio controller. Survives scene reloads via DontDestroyOnLoad and
/// plays everything through three sources: one-shot SFX, the looping engine,
/// and music.
///
/// Individual clips are not referenced here - they come from an <see cref="AudioBank"/>
/// per style, so switching between the classic 2020 sounds and the new ones is
/// simply a matter of reading from the other bank. Adding a sound to the game
/// means adding one field to AudioBank, not two fields here.
/// </summary>
public class AudioSystem : MonoBehaviour
{
    private const string PrefUseNewSounds = "USENEWSOUNDS";
    private const string PrefMusicMuted = "MUSICMUTED";
    private const string PrefSfxMuted = "SFXMUTED";
    private const string PrefMasterVolume = "MASTERVOLUME";

    public static AudioSystem Instance { get; private set; }

    [Header("Sound Banks")]
    public AudioBank classicBank;
    public AudioBank newBank;

    [Header("Mixer Routing (optional)")]
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;

    [Header("UI - lives in the scene, re-adopted on every scene load")]
    public Button sfxButton;
    public Sprite sfxOn;
    public Sprite sfxOff;
    public Button musicButton;
    public Sprite musicOn;
    public Sprite musicOff;
    public Toggle audioToggle;
    public Slider volumeSlider;

    private AudioSource sfxSource;
    private AudioSource engineSource;
    private AudioSource musicSource;

    /// Stand-in used when a bank slot is empty, so clip lookups never need a null check.
    private AudioBank emptyBank;

    private bool useNewSounds = true;
    private bool musicMuted;
    private bool sfxMuted;
    private float masterVolume = 1f;

    /// Which track the game currently wants to hear. Remembered so a style
    /// switch or an unmute can restart the right one.
    private enum Track
    {
        None,
        Menu,
        Game,
    }

    private Track currentTrack = Track.None;

    // ------------------------------------------------------------------
    // Lifetime

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

        engineSource.mute = sfxMuted;
        BindUi();

        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void Start()
    {
        PlayMenuMusic();
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= HandleSceneLoaded;
        Instance = null;
    }

    /// The game only ever reloads back into the main menu, so reset to that state.
    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopEngine();
        PlayMenuMusic();
    }

    private void CreateSources()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.outputAudioMixerGroup = sfxGroup;

        engineSource = gameObject.AddComponent<AudioSource>();
        engineSource.playOnAwake = false;
        engineSource.loop = true;
        engineSource.outputAudioMixerGroup = sfxGroup;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.outputAudioMixerGroup = musicGroup;
    }

    private void LoadPreferences()
    {
        useNewSounds = PlayerPrefs.GetInt(PrefUseNewSounds, 1) == 1;
        musicMuted = PlayerPrefs.GetInt(PrefMusicMuted, 1) == 0; // 1 = on, 0 = muted
        sfxMuted = PlayerPrefs.GetInt(PrefSfxMuted, 1) == 0;
        masterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefMasterVolume, 1f));
        AudioListener.volume = SliderToAmplitude(masterVolume);
    }

    /// Overall volume for everything the game plays, on top of the individual
    /// mute switches. AudioListener.volume is global, so this covers any source,
    /// not only the three owned by this class.
    ///
    /// The slider position is NOT used as the amplitude directly. Hearing is
    /// logarithmic, so on a linear scale the bottom few percent swallow most of
    /// the audible change (0.1 -> 0.2 is 6 dB) while the top third is barely
    /// distinguishable (0.7 -> 1.0 is only 3 dB). Mapping the travel onto a
    /// fixed decibel range instead makes every step of the slider sound equally
    /// big. Note that squaring the value does NOT fix this - a power curve
    /// stretches the whole dB range evenly and leaves the imbalance intact.
    public void SetMasterVolume(float sliderPosition)
    {
        masterVolume = Mathf.Clamp01(sliderPosition);
        AudioListener.volume = SliderToAmplitude(masterVolume);

        // Deliberately no PlayerPrefs.Save() here - a slider fires this on every
        // frame while being dragged. Flushed in OnApplicationPause/Quit instead.
        // The slider position is stored, not the amplitude, so the handle comes
        // back where the player left it.
        PlayerPrefs.SetFloat(PrefMasterVolume, masterVolume);
    }

    /// How much quieter the very bottom of the slider is than the top.
    /// 40 dB means every quarter of the travel roughly halves the perceived
    /// loudness. Raise it for a slider that reaches "almost silent" sooner.
    private const float VolumeRangeDb = 40f;

    private static float SliderToAmplitude(float sliderPosition)
    {
        if (sliderPosition <= 0f)
            return 0f;

        return Mathf.Pow(10f, (sliderPosition - 1f) * VolumeRangeDb / 20f);
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
            PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }

    // ------------------------------------------------------------------
    // Clip lookup

    private AudioBank Empty
    {
        get
        {
            if (emptyBank == null)
                emptyBank = ScriptableObject.CreateInstance<AudioBank>();
            return emptyBank;
        }
    }

    private AudioBank NewSounds => newBank != null ? newBank : Empty;
    private AudioBank ClassicSounds => classicBank != null ? classicBank : Empty;

    /// Prefers the active style, but falls back to the other bank so a sound
    /// that exists in only one style is still heard rather than silently missing.
    private AudioClip Pick(AudioClip fromNew, AudioClip fromClassic)
    {
        if (useNewSounds)
            return fromNew != null ? fromNew : fromClassic;
        return fromClassic != null ? fromClassic : fromNew;
    }

    // ------------------------------------------------------------------
    // Sound effects

    private void PlaySfx(AudioClip clip)
    {
        // Muted SFX are not played at all - cheaper than playing them at zero volume.
        if (sfxMuted || clip == null)
            return;

        sfxSource.PlayOneShot(clip);
    }

    public void PlayButton() => PlaySfx(Pick(NewSounds.button, ClassicSounds.button));

    public void PlayCoin() => PlaySfx(Pick(NewSounds.coin, ClassicSounds.coin));

    public void PlayExplosion() => PlaySfx(Pick(NewSounds.explosion, ClassicSounds.explosion));

    public void PlaySelectShip() => PlaySfx(Pick(NewSounds.selectShip, ClassicSounds.selectShip));

    public void PlayDeselectShip() =>
        PlaySfx(Pick(NewSounds.deselectShip, ClassicSounds.deselectShip));

    public void PlayBuyShip() => PlaySfx(Pick(NewSounds.buyShip, ClassicSounds.buyShip));

    // ------------------------------------------------------------------
    // Engine loop

    public void StartEngine()
    {
        AudioClip clip = Pick(NewSounds.engine, ClassicSounds.engine);
        if (clip == null)
            return;

        if (engineSource.clip != clip)
        {
            engineSource.Stop();
            engineSource.clip = clip;
        }

        engineSource.mute = sfxMuted;
        if (!engineSource.isPlaying)
            engineSource.Play();
    }

    public void StopEngine()
    {
        if (engineSource.isPlaying)
            engineSource.Stop();
    }

    // ------------------------------------------------------------------
    // Music

    public void PlayMenuMusic() => PlayTrack(Track.Menu);

    public void PlayGameMusic() => PlayTrack(Track.Game);

    /// Stops music entirely. Use PlayMenuMusic / PlayGameMusic to switch tracks -
    /// they handle the swap on their own.
    public void StopMusic()
    {
        currentTrack = Track.None;
        musicSource.Stop();
    }

    private void PlayTrack(Track track)
    {
        currentTrack = track;

        AudioClip clip = ClipFor(track);
        if (clip == null)
        {
            musicSource.Stop();
            return;
        }

        if (musicSource.clip != clip)
        {
            musicSource.Stop();
            musicSource.clip = clip;
        }

        // Stay silent while muted; unmuting resumes through ToggleMusicMuted.
        if (musicMuted)
            return;

        if (!musicSource.isPlaying)
            musicSource.Play();
    }

    private AudioClip ClipFor(Track track)
    {
        if (track == Track.Menu)
            return Pick(NewSounds.menuMusic, ClassicSounds.menuMusic);
        if (track == Track.Game)
            return Pick(NewSounds.gameMusic, ClassicSounds.gameMusic);
        return null;
    }

    // ------------------------------------------------------------------
    // Settings

    /// Switches between the classic and the new sound set. Public so the style
    /// can also be changed from somewhere other than the toggle.
    public void SetUseNewSounds(bool value)
    {
        if (useNewSounds == value)
            return;

        useNewSounds = value;
        PlayerPrefs.SetInt(PrefUseNewSounds, value ? 1 : 0);
        PlayerPrefs.Save();

        SwapRunningMusicToCurrentStyle();
        SwapRunningEngineToCurrentStyle();
        RefreshUi();
    }

    /// Keeps the playback position when the track changes, so switching style
    /// mid-song is not jarring.
    private void SwapRunningMusicToCurrentStyle()
    {
        AudioClip clip = ClipFor(currentTrack);
        if (clip == null || musicSource.clip == clip)
            return;

        float position = musicSource.time;
        bool wasPlaying = musicSource.isPlaying;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.time = Mathf.Clamp(position, 0f, Mathf.Max(0f, clip.length - 0.05f));

        if (wasPlaying && !musicMuted)
            musicSource.Play();
    }

    private void SwapRunningEngineToCurrentStyle()
    {
        if (!engineSource.isPlaying)
            return;

        AudioClip clip = Pick(NewSounds.engine, ClassicSounds.engine);
        if (clip == null || engineSource.clip == clip)
            return;

        engineSource.clip = clip;
        engineSource.Play();
    }

    public void ToggleSfxMuted()
    {
        sfxMuted = !sfxMuted;
        PlayerPrefs.SetInt(PrefSfxMuted, sfxMuted ? 0 : 1); // 1 = on, 0 = muted
        PlayerPrefs.Save();

        engineSource.mute = sfxMuted;
        RefreshUi();
    }

    public void ToggleMusicMuted()
    {
        musicMuted = !musicMuted;
        PlayerPrefs.SetInt(PrefMusicMuted, musicMuted ? 0 : 1); // 1 = on, 0 = muted
        PlayerPrefs.Save();

        // Pausing genuinely stops decoding, unlike volume 0, which keeps
        // burning CPU and battery on a phone while you hear nothing.
        if (musicMuted)
            musicSource.Pause();
        else
            PlayTrack(currentTrack);

        RefreshUi();
    }

    // ------------------------------------------------------------------
    // UI

    private void BindUi()
    {
        if (audioToggle != null)
        {
            audioToggle.onValueChanged.RemoveListener(SetUseNewSounds);
            audioToggle.SetIsOnWithoutNotify(useNewSounds);
            audioToggle.onValueChanged.AddListener(SetUseNewSounds);
        }

        if (sfxButton != null)
        {
            sfxButton.onClick.RemoveListener(ToggleSfxMuted);
            sfxButton.onClick.AddListener(ToggleSfxMuted);
        }

        if (musicButton != null)
        {
            musicButton.onClick.RemoveListener(ToggleMusicMuted);
            musicButton.onClick.AddListener(ToggleMusicMuted);
        }

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
            volumeSlider.SetValueWithoutNotify(masterVolume);
            volumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        RefreshUi();
    }

    /// Called on the surviving instance when a reloaded scene brings a fresh copy.
    private void AdoptSceneReferencesFrom(AudioSystem sceneCopy)
    {
        sfxButton = sceneCopy.sfxButton;
        sfxOn = sceneCopy.sfxOn;
        sfxOff = sceneCopy.sfxOff;
        musicButton = sceneCopy.musicButton;
        musicOn = sceneCopy.musicOn;
        musicOff = sceneCopy.musicOff;
        audioToggle = sceneCopy.audioToggle;
        volumeSlider = sceneCopy.volumeSlider;

        if (sceneCopy.classicBank != null)
            classicBank = sceneCopy.classicBank;
        if (sceneCopy.newBank != null)
            newBank = sceneCopy.newBank;

        BindUi();
    }

    /// Only runs when something actually changed - the old version did this
    /// every frame in Update().
    private void RefreshUi()
    {
        if (sfxButton != null && sfxOn != null && sfxOff != null)
        {
            Image image = sfxButton.GetComponent<Image>();
            if (image != null)
                image.sprite = sfxMuted ? sfxOff : sfxOn;
        }

        if (musicButton != null && musicOn != null && musicOff != null)
        {
            Image image = musicButton.GetComponent<Image>();
            if (image != null)
                image.sprite = musicMuted ? musicOff : musicOn;
        }

        if (audioToggle != null)
            audioToggle.SetIsOnWithoutNotify(useNewSounds);

        if (volumeSlider != null)
            volumeSlider.SetValueWithoutNotify(masterVolume);
    }
}
