using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LukesScripts.AI.Actions
{
    public class PlayerInRange : Conditional
    {
        public enum Operation
        {
            LessThan,
            LessThanOrEqualTo,
            EqualTo,
            NotEqualTo,
            GreaterThanOrEqualTo,
            GreaterThan
        }

        public Operation operation = Operation.GreaterThanOrEqualTo;

        public float threashold = 5f;

        public override TaskStatus OnUpdate()
        {
            float distance = Vector3.Distance(WeaponManager.instance.player.transform.position, transform.position);
            switch (operation)
            {
                case Operation.LessThan:
                    return distance < threashold ? TaskStatus.Success : TaskStatus.Failure;
                case Operation.LessThanOrEqualTo:
                    return distance <= threashold ? TaskStatus.Success : TaskStatus.Failure;
                case Operation.EqualTo:
                    return distance == threashold ? TaskStatus.Success : TaskStatus.Failure;
                case Operation.NotEqualTo:
                    return distance != threashold ? TaskStatus.Success : TaskStatus.Failure;
                case Operation.GreaterThanOrEqualTo:
                    return distance >= threashold ? TaskStatus.Success : TaskStatus.Failure;
                case Operation.GreaterThan:
                    return distance > threashold ? TaskStatus.Success : TaskStatus.Failure;
            }
            return TaskStatus.Failure;
        }
    }
}