using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnHit : Composite
{
    private int currentChildIndex = 0;
    private TaskStatus executionStatus = TaskStatus.Inactive;
    private AI ai;

    public override void OnStart()
    {
        ai = GetComponent<AI>();
    }

    public override int CurrentChildIndex()
    {
        return currentChildIndex;
    }

    public override bool CanExecute()
    {
        return currentChildIndex < children.Count && executionStatus != TaskStatus.Failure && ai.isHit;
    }

    public override void OnChildExecuted(TaskStatus childStatus)
    {
        currentChildIndex++;
        executionStatus = childStatus;
    }

    public override void OnConditionalAbort(int childIndex)
    {
        currentChildIndex = childIndex;
        executionStatus = TaskStatus.Inactive;
    }

    public override void OnEnd()
    {
        executionStatus = TaskStatus.Inactive;
        currentChildIndex = 0;
    }
}