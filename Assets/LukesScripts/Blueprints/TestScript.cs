using System.Collections;
using System.Collections.Generic;
using Bolt;
using Ludiq;
using LukesScripts.Blueprints;
using UnityEngine;
using EventHooks = LukesScripts.Blueprints.EventHooks;

public class TestScript : TestEventClass
{
    public FlowMachine blueprint;
    private FlowGraph graph;

    private void Awake()
    {
        graph = blueprint.DefaultGraph();
        /*EventBus.Register<EmptyEventArgs>(new EventHook(EventHooks.TestEvent, this), args => {
            Test();
        });*/
    }

    private void Update()
    {
        //EventBus.Trigger(EventHooks.TestEvent, gameObject);
        CustomEvent.Trigger(gameObject, "Test");
    }

    public override void Test()
    {
        Debug.Log("Test script");
        // Trigger Bolt Node here
    }

    void TriggerEvent(string name)
    {
        //EventBus.Trigger(new EventHook(name, this), new EmptyEventArgs());
    }
}

public abstract class TestEventClass : MonoBehaviour
{
    public abstract void Test();
}