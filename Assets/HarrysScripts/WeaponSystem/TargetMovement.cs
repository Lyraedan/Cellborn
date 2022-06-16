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

            if (!SuperSecret.secretEnabled)
            {
                if (plane.Raycast(ray, out float distanceToPlane))
                {
                    if (!ControlManager.ControllerConnected)
                    {
                        transform.position = ray.GetPoint(distanceToPlane);
                    }
                    else
                    {
                        float maxDistanceFromPlayer = 3f; // 1.25f

                        var horizontalTurn = Input.GetAxisRaw(ControlManager.JOYSTICK_HORIZONTAL_TURN) * maxDistanceFromPlayer;
                        var verticalTurn = Input.GetAxisRaw(ControlManager.JOYSTICK_VERTICAL_TURN) * maxDistanceFromPlayer;

                        float deadzone = 0.5f;
                        bool isMovingJoystick = (horizontalTurn < -deadzone || horizontalTurn >= deadzone) || (verticalTurn < -deadzone || verticalTurn >= deadzone);
                        var forward = isMovingJoystick ? PlayerStats.instance.gameObject.transform.forward : Vector3.zero;
                        var right = isMovingJoystick ? PlayerStats.instance.gameObject.transform.right : Vector3.zero;

                        if (isMovingJoystick)
                        {
                            //joystickHeading = new Vector3(right.x + horizontalTurn, 0, forward.y + -verticalTurn);
                            joystickHeading = new Vector3(horizontalTurn, 0, -verticalTurn);
                            //joystickHeading = (forward * verticalTurn) + (right * horizontalTurn);
                        }
                        var translation = PlayerStats.instance.gameObject.transform.position + joystickHeading;
                        translation.y = height;

                        transform.position = translation;
                    }
                }
            }
            else
            {
                RaycastHit hit;
                Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward, Color.white);
                int layerMask = LayerMask.NameToLayer("Player");

                layerMask = ~layerMask;

                var dir = Camera.main.transform.forward;
                var start = WeaponManager.instance.firepoint.position; //Camera.main.transform.position;
                if (Physics.Raycast(start, dir, out hit, Mathf.Infinity, layerMask))
                {
                    Debug.DrawRay(start, dir * hit.distance, Color.yellow);
                    Vector3 pos = hit.point;
                    Debug.Log("Hitting: " + hit.collider.gameObject.name);
                    pos.y = height;
                    transform.position = pos;
                } else
                {
                    Debug.DrawRay(start, Camera.main.transform.forward * 1000, Color.white);
                }
            }
        }
    }
}
