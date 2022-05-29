using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionHeight : MonoBehaviour
{
      public GameObject potion; 
    public float Height = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
         if (potion.transform.position.y < Height)
        {
            potion.transform.position = new Vector3(potion.transform.position.x, Height, potion.transform.position.z);
        }
    }
}
