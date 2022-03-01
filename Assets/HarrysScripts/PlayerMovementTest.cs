using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementTest : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam;
    public float speed, gravity;

    float horizontal, vertical;
    Vector3 velocity, forward, right;

    public Transform groundCheck;
    public float groundDistance;
    public LayerMask groundMask;
    [SerializeField]bool isGrounded;

    void Awake()
    {
        cam = Camera.main.transform;
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if(isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

<<<<<<< Updated upstream
        velocity.y += gravity * Time.deltaTime;
=======
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }
>>>>>>> Stashed changes

        velocity += Physics.gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
