using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class EndingCutscene : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string sceneName;

    void Start()
    {
        videoPlayer.loopPointReached += ReturnScene;
    }
    
    void ReturnScene (VideoPlayer vp)
    {
        SceneManager.LoadScene(sceneName);
    }
}
