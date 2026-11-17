using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The settings, opened from the main menu or the pause menu. Connects its audio controls to
    /// <see cref="AudioSystem"/>, which survives scene reloads, so the audio system needs no references
    /// into the scene. In the editor and in builds with checks it also has an Invincible switch for testing.
    /// </summary>
    public class SettingsScreen : MonoBehaviour
    {
#if UNITY_EDITOR || UNITY_ENABLE_CHECKS
        private const string k_InvincibleToggleName = "InvincibleToggle";
        private const string k_InvincibleLabel = "INVINCIBLE";
        private const string k_InvincibleDescription = "Fly through obstacles, test builds only";
#endif

        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private ScreenManager m_screens;
        [SerializeField] private Button m_backButton;
        [SerializeField] private Button m_sfxButton;
        [SerializeField] private Button m_musicButton;
        [Tooltip("The sounds of the 2020 original. Its look comes with the remaster in v1.7.0.")]
        [SerializeField] private Toggle m_classicModeToggle;
        [SerializeField] private Slider m_volumeSlider;

#if UNITY_EDITOR || UNITY_ENABLE_CHECKS
        private Toggle m_invincibleToggle;

        // The switch is a copy of the classic mode row made at runtime, so the scene stays as it is and release
        // builds have no trace of it. The layout of its section puts it below. On a phone it is easier to hit than
        // the three-finger double tap of the test panel, and it does not need the Rendering Debugger, whose first
        // opening costs a long frame and several megabytes.
        private void Awake()
        {
            m_invincibleToggle = Instantiate(m_classicModeToggle, m_classicModeToggle.transform.parent);
            m_invincibleToggle.name = k_InvincibleToggleName;
            TMP_Text[] texts = m_invincibleToggle.GetComponentsInChildren<TMP_Text>();
            texts[0].text = k_InvincibleLabel;
            texts[1].text = k_InvincibleDescription;
        }
#endif

        private void OnEnable()
        {
            m_backButton.onClick.AddListener(BackButton_Clicked);
            m_sfxButton.onClick.AddListener(SfxButton_Clicked);
            m_musicButton.onClick.AddListener(MusicButton_Clicked);
            m_classicModeToggle.onValueChanged.AddListener(ClassicModeToggle_ValueChanged);
            m_volumeSlider.onValueChanged.AddListener(VolumeSlider_ValueChanged);
#if UNITY_EDITOR || UNITY_ENABLE_CHECKS
            m_invincibleToggle.onValueChanged.AddListener(InvincibleToggle_ValueChanged);
            m_invincibleToggle.SetIsOnWithoutNotify(PlayerMotor.IsInvincible);
#endif
            ShowCurrentSettings();
        }

        private void OnDisable()
        {
            m_backButton.onClick.RemoveListener(BackButton_Clicked);
            m_sfxButton.onClick.RemoveListener(SfxButton_Clicked);
            m_musicButton.onClick.RemoveListener(MusicButton_Clicked);
            m_classicModeToggle.onValueChanged.RemoveListener(ClassicModeToggle_ValueChanged);
            m_volumeSlider.onValueChanged.RemoveListener(VolumeSlider_ValueChanged);
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

        // The click comes after the switch, so it is heard when the sound effects come back on and
        // not when they go off.
        private void SfxButton_Clicked()
        {
            AudioSystem.Instance.ToggleSfxMuted();
            AudioSystem.Instance.PlayButtonClick();
            ShowCurrentSettings();
        }

        private void MusicButton_Clicked()
        {
            AudioSystem.Instance.ToggleMusicMuted();
            AudioSystem.Instance.PlayButtonClick();
            ShowCurrentSettings();
        }

        private void ClassicModeToggle_ValueChanged(bool isOn)
        {
            AudioSystem.Instance.SetUseNewSounds(!isOn);
        }

        private void VolumeSlider_ValueChanged(float value)
        {
            AudioSystem.Instance.SetMasterVolume(value);
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

            ShowMuteState(m_sfxButton, audioSystem.IsSfxMuted);
            ShowMuteState(m_musicButton, audioSystem.IsMusicMuted);
            m_classicModeToggle.SetIsOnWithoutNotify(!audioSystem.UseNewSounds);
            m_volumeSlider.SetValueWithoutNotify(audioSystem.MasterVolume);
        }

        private static void ShowMuteState(Button button, bool isMuted)
        {
            button.GetComponent<OnOffButton>().ShowState(!isMuted);
        }
    }
}
