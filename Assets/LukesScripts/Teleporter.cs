using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{

    public Action OnTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
            OnTriggered?.Invoke();
    }
}
