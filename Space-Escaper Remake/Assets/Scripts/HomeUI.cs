using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeUI : MonoBehaviour
{
    [SerializeField] GameObject settingsMenu;
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject creditsScreen;
    [SerializeField] GameObject collectiblesScreen;
    [SerializeField] GameObject achievmentsScreen;
    [SerializeField] GameObject versionsScreen;

    public void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void LoadHangarScene()
    {
        SceneManager.LoadScene("HangarScene");
    }

    public void OpenSettingsMenu()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OpenCreditsScreen()
    {
        mainMenu.SetActive(false);
        creditsScreen.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OpenCollectiblesScreen()
    {
        mainMenu.SetActive(false);
        collectiblesScreen.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OpenAchievmentsScreen()
    {
        mainMenu.SetActive(false);
        achievmentsScreen.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OpenVersionsScreen()
    {
        mainMenu.SetActive(false);
        versionsScreen.SetActive(true);
        Time.timeScale = 0f;
    }

    public void BackToMainMenu()
    {
        settingsMenu.SetActive(false);
        creditsScreen.SetActive(false);
        collectiblesScreen.SetActive(false);
        achievmentsScreen.SetActive(false);
        versionsScreen.SetActive(false);
        mainMenu.SetActive(true);
        Time.timeScale = 1f;
    }
}
