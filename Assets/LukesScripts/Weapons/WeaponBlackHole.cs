using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBlackHole : WeaponBase
{

    public override void Init()
    {
    }

    public override void Tick()
    {
    }
    public override void Fire()
    {
        SpawnProjectile(1, 0);
    }

}
