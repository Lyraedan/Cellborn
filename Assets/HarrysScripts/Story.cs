using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Story : MonoBehaviour
{
    public List<Sprite> storyPictures;
    public Image picture;

    int chosenPic = 0;

    public GameObject storyGroup, loadingGroup;
    public Slider loadBar;
    public TextMeshProUGUI loadBarText;
    public string dungeonSceneName;

    bool isLoading = false;

    void Update()
    {
        if (Input.anyKeyDown && chosenPic <= storyPictures.Count && !isLoading)
        {
            AudioManager.instance.Play("PageTurn");
            chosenPic++;

            if(chosenPic == 3)
            {
                AudioManager.instance.Play("EnemyDeathBurst");
                AudioManager.instance.Play("SnakeDeath");
                AudioManager.instance.Play("FairyDeath");
            }
        }

        if (chosenPic < storyPictures.Count)
        {
            picture.sprite = storyPictures[chosenPic];
        }

        if (chosenPic == storyPictures.Count && !isLoading)
        {
            isLoading = true;
            LoadGame(dungeonSceneName);
        }
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
            
            loadBar.value = progress;
            loadBarText.text = (int)(progress * 100f) + "%";

            yield return null;
        }
    }
}
