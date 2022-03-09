using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public bool infiniteAmmo = false;
    public GameObject projectile;
    protected float targetDistance;

    void Start()
    {
        Init();
    }

    void Update()
    {
        targetDistance = Vector3.Distance(WeaponManager.instance.player.transform.position, WeaponManager.instance.target.transform.position);
        Tick();
    }

    public abstract void Init();
    public abstract void Tick();
    public abstract void Fire();

    protected void SpawnProjectile(int shots, float angle)
    {
        for (int i = 0; i < shots; i++)
        {
            float y = ((WeaponManager.instance.player.transform.eulerAngles.y - (angle / 2)) + ((angle / ((shots + 1)) * (i + 1))));

            GameObject proj = Instantiate(projectile, WeaponManager.instance.firepoint.transform.position, WeaponManager.instance.player.transform.rotation);
            //GameObject projInstance = Instantiate(projectile, WeaponManager.instance.firepoint.transform.position, Quaternion.Euler(0, y, 0));
            //projInstance.transform.LookAt(target.transform);
            proj.GetComponent<ProjectileBehaviour>().FireProjectile(targetDistance * 2);
        }
    }
}
