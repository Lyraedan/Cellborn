using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject player;
    public Vector3 distanceFrom = new Vector3(0.5f, -2.5f, 4.5f);

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 pos = player.transform.position;
            pos.x -= distanceFrom.x;
            pos.y -= distanceFrom.y;
            pos.z -= distanceFrom.z;
            Camera.main.transform.position = pos;
        }
    }
}
