using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class twoDimensionalAnimationStateController : MonoBehaviour
{
    Animator animator;
    public float velocityZ = 0.0f;
    public float velocityX = 0.0f;
    public float acceleration = 2.0f;
    public float deceleration = 2.0f;


    void Start()
    {
        // searches the game object this script is attached to and gets the animator component
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // input will be true if the player is pressing on the passed in key parameter
        // Get players input key
        bool forwardPressed = Input.GetKey("w");
        bool leftPressed = Input.GetKey("a");
        bool rightPressed = Input.GetKey("d");
        bool backwardPressed = Input.GetKey("s");

        // When player presses W, increase the velocity Z value
        if (forwardPressed)
        {
            velocityZ += Time.deltaTime * acceleration;
        }
        // When player presses S, decrease the velocity Z value
        if (backwardPressed)
        {
            velocityZ -= Time.deltaTime * acceleration;
        }
        // When player presses A, decrease the velocity X value
        if (leftPressed)
        {
            velocityX -= Time.deltaTime * acceleration;
        }
        // When player presses D, increase the velocity X value
        if (rightPressed)
        {
            velocityX += Time.deltaTime * acceleration;
        }

        animator.SetFloat("Velocity Z", velocityZ);
        animator.SetFloat("Velocity X", velocityX);
    }
}
