using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The settings, opened from the main menu or the pause menu. Connects its audio controls to
    /// <see cref="AudioSystem"/>, which survives scene reloads, so the audio system needs no references
    /// into the scene, opens the colorways and the credits and shows the version of the game. In the editor and
    /// in builds with checks it also has an Invincible switch for testing.
    /// </summary>
    public class SettingsScreen : MonoBehaviour
    {
#if UNITY_EDITOR || UNITY_ENABLE_CHECKS
        private const string k_InvincibleToggleName = "InvincibleToggle";
        private const string k_InvincibleTitleKey = "settings.invincible.title";
        private const string k_InvincibleDescriptionKey = "settings.invincible.description";
#endif

        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private ScreenManager m_screens;
        [SerializeField] private Button m_backButton;
        [SerializeField] private Slider m_musicSlider;
        [SerializeField] private TMP_Text m_musicVolumeText;
        [SerializeField] private Slider m_sfxSlider;
        [SerializeField] private TMP_Text m_sfxVolumeText;
        [Tooltip("The sounds of the 2020 original. Its look comes with the remaster in v1.7.0.")]
        [SerializeField] private Toggle m_classicModeToggle;
        [SerializeField] private Button m_colorwayButton;
        [Tooltip("The name of the colorway the UI shows.")]
        [SerializeField] private TMP_Text m_colorwayText;
        [SerializeField] private Button m_creditsButton;
        [SerializeField] private TMP_Text m_versionText;

#if UNITY_EDITOR || UNITY_ENABLE_CHECKS
        private Toggle m_invincibleToggle;
#endif

        // For tests, which tap them like a player.
        internal Button BackButton => m_backButton;

        internal Button ColorwayButton => m_colorwayButton;

        internal TMP_Text ColorwayText => m_colorwayText;

        internal Toggle ClassicModeToggle => m_classicModeToggle;

#if UNITY_EDITOR || UNITY_ENABLE_CHECKS
        // The switch is a copy of the classic mode row made at runtime, so the scene stays as it is and release
        // builds have no trace of it. The layout of its section puts it below. On a phone it is easier to hit than
        // the three-finger double tap of the test panel, and it does not need the Rendering Debugger, whose first
        // opening costs a long frame and several megabytes. The copy has the keys of the classic mode row, so its
        // texts get keys of their own.
        private void Awake()
        {
            m_invincibleToggle = Instantiate(m_classicModeToggle, m_classicModeToggle.transform.parent);
            m_invincibleToggle.name = k_InvincibleToggleName;
            LocalizedText[] texts = m_invincibleToggle.GetComponentsInChildren<LocalizedText>();
            texts[0].Key = k_InvincibleTitleKey;
            texts[1].Key = k_InvincibleDescriptionKey;
        }
#endif

        private void OnEnable()
        {
            m_backButton.onClick.AddListener(BackButton_Clicked);
            m_musicSlider.onValueChanged.AddListener(MusicSlider_ValueChanged);
            m_sfxSlider.onValueChanged.AddListener(SfxSlider_ValueChanged);
            m_classicModeToggle.onValueChanged.AddListener(ClassicModeToggle_ValueChanged);
            m_colorwayButton.onClick.AddListener(m_screens.ShowColorways);
            m_creditsButton.onClick.AddListener(m_screens.ShowCredits);
#if UNITY_EDITOR || UNITY_ENABLE_CHECKS
            m_invincibleToggle.onValueChanged.AddListener(InvincibleToggle_ValueChanged);
            m_invincibleToggle.SetIsOnWithoutNotify(PlayerMotor.IsInvincible);
#endif
            m_versionText.SetText(Application.version);
            m_colorwayText.SetText(ColorwayManager.Current.Title);
            ShowCurrentSettings();
        }

        private void OnDisable()
        {
            m_backButton.onClick.RemoveListener(BackButton_Clicked);
            m_musicSlider.onValueChanged.RemoveListener(MusicSlider_ValueChanged);
            m_sfxSlider.onValueChanged.RemoveListener(SfxSlider_ValueChanged);
            m_classicModeToggle.onValueChanged.RemoveListener(ClassicModeToggle_ValueChanged);
            m_colorwayButton.onClick.RemoveListener(m_screens.ShowColorways);
            m_creditsButton.onClick.RemoveListener(m_screens.ShowCredits);
#if UNITY_EDITOR || UNITY_ENABLE_CHECKS
            m_invincibleToggle.onValueChanged.RemoveListener(InvincibleToggle_ValueChanged);
#endif
        }

        // The settings leave the state of the game as it is, so going back shows the screen of the state:
        // the pause menu during a run, the main menu otherwise.
        private void BackButton_Clicked()
        {
            m_screens.Show(m_gameManager.State);
        }

        private void MusicSlider_ValueChanged(float value)
        {
            AudioSystem.Instance.SetMusicVolume(value);
            ShowPercent(m_musicVolumeText, value);
        }

        private void SfxSlider_ValueChanged(float value)
        {
            AudioSystem.Instance.SetSfxVolume(value);
            ShowPercent(m_sfxVolumeText, value);
        }

        private void ClassicModeToggle_ValueChanged(bool isOn)
        {
            AudioSystem.Instance.SetUseNewSounds(!isOn);
        }

#if UNITY_EDITOR || UNITY_ENABLE_CHECKS
        private void InvincibleToggle_ValueChanged(bool isOn)
        {
            PlayerMotor.IsInvincible = isOn;
        }
#endif

        private void ShowCurrentSettings()
        {
            AudioSystem audioSystem = AudioSystem.Instance;

            // Missing only if the scene was saved with this screen switched on: then it can wake before
            // the audio system, and the screen manager switches it off right away. The next open shows
            // the settings.
            if (audioSystem == null)
            {
                return;
            }

            m_musicSlider.SetValueWithoutNotify(audioSystem.MusicVolume);
            ShowPercent(m_musicVolumeText, audioSystem.MusicVolume);
            m_sfxSlider.SetValueWithoutNotify(audioSystem.SfxVolume);
            ShowPercent(m_sfxVolumeText, audioSystem.SfxVolume);
            m_classicModeToggle.SetIsOnWithoutNotify(!audioSystem.UseNewSounds);
        }

        private static void ShowPercent(TMP_Text text, float sliderPosition)
        {
            text.SetText("{0}%", Mathf.Round(sliderPosition * 100f));
        }
    }
}
