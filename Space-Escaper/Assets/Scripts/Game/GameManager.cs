using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceEscaper
{
    /// <summary>
    /// Holds the state of the game and changes it: main menu, hangar, run, pause and death.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private const string k_GameSceneName = "Game";

        [SerializeField] private ScreenManager m_screens;

        private GameState m_state;
        private bool m_hasRevived;

        public static GameManager Instance { get; private set; }

        /// <summary>
        /// Changed only here, by the methods the buttons call. A call that does not fit the
        /// state, like a pause outside a run, does nothing.
        /// </summary>
        public GameState State => m_state;

        /// <summary>
        /// A run has one revive.
        /// </summary>
        public bool CanRevive => !m_hasRevived;

        // Raised right after the state has changed, in the same call. A tap reaches the buttons
        // before any Update(), so on a revive the track is fresh before the ship flies on.
        public static event Action RunStarted;
        public static event Action HangarOpened;
        public static event Action HangarClosed;
        public static event Action RunPaused;
        public static event Action RunResumed;
        public static event Action ShipCrashed;
        public static event Action ShipRevived;

        // The editor starts Play Mode without a domain reload, so static fields would keep their
        // values from the last one.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
            RunStarted = null;
            HangarOpened = null;
            HangarClosed = null;
            RunPaused = null;
            RunResumed = null;
            ShipCrashed = null;
            ShipRevived = null;
        }

        private void Awake()
        {
            // "Exit" in the pause menu reloads the scene while the time stands still.
            Time.timeScale = 1f;
            Instance = this;
        }

        // A run must not go on while the player is away from the app, and must not
        // start again by itself when they come back. Android reports a pulled-down
        // notification shade only as lost focus, leaving the app as both. In the
        // Editor, clicking any window other than the Game view counts as lost focus.
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                Pause();
            }
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                Pause();
            }
        }

        /// <summary>
        /// Starts a run from the main menu.
        /// </summary>
        public void Play()
        {
            if (m_state != GameState.Menu)
            {
                return;
            }

            ChangeState(GameState.Run);
            OnRunStarted();
        }

        /// <summary>
        /// Pauses the run. Called by the pause button, and by the game itself when the app loses
        /// focus or goes to the background.
        /// </summary>
        public void Pause()
        {
            if (m_state != GameState.Run)
            {
                return;
            }

            ChangeState(GameState.Paused);
            OnRunPaused();
        }

        public void Continue()
        {
            if (m_state != GameState.Paused)
            {
                return;
            }

            ChangeState(GameState.Run);
            OnRunResumed();
        }

        /// <summary>
        /// Reloads the scene, which brings the game back to the main menu.
        /// </summary>
        public void ReturnToMenu()
        {
            SceneManager.LoadScene(k_GameSceneName);
        }

        /// <summary>
        /// Called by the ship when it crashes.
        /// </summary>
        public void HandleDeath()
        {
            if (m_state != GameState.Run)
            {
                return;
            }

            ChangeState(GameState.Dead);
            OnShipCrashed();
        }

        /// <summary>
        /// Revives the ship, once per run. There is no cost and no ad behind it at the moment.
        /// </summary>
        public void RequestRevive()
        {
            if (m_state != GameState.Dead || m_hasRevived)
            {
                return;
            }

            m_hasRevived = true;
            Revive();
        }

        public void OpenHangar()
        {
            if (m_state != GameState.Menu)
            {
                return;
            }

            ChangeState(GameState.Hangar);
            OnHangarOpened();
        }

        public void CloseHangar()
        {
            if (m_state != GameState.Hangar)
            {
                return;
            }

            ChangeState(GameState.Menu);
            OnHangarClosed();
        }

        private void Revive()
        {
            ChangeState(GameState.Run);
            OnShipRevived();
        }

        // Every state has its own screen, and only the pause stops the time.
        private void ChangeState(GameState state)
        {
            m_state = state;
            m_screens.Show(state);
            Time.timeScale = state == GameState.Paused ? 0f : 1f;
        }

        private static void OnRunStarted() => RunStarted?.Invoke();

        private static void OnHangarOpened() => HangarOpened?.Invoke();

        private static void OnHangarClosed() => HangarClosed?.Invoke();

        private static void OnRunPaused() => RunPaused?.Invoke();

        private static void OnRunResumed() => RunResumed?.Invoke();

        private static void OnShipCrashed() => ShipCrashed?.Invoke();

        private static void OnShipRevived() => ShipRevived?.Invoke();
    }
}
