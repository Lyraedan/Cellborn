using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileFire : MonoBehaviour
{
    public GameObject projectile;
    public Transform projectileSpawn;
    public GameObject target;

    float yRot;

    void Update()
    {
        projectileSpawn.transform.LookAt(target.transform);
        yRot = projectileSpawn.transform.rotation.y;
        yRot = Mathf.Clamp(projectileSpawn.transform.rotation.y, -80f, 80f);
        projectileSpawn.transform.localRotation = Quaternion.Euler(transform.rotation.x, yRot, transform.rotation.z);
        
        if (Input.GetButtonDown("Fire1"))
        {
            GameObject projInstance = Instantiate(projectile, projectileSpawn.position, projectileSpawn.rotation);
        }
    }
}
