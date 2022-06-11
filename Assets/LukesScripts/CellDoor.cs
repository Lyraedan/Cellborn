using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellDoor : MonoBehaviour
{
    public Rigidbody doorRigidbody;
    public float disappearTime;
    IEnumerator coroutine;
    public GameObject smokeEffect;

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
        var smoke = Instantiate(smokeEffect, new Vector3(gameObject.transform.position.x + -0.5f, 
                                                         gameObject.transform.position.y, 
                                                         gameObject.transform.position.z), Quaternion.identity);
        smoke.transform.localScale = new Vector3(2f, 2f, 2f);
        Destroy(gameObject);
    }
}
