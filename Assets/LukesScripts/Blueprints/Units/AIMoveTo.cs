using Bolt;
using Ludiq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LukesScripts.Blueprints
{
    [UnitTitle("AIMoveTo")]
    [UnitSubtitle("Make the AI Object look at the player")]
    public class AIMoveTo : Unit
    {
        [DoNotSerialize] public ControlInput inTrigger;
        [DoNotSerialize] public new ValueInput position;
        [DoNotSerialize] public ValueInput AiObject;

        [DoNotSerialize] public ControlOutput outTrigger;

        private GameObject result;
        private Vector3 point;

        protected override void Definition()
        {
            inTrigger = ControlInput("", (flow) =>
            {
                point = flow.GetValue<Vector3>(position);
                result = flow.GetValue<GameObject>(AiObject);
                AI.AI ai = result.GetComponent<AI.AI>();
                ai.MoveTo(point);
                return outTrigger;
            });

            outTrigger = ControlOutput("");
            position = ValueInput<Vector3>("Position");
            AiObject = ValueInput<GameObject>("AI Object");

            Requirement(position, inTrigger);
            Requirement(AiObject, inTrigger);
            Succession(inTrigger, outTrigger);
        }
    }
}
