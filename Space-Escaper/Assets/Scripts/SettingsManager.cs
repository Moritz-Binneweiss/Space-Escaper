using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public Animator pauseAnim,
        settingsAnim,
        menuAniim;

    public void SettingsOn()
    {
        settingsAnim.SetTrigger("Show");
        pauseAnim.SetTrigger("Hide");
    }

    public void SettingsOff()
    {
        // Back leads to where the settings were opened from: the pause menu
        // during a run, the main menu otherwise.
        if (PauseMenu.GameIsPaused)
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
}
