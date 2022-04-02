using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Acid : MonoBehaviour
{
    public int playerDamage;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Acid Enter");
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("Acid Exit");
    }

    private void OnCollisionStay(Collision collision)
    {
        var collObj = collision.gameObject;
        Debug.Log("Acid Collision");

        if (collObj.CompareTag("Player"))
        {
            Debug.Log("Acid Damage");
            PlayerStats.instance.currentHP -= playerDamage;
        }
    }
}