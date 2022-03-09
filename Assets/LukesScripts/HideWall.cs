using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideWall : MonoBehaviour
{
    public MeshRenderer wall;
    public bool visible => wall.enabled;

    public void Hide()
    {
        wall.enabled = false;
    }

    public void Show()
    {
        wall.enabled = true;
    }
}
