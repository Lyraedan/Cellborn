using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuGroup, creditsGroup, loadingGroup, optionsGroup, startGameGroup, logo;

    public Slider loadBar;
    public TextMeshProUGUI loadBarText;

    public VideoPlayer videoPlayer;
    public List<VideoClip> backgroundRenders;
    int chosenBackground;
    
    void Start()
    {
        chosenBackground = Random.Range(0, backgroundRenders.Count);
        videoPlayer.clip = backgroundRenders[chosenBackground];
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
        logo.SetActive(true);
        creditsGroup.SetActive(false);
        optionsGroup.SetActive(false);
        startGameGroup.SetActive(false);
    }

    public void OpenOptions()
    {
        
        mainMenuGroup.SetActive(false);
        optionsGroup.SetActive(true);
        logo.SetActive(false);
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    public void OpenStartOptions()
    {
        mainMenuGroup.SetActive(false);
        startGameGroup.SetActive(true);
    }
    
    public void StartGame(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    IEnumerator LoadSceneAsync (string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        mainMenuGroup.SetActive(false);
        startGameGroup.SetActive(false);
        loadingGroup.SetActive(true);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);            
            
            loadBar.value = progress;
            loadBarText.text = (int)(progress * 100f) + "%";

            yield return null;
        }
    }

    public void RefreshBackground()
    {
        chosenBackground = Random.Range(0, backgroundRenders.Count);
        videoPlayer.clip = backgroundRenders[chosenBackground];
    }

    public void ClickSound()
    {
        AudioManager.instance.Play("MenuClick");
    }
}
