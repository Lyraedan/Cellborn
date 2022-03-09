using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideWall : MonoBehaviour
{
    public MeshRenderer wall;
    private bool fadeOut, fadeIn;
    [SerializeField] private float fadeSpeed = 1.5f;

    public void Hide()
    {
        Debug.Log("Do fade!");
        fadeOut = true;

        while(fadeOut)
        {
            Debug.Log("Fading!");
            Color color = wall.material.color;
            float fadeAmount = color.a - (fadeSpeed * Time.deltaTime);

            color = new Color(color.r, color.g, color.b, fadeAmount);
            wall.material.color = color;

            if (color.a <= 0)
            {
                Debug.Log("Faded!");
                fadeOut = false;
                break;
            }
        }
    }

    public void Show()
    {
        Debug.Log("Do reveal!");
        fadeIn = true;
        while(fadeIn)
        {
            Debug.Log("Revealing!");
            Color color = wall.material.color;
            float fadeAmount = color.a + (fadeSpeed * Time.deltaTime);

            color = new Color(color.r, color.g, color.b, fadeAmount);
            wall.material.color = color;

            if (color.a >= 1)
            {
                Debug.Log("Revealed!");
                fadeIn = false;
                break;
            }
        }
    }
}
