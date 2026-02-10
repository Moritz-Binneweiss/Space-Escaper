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
    public Toggle audioStyleToggle;

    private AudioSource sfxSource;
    private AudioSource engineSource;
    private AudioSource musicSource;

    private bool sfxMuted;
    private bool musicMuted;
    private bool useNewAudio = true;

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
        sfxSource = gameObject.AddComponent<AudioSource>();
        engineSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        engineSource.loop = true;
        engineSource.mute = true;

        useNewAudio = PlayerPrefs.GetInt("USENEWSOUNDS", 1) == 1;
        musicMuted = PlayerPrefs.GetInt("MUSICMUTED", 1) == 0;
        sfxMuted = PlayerPrefs.GetInt("SFXMUTED", 1) == 0;

        if (audioStyleToggle != null)
        {
            audioStyleToggle.isOn = useNewAudio;
            audioStyleToggle.onValueChanged.AddListener(OnAudioStyleChanged);
        }

        engineSource.clip = useNewAudio ? newEngine : classicEngine;
        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (sfxButton != null)
        {
            sfxMuted = PlayerPrefs.GetInt("SFXMUTED", 1) == 0;
            sfxButton.GetComponent<Image>().sprite = sfxMuted ? sfxOff : sfxOn;
            sfxSource.volume = sfxMuted ? 0f : 1f;
            engineSource.volume = sfxMuted ? 0f : 1f;
        }

        if (musicButton != null)
        {
            musicMuted = PlayerPrefs.GetInt("MUSICMUTED", 1) == 0;
            musicButton.GetComponent<Image>().sprite = musicMuted ? musicOff : musicOn;
            musicSource.volume = musicMuted ? 0f : 1f;
        }
    }

    private void OnAudioStyleChanged(bool isOn)
    {
        useNewAudio = isOn;
        PlayerPrefs.SetInt("USENEWSOUNDS", useNewAudio ? 1 : 0);
        engineSource.clip = useNewAudio ? newEngine : classicEngine;

        if (musicSource.isPlaying)
        {
            float currentTime = musicSource.time;
            AudioClip currentClip = musicSource.clip;

            if (currentClip == classicMenuMusic || currentClip == newMenuMusic)
            {
                musicSource.clip = useNewAudio ? newMenuMusic : classicMenuMusic;
            }
            else if (currentClip == classicGameMusic || currentClip == newGameMusic)
            {
                musicSource.clip = useNewAudio ? newGameMusic : classicGameMusic;
            }

            musicSource.time = currentTime;
            musicSource.Play();
        }
    }

    public void PlayExplosion()
    {
        AudioClip clip = useNewAudio ? newExplosion : classicExplosion;
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayButton()
    {
        AudioClip clip = useNewAudio ? newButton : classicButton;
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayCoin()
    {
        AudioClip clip = useNewAudio ? newCoin : classicCoin;
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlaySelectShip()
    {
        AudioClip clip = useNewAudio ? newSelectShip : classicSelectShip;
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayDeselectShip()
    {
        if (newDeselectShip != null)
            sfxSource.PlayOneShot(newDeselectShip);
    }

    public void PlayBuyShip()
    {
        if (newBuyShip != null)
            sfxSource.PlayOneShot(newBuyShip);
    }

    public void PlayEngine()
    {
        if (engineSource.clip != null && !engineSource.isPlaying)
            engineSource.Play();
    }

    public void StartEngine()
    {
        engineSource.mute = false;
    }

    public void StopEngine()
    {
        engineSource.mute = true;
    }

    public void StartMenuMusic()
    {
        AudioClip clip = useNewAudio ? newMenuMusic : classicMenuMusic;
        if (clip != null)
        {
            if (musicSource.clip != clip)
            {
                musicSource.clip = clip;
                musicSource.Play();
            }
            else if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
    }

    public void StopMenuMusic()
    {
        AudioClip clip = useNewAudio ? newGameMusic : classicGameMusic;
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    public void MutePressed()
    {
        sfxMuted = !sfxMuted;
        PlayerPrefs.SetInt("SFXMUTED", sfxMuted ? 0 : 1);
    }

    public void MusicMutePressed()
    {
        musicMuted = !musicMuted;
        PlayerPrefs.SetInt("MUSICMUTED", musicMuted ? 0 : 1);
    }
}
