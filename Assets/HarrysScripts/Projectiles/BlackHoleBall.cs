using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHoleBall : ProjectileBehaviour
{
    public int bounces;
    public float holeSpawnYOffset;
    public GameObject blackHolePrefab;

    bool hasBounced;

    void Update()
    {
        if (bounces <= 0)
        {
            Instantiate(blackHolePrefab, new Vector3(transform.position.x, holeSpawnYOffset, transform.position.z), Quaternion.identity);
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        var collObj = other.gameObject;

        if (collObj.tag == "Enemy")
        {
            Instantiate(blackHolePrefab, new Vector3(transform.position.x, holeSpawnYOffset, transform.position.z), Quaternion.identity);
        }

        if (!hasBounced && other.gameObject.layer == 11)
        {
            bounces -= 1;
            hasBounced = true;
        }
    }

    void OnCollisionExit(Collision other)
    {
        hasBounced = false;
    }
}
