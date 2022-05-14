using Bolt;
using Ludiq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LukesScripts.Blueprints
{
    [UnitTitle("Reference Player")]
    [UnitSubtitle("Our player reference")]
    public class ReferencePlayer : Unit
    {
        [DoNotSerialize] public ControlInput inTrigger;

        [DoNotSerialize] public ControlOutput outTrigger;
        [DoNotSerialize] public ValueOutput output;

        private GameObject result;

        protected override void Definition()
        {
            inTrigger = ControlInput("", (flow) =>
            {
                result = WeaponManager.instance.player;
                return outTrigger;
            });

            outTrigger = ControlOutput("");
            output = ValueOutput<GameObject>("Player Reference", (flow) => result);

            Succession(inTrigger, outTrigger);
            Assignment(inTrigger, output);
        }
    }
}
