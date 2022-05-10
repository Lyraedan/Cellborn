using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoBall : MonoBehaviour
{
    public float ballRotateSpeed;
    public float colourChangeSpeed;

    public Light dotsLight;

    //Colour
    public float color_h, color_s, color_v;
    
    // Start is called before the first frame update
    void Start()
    {
        color_s = 1.0f;
        color_v = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * ballRotateSpeed * Time.deltaTime);
        color_h += 1.0f / 360.0f * colourChangeSpeed;
        dotsLight.color = Color.HSVToRGB(color_h, color_s, color_v);

        if (color_h > 1.0f)
        {
            color_h = 0.0f;
        }
    }
}
