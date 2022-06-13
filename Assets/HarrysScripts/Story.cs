using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Story : MonoBehaviour
{
    public List<Sprite> storyPicturesKeyboard, storyPicturesController;
    public Image pictureKeyboard, pictureController;

    int chosenPic = 0;

    public GameObject storyGroup, loadingGroup;
    public TextMeshProUGUI loadBarText;
    public string dungeonSceneName;

    bool isLoading = false;

    public AudioSource source, snake, fairy;
    public AudioClip pageTurn, enemyDeathBurst, snakeDeath, fairyDeath, storyMusic;

    void Start()
    {
        MusicManager.instance.source.clip = storyMusic;
        MusicManager.instance.source.Play();
    }

    void Update()
    {
        if (Input.anyKeyDown && chosenPic <= storyPicturesKeyboard.Count && !isLoading)
        {
            source.clip = pageTurn;
            source.Play();
            chosenPic++;

            if(chosenPic == 3)
            {
                source.clip = enemyDeathBurst;
                source.Play();
                snake.clip = snakeDeath;
                snake.Play();
                fairy.clip = fairyDeath;
                fairy.Play();
            }
        }

        if (chosenPic < storyPicturesKeyboard.Count)
        {
            pictureKeyboard.sprite = storyPicturesKeyboard[chosenPic];
            pictureController.sprite = storyPicturesController[chosenPic];
        }

        if (chosenPic == storyPicturesKeyboard.Count && !isLoading)
        {
            isLoading = true;
            LoadGame(dungeonSceneName);
        }

        /* if (ControlManager.ControllerConnected)
        {
            pictureController.enabled = true;
            pictureKeyboard.enabled = false;
        }
        else
        {
            pictureKeyboard.enabled = true;
            pictureController.enabled = false;
        } */
    }

    void LoadGame(string sceneName)
    {
        storyGroup.SetActive(false);
        loadingGroup.SetActive(true);
        
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
}
