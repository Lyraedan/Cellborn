using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMovement : MonoBehaviour
{
    public float height = -0.45f;
    private Vector3 joystickHeading = Vector3.zero;

    void Update()
    {
        if (!PauseMenu.isPaused)
        {
            Plane plane = new Plane(Vector3.up, -height);
            Ray ray = Camera.main.ScreenPointToRay(ControlManager.ControllerConnected ? Vector3.zero : Input.mousePosition);

            if(plane.Raycast(ray, out float distanceToPlane))
            {
                if (!ControlManager.ControllerConnected)
                {
                    transform.position = ray.GetPoint(distanceToPlane);
                } else
                {
                    var horizontalTurn = Input.GetAxisRaw("HorizontalTurn") * 1.25f;
                    var verticalTurn = Input.GetAxisRaw("VerticalTurn") * 1.25f;

                    float deadzone = 0.5f;
                    bool isMovingJoystick = (horizontalTurn < -deadzone || horizontalTurn >= deadzone) || (verticalTurn < -deadzone || verticalTurn >= deadzone);
                    var forward = PlayerStats.instance.gameObject.transform.forward;
                    var right = PlayerStats.instance.gameObject.transform.right;

                    /*
                    var heading = PlayerStats.instance.gameObject.transform.position + 
                                  ((forward * verticalTurn)
                                  + (right * horizontalTurn));
                                  */

                    if (isMovingJoystick)
                    {
                        //joystickHeading = new Vector3(right.x + horizontalTurn, 0, forward.y + -verticalTurn);
                        joystickHeading = new Vector3(horizontalTurn, 0, -verticalTurn);
                    }
                    var translation = PlayerStats.instance.gameObject.transform.position + joystickHeading;
                    translation.y = height;

                        transform.position = translation;
                }
            }
        }
    }
}
