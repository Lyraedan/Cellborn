using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool isPaused = false;
    public KeyCode pauseButton;

    public GameObject pauseMenuUI;

    public string mainMenuSceneName = "MainMenu";

    public LoadingTips tipsScript;
    
    void Update()
    {
        if (Input.GetKeyDown(pauseButton))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        AudioManager.instance.Play("MenuClick");
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
        isPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        tipsScript.SetTip();
        isPaused = true;
    }

    public void ReturnToMenu()
    {
        AudioManager.instance.Play("MenuClick");
        Resume();
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        AudioManager.instance.Play("MenuClick");
        Resume();
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}

