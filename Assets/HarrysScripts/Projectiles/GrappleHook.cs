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

    public float timer = 0;

    [SerializeField]GameObject pulledObject;
    [SerializeField]CharacterController playerController;
    [SerializeField]string collisionTag;
    [SerializeField]bool isPulling, isPhysObj;

    public Material cordMaterial;
    public float cordWidth = 0.05f;
    public Color ropeColor = Color.red;
    private LineRenderer cord;
   
    void Start()
    {
        playerController = FindObjectOfType<PlayerMovementTest>().controller;
        managerInstance = FindObjectOfType<WeaponManager>();
        playerObject = FindObjectOfType<PlayerMovementTest>().gameObject;

        cord = gameObject.AddComponent<LineRenderer>();
        cord.material = cordMaterial;
    }

    void Update()
    {
        timer += 1f * Time.deltaTime;

        playerDistance = playerObject.transform.position - this.transform.position;

        DrawLine(playerObject.transform.position, transform.position, ropeColor);

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
            else
            {
                //PullPlayer(playerObject);

                forceDirection = transform.position - playerObject.transform.position;
                PlayerMovementTest.instance.disableMovement = true;
                playerController.Move(forceDirection * playerPull * Time.deltaTime);
            }
        }
    }        
    
    void OnCollisionEnter(Collision other)
    {
        if (!other.transform.CompareTag("Roof"))
        {
            collisionTag = other.gameObject.tag;
            pulledObject = other.gameObject;
            projRigidbody.useGravity = false;
            projRigidbody.velocity = Vector3.zero;
            projRigidbody.constraints = RigidbodyConstraints.FreezeAll;
            gameObject.transform.SetParent(other.gameObject.transform);
            isPulling = true;
            if (other.gameObject.GetComponent<Rigidbody>())
            {
                isPhysObj = true;
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

    void RetrieveHook()
    {
        WeaponProperties grappleWeapon;

        timer = 0;

        projRigidbody.GetComponent<Rigidbody>().useGravity = true;
        projRigidbody.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

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

        PlayerMovementTest.instance.disableMovement = false;
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
