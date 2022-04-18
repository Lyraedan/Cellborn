using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LukesScripts.AI
{
    public class IsInRangeOfMuffin : Conditional
    {
        public Operation operation = Operation.GreaterThanOrEqualTo;
        public float threashold = 5f;

        public override TaskStatus OnUpdate()
        {
            var nearest = GetNearestMuffin();
            if(nearest != null)
            {
                float distance = Vector3.Distance(nearest.transform.position, transform.position);
                Debug.Log("Distance to nearest muffin: " + Mathf.RoundToInt(distance));
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
            }
            return TaskStatus.Failure;
        }

        public GameObject GetNearestMuffin()
        {
            var muffins = GameObject.FindObjectsOfType<Muffin>().ToList();
            if (muffins.Count > 0)
            {
                muffins.Sort((a, b) =>
                {
                    return a.DistanceFrom(transform.position).CompareTo(b.DistanceFrom(transform.position));
                });
                return muffins[0].gameObject;
            }
            else
                return null;
        }
    }
}
