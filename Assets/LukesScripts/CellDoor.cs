using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellDoor : MonoBehaviour
{
    public Rigidbody doorRigidbody;
    public float disappearTime;
    IEnumerator coroutine;

    public void OnCollisionEnter(Collision collision)
    {
        var collObj = collision.gameObject;

        if (collObj.tag == "Projectile")
        {
            doorRigidbody.constraints = RigidbodyConstraints.None;
            coroutine = DoorVanish(disappearTime);
            StartCoroutine(coroutine);
        }
    }

    IEnumerator DoorVanish(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
