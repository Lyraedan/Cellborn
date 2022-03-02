using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuGroup, creditsGroup;
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OpenCredits()
    {
        mainMenuGroup.SetActive(false);
        creditsGroup.SetActive(true);
    }

    public void OpenMainMenu()
    {
        mainMenuGroup.SetActive(true);
        creditsGroup.SetActive(false);
    }

    public void StartGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void CloseGame()
    {
        Application.Quit();
    }
}
