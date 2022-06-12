using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu instance;
    
    public static bool isPaused = false;

    public GameObject pauseMenuUI;

    public string mainMenuSceneName = "MainMenu";

    public LoadingTips tipsScript;

    public AudioSource source;
    public AudioClip menuClickSound;
    public bool canPause = true;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    void Start()
    {
        canPause = true;
    }
    
    void Update()
    {
        if (!canPause)
            return;

        if (RoomGenerator.instance.cutscenePlaying)
            return;

        if (Input.GetButtonDown(ControlManager.INPUT_PAUSE))
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
        source.clip = menuClickSound;
        source.Play();
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
        isPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        //tipsScript.SetTip();
        isPaused = true;
    }

    public void RegenLevel()
    {
        Resume();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMenu()
    {
        source.clip = menuClickSound;
        source.Play();
        Resume();
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        source.clip = menuClickSound;
        source.Play();
        Resume();
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}

