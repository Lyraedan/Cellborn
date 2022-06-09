using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GrappleHook : ProjectileBehaviour
{
    public GameObject playerObject;

    public float playerPull, enemyPull;

    public SphereCollider triggerCollider;

    public Vector3 forceDirection;
    private Vector3 playerDistance;

    public float timer = 0;

    [SerializeField]GameObject pulledObject;
    [SerializeField]CharacterController playerController;
    [SerializeField]string collisionTag;
    [SerializeField]bool isPhysObj;
    public bool isPulling;

    public Material cordMaterial;
    public float cordWidth = 0.05f;
    public Color ropeColor = Color.red;
    private LineRenderer cord;
   
    void Start()
    {
        playerController = FindObjectOfType<PlayerMovementTest>().controller;
        playerObject = FindObjectOfType<PlayerMovementTest>().gameObject;

        cord = gameObject.AddComponent<LineRenderer>();
        cord.material = cordMaterial;
    }

    void Update()
    {
        timer += 1f * Time.deltaTime;

        playerDistance = playerObject.transform.position - this.transform.position;

        DrawLine(WeaponManager.instance.grappleFirepoint.transform.position, transform.position, ropeColor);

        if (!WeaponManager.instance.HasWeaponInInventory(4))
        {
            RetrieveHook();
        }

        if(WeaponManager.instance.currentlyHeldWeapons[WeaponManager.instance.currentlySelectedIndex].weaponId != 4)
        {
            RetrieveHook();
        }

        if (isPulling && playerDistance.magnitude < 0.9)
        {
            RetrieveHook();
        }

        if (timer > 2)
        {
            RetrieveHook();
        }
    }

    void FixedUpdate()
    {
        if (isPulling)
        {
            if (isPhysObj)
            {
                //other.gameObject.GetComponent<EnemyScript>().currentHP -= enemyDamage;
                //PullEnemy(playerObject, pulledObject);

                forceDirection = playerObject.transform.position - transform.position;

                try
                {
                    if (pulledObject != null)
                    {
                        Rigidbody rigidBody = pulledObject.GetComponent<Rigidbody>();
                        rigidBody.AddForce(forceDirection * enemyPull * Time.deltaTime);
                    }
                } catch(System.Exception e)
                {
                    pulledObject = null;
                }
            }
            forceDirection = transform.position - playerObject.transform.position;
            PlayerMovementTest.instance.disableMovement = true;
            playerController.Move(forceDirection * playerPull * Time.deltaTime);
        }
    }        
    
    void OnCollisionEnter(Collision other)
    {
        if (!other.transform.CompareTag("Roof"))
        {
            /*if (other.transform.CompareTag("Enemy"))
            {
                RetrieveHook();
                return;
            }
            */

            collisionTag = other.gameObject.tag;
            pulledObject = other.gameObject;
            projRigidbody.useGravity = false;
            projRigidbody.velocity = Vector3.zero;
            projRigidbody.constraints = RigidbodyConstraints.FreezeAll;
            gameObject.transform.SetParent(other.gameObject.transform);
            isPulling = true;
            FindObjectOfType<GrappleOffence>().isAttacking = true;
            if (other.gameObject.GetComponent<Rigidbody>())
            {
                isPhysObj = true;
            }

            if (other.transform.CompareTag("Weapon"))
            {
                playerObject.GetComponent<WeaponManager>().Pickup(other.gameObject.GetComponent<WeaponProperties>());
                RetrieveHook();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerMovementTest>())
        {
            if (t >= 1f)
            {
                RetrieveHook();
            }      
        }
    }

    public void RetrieveHook()
    {
        WeaponProperties grappleWeapon;

        timer = 0;

        projRigidbody.GetComponent<Rigidbody>().useGravity = true;
        projRigidbody.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

        for (int i = 0; i < WeaponManager.instance.currentlyHeldWeapons.Count; i++)
        {
            if (WeaponManager.instance.currentlyHeldWeapons[i].weaponId == 4)
            {
                grappleWeapon = WeaponManager.instance.currentlyHeldWeapons[i];
                grappleWeapon.SetAmmo(1);
                WeaponManager.instance.ammoText.text = grappleWeapon.currentAmmo + " / " + grappleWeapon.maxAmmo;
                break;
            }
        }

        PlayerMovementTest.instance.disableMovement = false;
        FindObjectOfType<GrappleOffence>().isAttacking = false;
        Destroy(gameObject);
    }

    void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        cord.startColor = color;
        cord.endColor = color;

        cord.startWidth = cordWidth;
        cord.endWidth = cordWidth;

        cord.SetPosition(0, start);
        cord.SetPosition(1, end);

    }
}
