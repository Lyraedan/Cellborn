using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{

    public Action OnTriggered;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Teleporter triggered by " + other.tag + " -> " + other.name);
        if(other.CompareTag("Player"))
            OnTriggered?.Invoke();
    }
}
