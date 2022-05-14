using Bolt;
using Ludiq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LukesScripts.Blueprints
{
    [UnitTitle("AI Look at Player")]
    [UnitSubtitle("Make the AI Object look at the player")]
    public class AILookAtPlayer : Unit
    {
        [DoNotSerialize] public ControlInput inTrigger;
        [DoNotSerialize] public ValueInput AiObject;

        [DoNotSerialize] public ControlOutput outTrigger;

        private GameObject result;

        protected override void Definition()
        {
            inTrigger = ControlInput("", (flow) =>
            {
                var player = WeaponManager.instance.player.transform.position;
                result = flow.GetValue<GameObject>(AiObject);
                AI.AI ai = result.GetComponent<AI.AI>();
                ai.LookAt(player);
                return outTrigger;
            });

            outTrigger = ControlOutput("");
            AiObject = ValueInput<GameObject>("AI Object");

            Requirement(AiObject, inTrigger);
            Succession(inTrigger, outTrigger);
        }
    }
}
