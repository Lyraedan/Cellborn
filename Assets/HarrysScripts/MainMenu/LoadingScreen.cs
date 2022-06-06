using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public TextMeshProUGUI loadBarText;
    public string scene;
    
    void Start()
    {
        ChangeScene(scene);
    }

    public void ChangeScene(string sceneName)
    {
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
