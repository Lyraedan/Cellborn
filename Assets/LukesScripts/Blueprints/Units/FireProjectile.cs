using Bolt;
using Ludiq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LukesScripts.Blueprints
{
    [UnitTitle("Fire Projectile")]
    [UnitSubtitle("Trigger ProjectileBehaviour FireProjectile(dir)")]
    public class FireProjectile : Unit
    {
        [DoNotSerialize] public ControlInput inTrigger;
        [DoNotSerialize] public ValueInput projectile;

        [DoNotSerialize] public ControlOutput outTrigger;
        [DoNotSerialize] public ValueOutput output;

        private GameObject result;

        protected override void Definition()
        {
            inTrigger = ControlInput("", (flow) =>
            {
                var targetDistance = Vector3.Distance(WeaponManager.instance.player.transform.position, WeaponManager.instance.target.transform.position);
                result = flow.GetValue<GameObject>(projectile);
                result.GetComponent<ProjectileBehaviour>().FireProjectile(targetDistance * 2);
                return outTrigger;
            });

            outTrigger = ControlOutput("");
            projectile = ValueInput<GameObject>("Projectile GameObject");

            output = ValueOutput<GameObject>("Instantiated Projectile", (flow) => result);

            Requirement(projectile, inTrigger);
            Succession(inTrigger, outTrigger);
            Assignment(inTrigger, output);
        }
    }
}
