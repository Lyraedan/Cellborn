using Bolt;
using Ludiq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LukesScripts.Blueprints
{
    [UnitTitle("Hurt Player")]
    [UnitSubtitle("Inflict damage upon the player")]
    public class HurtPlayer : Unit
    {
        [DoNotSerialize] public ControlInput inTrigger;
        [DoNotSerialize] public ValueInput damage;

        [DoNotSerialize] public ControlOutput outTrigger;

        private int result;

        protected override void Definition()
        {
            inTrigger = ControlInput("", (flow) =>
            {
                result = flow.GetValue<int>(damage);
                PlayerStats.instance.DamagePlayer(result);
                return outTrigger;
            });

            outTrigger = ControlOutput("");
            damage = ValueInput<int>("Damage Amount");

            Requirement(damage, inTrigger);
            Succession(inTrigger, outTrigger);
        }
    }
}
