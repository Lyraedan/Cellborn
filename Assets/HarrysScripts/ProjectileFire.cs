using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileFire : MonoBehaviour
{
    public GameObject projectile;
    public Transform projectileSpawn, player, target;

    void Update()
    {                
        player.LookAt(target.position);
        player.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        if (Input.GetButtonDown("Fire1"))
        {
            GameObject projInstance = Instantiate(projectile, transform.position, transform.rotation);
        }
    }
}
