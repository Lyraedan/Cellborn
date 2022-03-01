using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject player;
    public Vector3 distanceFrom = new Vector3(1.5f, -5f, 10f);

    void Update()
    {
        if (player != null)
        {
            Vector3 pos = player.transform.position;
            pos.x -= distanceFrom.x;
            pos.y -= distanceFrom.y;
            pos.z -= distanceFrom.z;
            transform.position = pos;
        }
    }
}
