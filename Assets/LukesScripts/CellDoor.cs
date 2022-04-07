using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellDoor : MonoBehaviour
{
    public Rigidbody doorRigidbody;

    public void OnCollisionEnter(Collision collision)
    {
        var collObj = collision.gameObject;

        if (collObj.tag == "Projectile")
        {
            doorRigidbody.constraints = RigidbodyConstraints.None;
        }
    }
}
