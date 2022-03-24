using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public CharacterController controller;
    public Transform cam;
    public float speed;

    float horizontal, vertical;

    public float gravity;
    public Transform groundCheck;
    public float groundDistance;
    public LayerMask groundMask;
    public Vector3 movingDirection = Vector3.zero;
    [SerializeField]bool isGrounded;

    Vector3 forward, right, velocity;

    private void Start()
    {
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;
    }

    void Update()
    {
        if (WeaponManager.instance == null)
            return;

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if(isGrounded && velocity.y <= 0)
        {
            velocity.y = -2f;
        }

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        Vector3 rightMovement = right * speed * Time.deltaTime * horizontal;
        Vector3 upMovement = forward * speed * Time.deltaTime * vertical;

        Vector3 heading = Vector3.Normalize(rightMovement + upMovement);

        movingDirection = heading * speed * Time.deltaTime;
        controller.Move(movingDirection);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        //Rotate player
        var direction = WeaponManager.instance.target.transform.position - transform.position;
        direction.y = 0;
        var rotation = Quaternion.LookRotation(direction);
        var dampening = 8;
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * dampening);
    }
}
