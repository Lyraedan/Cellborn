using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LightCulling
{
    public Light[] sources;

    public void Cull(bool state)
    {
        if (sources.Length <= 0)
            return;

        for(int i = 0; i < sources.Length; i++)
        {
            sources[i].enabled = state;
        }
    }
}
