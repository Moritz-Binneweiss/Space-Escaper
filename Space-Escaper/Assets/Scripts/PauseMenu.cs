using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public Animator pauseAnim,
        gameMenuAnim;

    public static bool GameIsPaused = false;

    private SoundManager engine;

    public SettingsManager sett;

    // Update is called once per frame
    void Start()
    {
        engine = GameObject.FindGameObjectWithTag("Audio").GetComponent<SoundManager>();
        sett = GetComponent<SettingsManager>();
    }

    public void Continue()
    {
        sett.Continued();
        engine.StartEngine();
        gameMenuAnim.SetTrigger("Show");
        pauseAnim.SetTrigger("Hide");
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    public void Pause()
    {
        sett.Paused();
        engine.StopEngine();
        gameMenuAnim.SetTrigger("Hide");
        pauseAnim.SetTrigger("Show");
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void Quit()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }
}
