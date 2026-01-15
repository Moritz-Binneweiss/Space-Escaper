using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public Animator pauseAnim,
        settingsAnim,
        menuAniim;

    public static bool GameIsPaused = false;

    // Start is called before the first frame update
    void Start()
    {
        GameIsPaused = false;
    }

    // Update is called once per frame
    void Update() { }

    public void SettingsOn()
    {
        settingsAnim.SetTrigger("Show");
        pauseAnim.SetTrigger("Hide");
    }

    public void SettingsOff()
    {
        if (GameIsPaused == true)
        {
            pauseAnim.SetTrigger("Show");
            settingsAnim.SetTrigger("Hide");
        }
        else
        {
            settingsAnim.SetTrigger("Hide");
            menuAniim.SetTrigger("Show");
        }
    }

    public void MenuSettingsOn()
    {
        settingsAnim.SetTrigger("Show");
        menuAniim.SetTrigger("Hide");
    }

    public void Paused()
    {
        GameIsPaused = true;
    }

    public void Continued()
    {
        GameIsPaused = false;
    }
}
