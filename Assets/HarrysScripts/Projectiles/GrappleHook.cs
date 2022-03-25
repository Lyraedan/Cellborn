using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GrappleHook : ProjectileBehaviour
{
    public GameObject playerObject;

    public WeaponManager managerInstance;

    public float playerPull, enemyPull;

    public SphereCollider triggerCollider;

    public Vector3 forceDirection;
    private Vector3 playerDistance;

    [SerializeField]GameObject pulledObject;
    [SerializeField]CharacterController playerController;
    [SerializeField]string collisionTag;
    [SerializeField]bool isPulling, isPhysObj;
   
    void Start()
    {
        playerController = FindObjectOfType<PlayerMovementTest>().controller;
        managerInstance = FindObjectOfType<WeaponManager>();
        playerObject = FindObjectOfType<PlayerMovementTest>().gameObject;
    }

    void Update()
    {
        playerDistance = playerObject.transform.position - this.transform.position;

        if (!managerInstance.HasWeaponInInventory(4))
        {
            RetrieveHook();
        }

        if(managerInstance.currentlyHeldWeapons[managerInstance.currentlySelectedIndex].weaponId != 4)
        {
            RetrieveHook();
        }

        if (isPulling && playerDistance.magnitude < 0.9)
        {
            RetrieveHook();
        }

        if (isPulling && Input.GetButton("Fire1"))
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
                        rigidBody.AddForce(forceDirection.normalized * enemyPull * Time.deltaTime);
                    }
                } catch(System.Exception e)
                {
                    pulledObject = null;
                }
            }
            else
            {
                //PullPlayer(playerObject);

                forceDirection = transform.position - playerObject.transform.position;

                playerController.Move(forceDirection.normalized * playerPull * Time.deltaTime);
            }
        }
    }        
    
    void OnCollisionEnter(Collision other)
    {
        projRigidbody.isKinematic = true;
        gameObject.transform.SetParent(other.gameObject.transform);

        collisionTag = other.gameObject.tag;
        pulledObject = other.gameObject;
        isPulling = true;

        if (other.gameObject.GetComponent<Rigidbody>())
        {
            isPhysObj = true;
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

    void RetrieveHook()
    {
        WeaponProperties grappleWeapon;

        for (int i = 0; i < managerInstance.currentlyHeldWeapons.Count; i++)
        {
            if (managerInstance.currentlyHeldWeapons[i].weaponId == 4)
            {
                grappleWeapon = managerInstance.currentlyHeldWeapons[i];
                grappleWeapon.currentAmmo = 1;
                managerInstance.ammoText.text = "Ammo: " + grappleWeapon.currentAmmo + " / " + grappleWeapon.maxAmmo;
                break;
            }
        }

        Destroy(gameObject);
    }
}
