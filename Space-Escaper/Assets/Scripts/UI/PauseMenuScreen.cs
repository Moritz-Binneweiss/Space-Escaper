using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The pause menu: the run so far and switches for the music and the sound effects. Continues the run, opens the
    /// settings or goes back to the main menu.
    /// </summary>
    public class PauseMenuScreen : MonoBehaviour
    {
        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private ScreenManager m_screens;
        [SerializeField] private RunScore m_runScore;
        [SerializeField] private TMP_Text m_timeText;
        [SerializeField] private TMP_Text m_scoreText;
        [SerializeField] private TMP_Text m_coinText;
        [SerializeField] private TMP_Text m_highscoreText;
        [SerializeField] private TMP_Text m_distanceText;
        [SerializeField] private Button m_musicButton;
        [SerializeField] private Button m_sfxButton;
        [SerializeField] private Button m_continueButton;
        [SerializeField] private Button m_settingsButton;
        [SerializeField] private Button m_exitButton;

        // For tests, which tap it when a focus change in the editor pauses the run.
        internal Button ContinueButton => m_continueButton;

        // The run stands still while this screen is up, so its numbers are set once.
        private void OnEnable()
        {
            m_musicButton.onClick.AddListener(MusicButton_Clicked);
            m_sfxButton.onClick.AddListener(SfxButton_Clicked);
            m_continueButton.onClick.AddListener(m_gameManager.Continue);
            m_settingsButton.onClick.AddListener(m_screens.ShowSettings);
            m_exitButton.onClick.AddListener(m_gameManager.ReturnToMenu);

            int seconds = (int)m_runScore.FlightTime;
            float minutes = seconds / 60;
            float secondsOfMinute = seconds % 60;
            m_timeText.SetText("TIME {0}:{1:00}", minutes, secondsOfMinute);
            m_scoreText.SetText("{0}", m_runScore.Score);
            m_coinText.SetText("+{0}", m_runScore.Coins);
            m_highscoreText.SetText("{0}", m_runScore.Highscore);
            m_distanceText.SetText("{0} m", m_runScore.Distance);
            ShowAudioSwitches();
        }

        private void OnDisable()
        {
            m_musicButton.onClick.RemoveListener(MusicButton_Clicked);
            m_sfxButton.onClick.RemoveListener(SfxButton_Clicked);
            m_continueButton.onClick.RemoveListener(m_gameManager.Continue);
            m_settingsButton.onClick.RemoveListener(m_screens.ShowSettings);
            m_exitButton.onClick.RemoveListener(m_gameManager.ReturnToMenu);
        }

        private void MusicButton_Clicked()
        {
            AudioSystem.Instance.ToggleMusicMuted();
            AudioSystem.Instance.PlayButtonClick();
            ShowAudioSwitches();
        }

        // The click comes after the switch, so it is heard when the sound effects come back on and
        // not when they go off.
        private void SfxButton_Clicked()
        {
            AudioSystem.Instance.ToggleSfxMuted();
            AudioSystem.Instance.PlayButtonClick();
            ShowAudioSwitches();
        }

        private void ShowAudioSwitches()
        {
            AudioSystem audioSystem = AudioSystem.Instance;

            // Missing only if the scene was saved with this screen switched on: then it can wake before
            // the audio system, and the screen manager switches it off right away.
            if (audioSystem == null)
            {
                return;
            }

            ShowState(m_musicButton, !audioSystem.IsMusicMuted);
            ShowState(m_sfxButton, !audioSystem.IsSfxMuted);
        }

        private static void ShowState(Button button, bool isOn)
        {
            button.GetComponent<OnOffButton>().ShowState(isOn);
        }
    }
}
