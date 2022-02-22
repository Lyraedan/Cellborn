using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static float horizontal {
        get {
            if(Input.GetJoystickNames().Length > 0)
            {
                Debug.Log("Gamepad Horizontal: " + Input.GetAxisRaw("Left_Anolog_Horizontal"));
                return Input.GetAxisRaw("Left_Anolog_Horizontal");
            }
            return Input.GetAxisRaw("Horizontal");
        }
    }

    public static float vertical
    {
        get
        {
            if (Input.GetJoystickNames().Length > 0)
            {
                Debug.Log("Gamepad Vertical: " + Input.GetAxisRaw("Left_Anolog_Vertical"));
                return Input.GetAxisRaw("Left_Anolog_Vertical");
            }
            return Input.GetAxisRaw("Vertical");
        }
    }
}
