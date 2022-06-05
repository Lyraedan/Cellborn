using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JunkCollector : MonoBehaviour
{

    public float threashold = -5f;
    public float aboveThreashold = 1.5f;

    void Update()
    {
        if(transform.position.y < threashold)
        {
            Debug.Log("Yeeeeeting " + gameObject.name + " from existance");
            Destroy(this);
        }

        if (transform.position.y > aboveThreashold)
        {
            Debug.Log("Yeeeeeting " + gameObject.name + " from existance");
            Destroy(this);
        }
    }
}
