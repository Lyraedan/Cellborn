using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMovement : MonoBehaviour
{
    public float height = -0.45f;

    void Update()
    {
        if (!PauseMenu.isPaused)
        {
            Plane plane = new Plane(Vector3.up, -height);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(plane.Raycast(ray, out float distanceToPlane))
            {
                transform.position = ray.GetPoint(distanceToPlane);
            }
        }
    }
}
