using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebDesolve : MonoBehaviour
{
    public List<GameObject> gameObjects = new List<GameObject>();

    void Desolve()
    {
        Destroy();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Projectile"))
        {
            Desolve();
        }
    }

    void Destroy()
    {
        for(int i = 0; i < gameObjects.Count; i++)
        {
            var obj = gameObjects[i];
            gameObjects.Remove(obj);
            Destroy(obj);
        }
        RoomGenerator.instance.BakeNavmesh();
    }

}
