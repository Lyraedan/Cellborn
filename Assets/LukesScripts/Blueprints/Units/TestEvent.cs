using Bolt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[UnitCategory("LukesScripts/Blueprints")]
[UnitOrder(2)]
public class TestEvent : MachineEventUnit<EmptyEventArgs>
{
    protected override string hookName => "Test event";
}
