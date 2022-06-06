using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Rendering.PostProcessing;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuGroup, creditsGroup, loadingGroup, optionsGroup, startGameGroup, logo;

    public TextMeshProUGUI loadBarText;

    public VideoPlayer videoPlayer;
    public List<VideoClip> backgroundRenders;
    public GameObject background;
    int chosenBackground;

    public AudioSource source;
    public AudioClip menuClickSound;

    public AudioClip menuMusic;

    public PostProcessProfile profile;
    private ColorGrading _colorGrading;

    public GameObject splashGroup;
    public GameObject mainGroup;
    
    void Start()
    {
        chosenBackground = Random.Range(0, backgroundRenders.Count);
        videoPlayer.clip = backgroundRenders[chosenBackground];

        MusicManager.instance.source.clip = menuMusic;
        MusicManager.instance.source.Play();

        profile.TryGetSettings(out _colorGrading);
        _colorGrading.active = false;
    }

    void Update()
    {
        if (splashGroup.active)
        {
            if (Input.anyKeyDown)
            {
                SplashToMenu();
                ClickSound();
            }
        }
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
        mainMenuGroup.SetActive(false);
        startGameGroup.SetActive(false);
        loadingGroup.SetActive(true);
        background.SetActive(false);
        logo.SetActive(false);
        
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    IEnumerator LoadSceneAsync (string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);            
            
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
        source.clip = menuClickSound;
        source.Play();
    }

    public void SplashToMenu()
    {
        splashGroup.SetActive(false);
        mainGroup.SetActive(true);
    }
}
