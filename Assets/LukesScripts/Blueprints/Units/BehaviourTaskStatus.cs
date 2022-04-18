using BehaviorDesigner.Runtime.Tasks;
using Bolt;
using Ludiq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LukesScripts.Blueprints
{
    [UnitTitle("Behaviour tree TaskStatus")]
    public class BehaviourTaskStatus : Unit
    {
        [DoNotSerialize] public ControlInput inTrigger;
        [DoNotSerialize] public ControlOutput outTrigger;

        [DoNotSerialize] public ValueInput statusInput;
        [DoNotSerialize] public ValueOutput statusOutput;

        private TaskStatus status;

        protected override void Definition()
        {
            inTrigger = ControlInput("", (flow) =>
            {
                status = flow.GetValue<TaskStatus>(statusInput);
                return outTrigger;
            });

            outTrigger = ControlOutput("");

            statusInput = ValueInput<TaskStatus>("Set Status");

            statusOutput = ValueOutput<TaskStatus>("Status", (flow) => status);

            Requirement(statusInput, inTrigger);
            Succession(inTrigger, outTrigger);
            Assignment(inTrigger, statusOutput);
        }
    }
}
