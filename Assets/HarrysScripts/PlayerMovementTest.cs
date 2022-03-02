using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementTest : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam;
    public float speed;

    float horizontal, vertical;

    public Transform groundCheck;
    public float groundDistance;
    public LayerMask groundMask;
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
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if(!isGrounded && velocity.y <= 0)
        {
            velocity.y = -2f;
        }

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        Vector3 rightMovement = right * speed * Time.deltaTime * horizontal;
        Vector3 upMovement = forward * speed * Time.deltaTime * vertical;

        Vector3 heading = Vector3.Normalize(rightMovement + upMovement);

        controller.Move(heading * speed * Time.deltaTime);

        velocity += Physics.gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}
