using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuGroup, creditsGroup, loadingGroup;

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
        creditsGroup.SetActive(false);
    }

    public void CloseGame()
    {
        Application.Quit();
    }
    
    public void StartGame(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    IEnumerator LoadSceneAsync (string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        mainMenuGroup.SetActive(false);
        loadingGroup.SetActive(true);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);            
            
            loadBar.value = progress;
            loadBarText.text = (int)(progress * 100f) + "%";

            yield return null;
        }
    }
}
