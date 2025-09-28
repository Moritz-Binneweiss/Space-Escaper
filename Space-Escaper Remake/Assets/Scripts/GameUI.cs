using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject gameOverlay;
    [SerializeField] GameObject deathMenu;

    public void PauseGame()
    {
        gameOverlay.SetActive(false);
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ContinueGame()
    {
        pauseMenu.SetActive(false);
        gameOverlay.SetActive(true);
        Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        // Load settings menu
    }

    public void QuitGame()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public void ShowDeathMenu()
    {
        gameOverlay.SetActive(false);
        deathMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Revive()
    {
        deathMenu.SetActive(false);
        gameOverlay.SetActive(true);
        Time.timeScale = 1f;
    }

    public void Exit()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
