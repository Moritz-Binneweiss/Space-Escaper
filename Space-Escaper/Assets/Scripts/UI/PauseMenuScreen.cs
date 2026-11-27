using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpaceEscaper
{
    /// <summary>
    /// The pause menu: the run so far and switches for the music and the sound effects. Continues the run, opens the
    /// settings or goes back to the main menu. A finger can also swipe it away downwards, which continues the run.
    /// </summary>
    public class PauseMenuScreen : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        // A finger that rests this long before it lets go has stopped, it swipes with no speed.
        private const float k_RestTime = 0.1f;

        [SerializeField] private GameManager m_gameManager;
        [SerializeField] private ScreenManager m_screens;
        [SerializeField] private RunScore m_runScore;
        [SerializeField] private LocalizedText m_timeText;
        [SerializeField] private LocalizedText m_scoreText;
        [SerializeField] private LocalizedText m_coinText;
        [SerializeField] private LocalizedText m_highscoreText;
        [SerializeField] private LocalizedText m_distanceText;
        [SerializeField] private Button m_musicButton;
        [SerializeField] private Button m_sfxButton;
        [SerializeField] private Button m_continueButton;
        [SerializeField] private Button m_settingsButton;
        [SerializeField] private Button m_exitButton;

        private float m_dragSpeed;
        private float m_lastDragTime;

        // For tests, which tap it when a focus change in the editor pauses the run.
        internal Button ContinueButton => m_continueButton;

        // The run stands still while this screen is up, so its numbers are set once.
        private void OnEnable()
        {
            m_musicButton.onClick.AddListener(MusicButton_Clicked);
            m_sfxButton.onClick.AddListener(SfxButton_Clicked);
            m_continueButton.onClick.AddListener(m_gameManager.Continue);
            m_settingsButton.onClick.AddListener(m_screens.ShowSettings);
            m_exitButton.onClick.AddListener(ExitButton_Clicked);

            int seconds = (int)m_runScore.FlightTime;
            float minutes = seconds / 60;
            float secondsOfMinute = seconds % 60;
            m_timeText.ShowNumbers(minutes, secondsOfMinute);
            m_scoreText.ShowNumbers(m_runScore.Score);
            m_coinText.ShowNumbers(m_runScore.Coins);
            m_highscoreText.ShowNumbers(m_runScore.Highscore);
            m_distanceText.ShowNumbers(m_runScore.Distance);
            ShowAudioSwitches();
        }

        private void OnDisable()
        {
            m_musicButton.onClick.RemoveListener(MusicButton_Clicked);
            m_sfxButton.onClick.RemoveListener(SfxButton_Clicked);
            m_continueButton.onClick.RemoveListener(m_gameManager.Continue);
            m_settingsButton.onClick.RemoveListener(m_screens.ShowSettings);
            m_exitButton.onClick.RemoveListener(ExitButton_Clicked);
        }

        // A drag anywhere on the screen moves the sheet, also one that starts on a button, which then does not click.
        public void OnBeginDrag(PointerEventData eventData)
        {
            m_dragSpeed = 0f;
            m_lastDragTime = Time.unscaledTime;
        }

        public void OnDrag(PointerEventData eventData)
        {
            m_screens.DragPauseSheet(eventData.delta.y);
            if (Time.unscaledDeltaTime > 0f)
            {
                m_dragSpeed = -eventData.delta.y / Time.unscaledDeltaTime;
            }

            m_lastDragTime = Time.unscaledTime;
        }

        // Let go far or fast enough, the sheet goes on down and the run goes on, else the sheet springs back.
        public void OnEndDrag(PointerEventData eventData)
        {
            float speed = Time.unscaledTime - m_lastDragTime < k_RestTime ? m_dragSpeed : 0f;
            if (m_screens.IsPauseSheetSwipedAway(speed))
            {
                m_gameManager.Continue();
            }
            else
            {
                m_screens.SettlePauseSheet();
            }
        }

        // The scene reloads behind the curtain.
        private void ExitButton_Clicked()
        {
            m_screens.ChangeBehindCurtain(m_gameManager.ReturnToMenu);
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
