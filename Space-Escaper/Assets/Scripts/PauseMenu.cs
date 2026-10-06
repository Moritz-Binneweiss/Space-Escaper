using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public Animator pauseAnim,
        gameMenuAnim;

    /// The one pause state of the game. SettingsManager reads it to decide where
    /// "Back" leads. Static, so it would survive the scene reload behind "Exit"
    /// and stay stuck on true - hence the reset in Awake.
    public static bool GameIsPaused { get; private set; }

    private AudioSystem engine;

    private void Awake()
    {
        GameIsPaused = false;
    }

    private void Start()
    {
        engine = AudioSystem.Instance;
    }

    public void Continue()
    {
        if (!GameIsPaused)
            return;

        GameIsPaused = false;
        engine.StartEngine();
        gameMenuAnim.SetTrigger("Show");
        pauseAnim.SetTrigger("Hide");
        Time.timeScale = 1f;
    }

    /// Safe to call from anywhere, not only from the pause button: it only pauses
    /// a run that is going on with the ship alive, and only once.
    public void Pause()
    {
        if (GameIsPaused || !GameManager.Instance.IsRunActive)
            return;

        GameIsPaused = true;
        engine.StopEngine();
        gameMenuAnim.SetTrigger("Hide");
        pauseAnim.SetTrigger("Show");
        Time.timeScale = 0f;
    }
}
