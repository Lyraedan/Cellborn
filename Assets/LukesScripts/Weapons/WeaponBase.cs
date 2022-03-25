using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public bool infiniteAmmo = false;
    public bool isInPlayerInventory = false;
    public GameObject projectile;
    protected float targetDistance;

    // Shooting
    public float angle = 0;
    public float yRot {
        get {
            return (WeaponManager.instance.player.transform.eulerAngles.y - (angle / 2));
        }
    }

    // Shooting delay
    public float secondsBetweenShots = 0.5f;
    protected bool canFire = true;
    protected float timer = 0;

    void Start()
    {
        Init();
        timer = secondsBetweenShots;
    }

    void Update()
    {
        if (!isInPlayerInventory)
            return;

        targetDistance = Vector3.Distance(WeaponManager.instance.player.transform.position, WeaponManager.instance.target.transform.position);
        if (!canFire)
        {
            timer += 1f * Time.deltaTime;
        }
        canFire = timer > secondsBetweenShots;
        Tick();
    }

    public void Shoot(System.Action<bool> afterShot)
    {
        if (canFire)
        {
            Fire();
            timer = 0;
            canFire = false;
            afterShot?.Invoke(true);
        } else
        {
            //Debug.LogError("Halt in the name of the firing law!");
            afterShot?.Invoke(false);
        }
    }

    public abstract void Init();
    public abstract void Tick();
    public abstract void Fire();
}
