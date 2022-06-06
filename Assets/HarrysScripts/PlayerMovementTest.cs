using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovementTest : MonoBehaviour
{
    public static PlayerMovementTest instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    [HideInInspector] public bool disableMovement = false;

    public CharacterController controller;
    public Collider playerCollider;
    public Animator animController;
    public Transform cam;
    public float speed;

    [HideInInspector] public float potionSpeedMultiplier;
    [HideInInspector] public bool isSpedUp;
    [HideInInspector] public float speedUpTime;
    float speedUpTimer;

    float horizontal, vertical;

    public float gravity;
    public Transform groundCheck;
    public float groundDistance;
    public LayerMask groundMask;
    public Vector3 movingDirection = Vector3.zero;
    [Header("Physics")]
    public bool isGrounded;
    public GameObject StoodOn;
    public bool onSlope;
    private Vector3 slopeNormal;
    public bool isSpeedPotioned = false;

    Vector3 forward, right, velocity;

    public Vector3 test;

    private void Start()
    {
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;

        potionSpeedMultiplier = 1f;
    }

    void Update()
    {
        if (WeaponManager.instance == null)
            return;

        (isGrounded, onSlope, slopeNormal, StoodOn) = CheckIsGrounded();

        if (isGrounded && velocity.y <= 0)
        {
            velocity.y = -2f;
        }

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        Vector3 rightMovement = right * speed * Time.deltaTime * horizontal;
        Vector3 upMovement = forward * speed * Time.deltaTime * vertical;

        Vector3 heading = Vector3.Normalize(rightMovement + upMovement);

        if (isSpedUp)
        {
            speedUpTimer += 1f * Time.deltaTime;

            if (speedUpTimer >= speedUpTime)
            {
                potionSpeedMultiplier = 1f;
                speedUpTime = 0f;
                speedUpTimer = 0f;
                isSpedUp = false;
            }
        }

        if (!disableMovement)
        {
            movingDirection = heading * speed * potionSpeedMultiplier * Time.deltaTime;
            Animate(movingDirection);

            controller.Move(movingDirection);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        if (WeaponManager.instance.target == null)
            return;

        //Rotate player
        var direction = WeaponManager.instance.target.transform.position - transform.position;
        direction.y = 0;
        var rotation = Quaternion.LookRotation(direction);
        var dampening = 8;
        var newRotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * dampening);
        transform.rotation = newRotation;
        test = Vector3.Cross(transform.position, WeaponManager.instance.target.transform.position);
    }

    public void Animate(Vector3 heading)
    {
        if (heading.magnitude > 1.0f)
        {
            heading = heading.normalized;
        }

        heading = transform.InverseTransformDirection(heading);

        animController.SetFloat("VelocityX", heading.x, 0.05f, Time.deltaTime);
        animController.SetFloat("VelocityZ", heading.z, 0.05f, Time.deltaTime);
    }

    public void TeleportPlayer(Vector3 position)
    {
        controller.enabled = false;
        transform.position = position;
        controller.enabled = true;
    }

    public void TeleportPlayerToRandomPoint()
    {
        float radius = 1000;
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }

        TeleportPlayer(finalPosition);
    }

    private (bool, bool, Vector3, GameObject) CheckIsGrounded()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, playerCollider.bounds.extents.y + 0.1f))
        {
            var onSlope = Vector3.Distance(hit.normal, Vector3.up) > 0f;
            var slopeAngle = hit.normal;
            var standingOn = hit.transform.gameObject;

            return (true, onSlope, slopeAngle, standingOn);
        }
        return (false, false, Vector3.zero, null);
    }
}
