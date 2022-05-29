using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHeight : MonoBehaviour
{ 
    public GameObject weapon; 
    public float Height = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (weapon.transform.position.y < Height)
        {
            weapon.transform.position = new Vector3(weapon.transform.position.x, Height, weapon.transform.position.z);
        }
    }
}
