using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialGenerator : MonoBehaviour
{

    public Material reference;
    public new Renderer renderer;


    void Start()
    {
        Material material = new Material(reference.shader);
        material.CopyPropertiesFromMaterial(reference);
        renderer.material = material;
        Debug.Log("Generated material");
    }

}
