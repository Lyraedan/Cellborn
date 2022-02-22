using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementTest : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam;
    public float speed;

    float horizontal, vertical;
    Vector3 velocity;

    public Transform groundCheck;
    public float groundDistance;
    public LayerMask groundMask;
    [SerializeField]bool isGrounded;

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        horizontal = InputManager.horizontal;
        vertical = InputManager.vertical;
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        controller.Move(direction * speed * Time.deltaTime);

        velocity += Physics.gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}
