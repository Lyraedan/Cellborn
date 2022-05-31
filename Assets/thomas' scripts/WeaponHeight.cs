using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHeight : MonoBehaviour
{ 
    public float height = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(transform.position.x, height, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
