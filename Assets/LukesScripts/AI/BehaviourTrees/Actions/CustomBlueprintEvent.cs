using BehaviorDesigner.Runtime.Tasks;
using Bolt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LukesScripts.AI
{
    public class CustomBlueprintEvent : Action
    {
        public enum EventCalled
        {
            OnStart, OnUpdate, OnReset, OnBehaviourCompleted
        }
        public string hookName;
        public EventCalled callIn;
        public CustomVariable[] arguments;

        public override void OnStart()
        {
            if (callIn.Equals(EventCalled.OnStart))
                CustomEvent.Trigger(gameObject, hookName, ArgumentsToParameters());
        }

        public override TaskStatus OnUpdate()
        {
            if (callIn.Equals(EventCalled.OnUpdate))
                CustomEvent.Trigger(gameObject, hookName, ArgumentsToParameters());
            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            if (callIn.Equals(EventCalled.OnReset))
                CustomEvent.Trigger(gameObject, hookName, ArgumentsToParameters());
        }

        public override void OnBehaviorComplete()
        {
            if (callIn.Equals(EventCalled.OnBehaviourCompleted))
                CustomEvent.Trigger(gameObject, hookName, ArgumentsToParameters());
        }

        public object[] ArgumentsToParameters()
        {
            if (arguments.Length > 0)
            {
                List<object> parameters = new List<object>();
                for (int i = 0; i < arguments.Length; i++)
                {
                    parameters.Add(arguments[i].GetValue());
                }
                return parameters.ToArray();
            }
            else // Empty arguments
                return new object[] { 0 };
        }
    }

    [System.Serializable]
    public class CustomVariable {
        public enum DataType
        {
            STRING, INT, FLOAT, BOOL, VECTOR3
        }

        public DataType dataType = DataType.STRING;
        public string value = string.Empty;

        public object GetValue()
        {
            switch(dataType)
            {
                case DataType.STRING:
                    return value;
                case DataType.INT:
                    return float.Parse(value);
                case DataType.FLOAT:
                    return float.Parse(value);
                case DataType.BOOL:
                    return bool.Parse(value);
                case DataType.VECTOR3:
                    float x = float.Parse(value.Split(',')[0]);
                    float y = float.Parse(value.Split(',')[1]);
                    float z = float.Parse(value.Split(',')[2]);
                    return new Vector3(x, y, z);
                default:
                    return value;
            }
        }
    }
}
