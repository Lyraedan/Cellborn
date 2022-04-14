using System;
using UnityEngine;

public class Screenshot : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            ScreenCapture.CaptureScreenshot("E:\\Users\\harry\\Pictures\\screenshots\\" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".png");
            UnityEditor.AssetDatabase.Refresh();
        }
    }
}