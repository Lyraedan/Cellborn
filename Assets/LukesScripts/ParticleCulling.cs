using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ParticleCulling
{
    public ParticleSystem[] emitters;

    public void Cull(bool state)
    {
        if (emitters.Length <= 0)
            return;

        for (int i = 0; i < emitters.Length; i++)
        {
            emitters[i].gameObject.SetActive(state);
        }
    }
}
