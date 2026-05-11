using UnityEngine;
using UnityEngine.UI;

public class AudioSystem : MonoBehaviour
{
    public static AudioSystem Instance { get; private set; }

    [Header("Classic Audio Clips")]
    public AudioClip classicExplosion;
    public AudioClip classicButton;
    public AudioClip classicEngine;
    public AudioClip classicCoin;
    public AudioClip classicSelectShip;
    public AudioClip classicMenuMusic;
    public AudioClip classicGameMusic;

    [Header("New Audio Clips")]
    public AudioClip newExplosion;
    public AudioClip newButton;
    public AudioClip newEngine;
    public AudioClip newCoin;
    public AudioClip newSelectShip;
    public AudioClip newDeselectShip;
    public AudioClip newBuyShip;
    public AudioClip newMenuMusic;
    public AudioClip newGameMusic;

    [Header("UI Elements")]
    public Button sfxButton;
    public Sprite sfxOn;
    public Sprite sfxOff;
    public Button musicButton;
    public Sprite musicOn;
    public Sprite musicOff;
    public Toggle audioToggle;

    private AudioSource sfxSource;
    private AudioSource engineSource;
    private AudioSource musicSource;

    private bool useNewSounds = true;
    private bool musicMuted = false;
    private bool sfxMuted = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Create AudioSources dynamically
        sfxSource = gameObject.AddComponent<AudioSource>();
        engineSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();

        // Configure AudioSources
        sfxSource.playOnAwake = false;
        engineSource.playOnAwake = false;
        engineSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.loop = true;

        // Load preferences
        useNewSounds = PlayerPrefs.GetInt("USENEWSOUNDS", 1) == 1;
        musicMuted = PlayerPrefs.GetInt("MUSICMUTED", 1) == 0; // 1 = ON, 0 = MUTED
        sfxMuted = PlayerPrefs.GetInt("SFXMUTED", 1) == 0; // 1 = ON, 0 = MUTED

        // Setup UI
        if (audioToggle != null)
        {
            audioToggle.isOn = useNewSounds;
            audioToggle.onValueChanged.AddListener(OnAudioStyleChanged);
        }

        if (sfxButton != null)
        {
            sfxButton.onClick.AddListener(MutePressed);
        }

        if (musicButton != null)
        {
            musicButton.onClick.AddListener(MusicMutePressed);
        }

        Debug.Log(
            "[AudioSystem] Started! useNewSounds="
                + useNewSounds
                + " musicMuted="
                + musicMuted
                + " sfxMuted="
                + sfxMuted
        );
        StartMenuMusic();
        UpdateUI();
    }

    private AudioClip GetClip(AudioClip newClip, AudioClip classicClip)
    {
        if (useNewSounds && newClip != null)
            return newClip;
        return classicClip;
    }

    void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Update button sprites
        if (sfxButton != null && sfxOn != null && sfxOff != null)
        {
            sfxButton.GetComponent<Image>().sprite = sfxMuted ? sfxOff : sfxOn;
        }

        if (musicButton != null && musicOn != null && musicOff != null)
        {
            musicButton.GetComponent<Image>().sprite = musicMuted ? musicOff : musicOn;
        }

        // Apply mute state using volume
        sfxSource.volume = sfxMuted ? 0f : 1f;
        engineSource.volume = sfxMuted ? 0f : 1f;
        musicSource.volume = musicMuted ? 0f : 1f;
    }

    private void OnAudioStyleChanged(bool isOn)
    {
        useNewSounds = isOn;
        PlayerPrefs.SetInt("USENEWSOUNDS", isOn ? 1 : 0);
        PlayerPrefs.Save();

        // Switch music seamlessly
        if (musicSource.isPlaying)
        {
            float currentTime = musicSource.time;
            AudioClip newClip = useNewSounds ? newMenuMusic : classicMenuMusic;
            if (newClip != null && musicSource.clip != newClip)
            {
                musicSource.clip = newClip;
                musicSource.time = Mathf.Min(currentTime, newClip.length);
                musicSource.Play();
            }
        }
    }

    public void PlayExplosion()
    {
        AudioClip clip = GetClip(newExplosion, classicExplosion);
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayButton()
    {
        AudioClip clip = GetClip(newButton, classicButton);
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayCoin()
    {
        AudioClip clip = GetClip(newCoin, classicCoin);
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlaySelectShip()
    {
        AudioClip clip = GetClip(newSelectShip, classicSelectShip);
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayDeselectShip()
    {
        if (useNewSounds && newDeselectShip != null)
        {
            sfxSource.PlayOneShot(newDeselectShip);
        }
    }

    public void PlayBuyShip()
    {
        if (useNewSounds && newBuyShip != null)
        {
            sfxSource.PlayOneShot(newBuyShip);
        }
    }

    public void PlayEngine()
    {
        AudioClip clip = GetClip(newEngine, classicEngine);
        if (clip != null)
            engineSource.PlayOneShot(clip);
    }

    public void StartEngine()
    {
        AudioClip clip = GetClip(newEngine, classicEngine);
        if (clip != null && !engineSource.isPlaying)
        {
            engineSource.clip = clip;
            engineSource.Play();
        }
    }

    public void StopEngine()
    {
        if (engineSource.isPlaying)
        {
            engineSource.Stop();
        }
    }

    public void StartMenuMusic()
    {
        AudioClip clip = GetClip(newMenuMusic, classicMenuMusic);
        if (clip != null && !musicSource.isPlaying)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    public void StartGameMusic()
    {
        AudioClip clip = GetClip(newGameMusic, classicGameMusic);
        Debug.Log(
            "[AudioSystem] StartGameMusic called. useNewSounds="
                + useNewSounds
                + " clip="
                + (clip != null ? clip.name : "NULL")
                + " musicMuted="
                + musicMuted
                + " volume="
                + musicSource.volume
        );
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.Play();
            Debug.Log(
                "[AudioSystem] Music playing: "
                    + clip.name
                    + " isPlaying="
                    + musicSource.isPlaying
                    + " clip.length="
                    + clip.length
                    + " clip.loadState="
                    + clip.loadState
            );
        }
    }

    public void StopMenuMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    public void MutePressed()
    {
        sfxMuted = !sfxMuted;
        PlayerPrefs.SetInt("SFXMUTED", sfxMuted ? 0 : 1); // 1 = ON, 0 = MUTED
        PlayerPrefs.Save();
        UpdateUI();
    }

    public void MusicMutePressed()
    {
        musicMuted = !musicMuted;
        PlayerPrefs.SetInt("MUSICMUTED", musicMuted ? 0 : 1); // 1 = ON, 0 = MUTED
        PlayerPrefs.Save();
        UpdateUI();
    }
}
