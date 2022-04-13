using System;
using Unity;
using UnityEngine;
using Bolt;
using Ludiq;

[IncludeInSettings(true)]
[UnitTitle("TestUnit")]
[UnitSubtitle("Multiply A x B x C")]
[TypeIcon(typeof(Multiply<float>))]

public class TestUnit : Unit
{
    [DoNotSerialize] public ControlInput inTrigger;
    [DoNotSerialize] public ValueInput inA;
    [DoNotSerialize] public ValueInput inB;
    [DoNotSerialize] public ValueInput inC;

    [DoNotSerialize] public ControlOutput outTrigger;
    [DoNotSerialize] public ValueOutput outA;

    private float result;

    protected override void Definition()
    {
        inTrigger = ControlInput("", (flow) =>
        {
            result = flow.GetValue<float>(inA) * flow.GetValue<float>(inB) * flow.GetValue<float>(inC);
            return outTrigger;
        });

        outTrigger = ControlOutput("");

        inA = ValueInput<float>("A");
        inB = ValueInput<float>("B");
        inC = ValueInput<float>("C");

        outA = ValueOutput<float>("A x B x C", (flow) => result);

        Requirement(inA, inTrigger);
        Requirement(inB, inTrigger);
        Requirement(inC, inTrigger);
        Succession(inTrigger, outTrigger);
        Assignment(inTrigger, outA);
    }
}