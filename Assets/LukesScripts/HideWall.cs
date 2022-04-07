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
        fadeOut = true;

        while(fadeOut)
        {
            Color color = wall.material.color;
            float fadeAmount = color.a - (fadeSpeed * Time.deltaTime);

            color = new Color(color.r, color.g, color.b, fadeAmount);
            wall.material.color = color;

            if (color.a <= 0)
            {
                fadeOut = false;
                break;
            }
        }
    }

    public void Show()
    {
        fadeIn = true;
        while(fadeIn)
        {
            Color color = wall.material.color;
            float fadeAmount = color.a + (fadeSpeed * Time.deltaTime);

            color = new Color(color.r, color.g, color.b, fadeAmount);
            wall.material.color = color;

            if (color.a >= 1)
            {
                fadeIn = false;
                break;
            }
        }
    }
}
