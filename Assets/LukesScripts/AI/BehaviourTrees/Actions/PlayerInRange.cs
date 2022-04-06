using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInRange : Conditional
{
    public float threashold = 5f;

    public override TaskStatus OnUpdate()
    {
        if (Vector3.Distance(WeaponManager.instance.player.transform.position, transform.position) < threashold)
            return TaskStatus.Success;
        else
            return TaskStatus.Failure;
    }
}
