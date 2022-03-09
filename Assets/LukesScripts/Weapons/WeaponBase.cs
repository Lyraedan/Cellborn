using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public bool infiniteAmmo = false;
    public GameObject projectile;
    protected float targetDistance;

    private GameObject player, target;

    void Start()
    {
        Init();
    }

    void Update()
    {
        if (target == null)
        {
            target = GameObject.Find("Target");
            return;
        }

        if(player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            return;
        }

        targetDistance = Vector3.Distance(player.transform.position, target.transform.position);

        player.transform.LookAt(target.transform.position);
        player.transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

        Tick();
    }

    public abstract void Init();
    public abstract void Tick();
    public abstract void Fire();

    protected void SpawnProjectile(int shots, float angle)
    {
        if (target == null)
        {
            target = GameObject.Find("Target");
            return;
        }

        for (int i = 0; i < shots; i++)
        {
            float y = ((transform.eulerAngles.y - (angle / 2)) + ((angle / ((shots + 1)) * (i + 1))));

            Vector3 pos = transform.position;
            pos.y += -0.1f;

            GameObject projInstance = Instantiate(projectile, WeaponManager.instance.firepoint.position, Quaternion.Euler(0, y, 0));
            projInstance.transform.LookAt(target.transform);
            projInstance.GetComponent<ProjectileBehaviour>().FireProjectile(targetDistance * 2);
        }
    }
}
