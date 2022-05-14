using Bolt;
using Ludiq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LukesScripts.Blueprints
{
    [UnitTitle("AI Attack")]
    [UnitSubtitle("Trigger an AI Attack")]
    public class AIAttack : Unit
    {
        [DoNotSerialize] public ControlInput inTrigger;
        [DoNotSerialize] public ValueInput AiObject;

        [DoNotSerialize] public ControlOutput outTrigger;

        private GameObject result;

        protected override void Definition()
        {
            inTrigger = ControlInput("", (flow) =>
            {
                result = flow.GetValue<GameObject>(AiObject);
                AI.AI ai = result.GetComponent<AI.AI>();
                ai.DoAttack();
                return outTrigger;
            });

            outTrigger = ControlOutput("");
            AiObject = ValueInput<GameObject>("AI Object");

            Requirement(AiObject, inTrigger);
            Succession(inTrigger, outTrigger);
        }
    }
}
