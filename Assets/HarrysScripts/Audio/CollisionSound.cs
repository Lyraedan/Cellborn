using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionSound : MonoBehaviour
{
    public AudioSource source;

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag != "Player" || other.gameObject.tag != "Untagged")
        {
            source.Play();
        }
    }
}
