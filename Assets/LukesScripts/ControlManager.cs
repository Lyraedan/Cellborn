using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlManager : MonoBehaviour
{
    private static int XboxOneController = 0;
    private static int PS4Controller = 0;
    public static bool ControllerConnected
    {
        get
        {
            return XboxOneController != 0 || PS4Controller != 0;
        }
    }

    public static string pickup = "F";
    public static string drop = "G";
    public static string usePotionA = "Q";
    public static string usePotionB = "E";

    public static string INPUT_FIRE = "Fire1";
    public static string INPUT_PICKUP = "Pickup";
    public static string INPUT_DROP = "Drop";
    public static string INPUT_PAUSE = "Pause";

    public static string INPUT_USE_POTION_A = "UsePotionA";
    public static string INPUT_USE_POTION_B = "UsePotionB";

    public static string JOYSTICK_HORIZONTAL_TURN = "HorizontalTurn";
    public static string JOYSTICK_VERTICAL_TURN = "VerticalTurn";
    public static string JOYSTICK_WEAPON_SWITCH = "XboxWeaponSwitch";


    private void Update()
    {
        var names = Input.GetJoystickNames();

        Debug.Log("Controllers: " + names.Length);
        for (int i = 0; i < names.Length; i++)
        {
            Debug.Log("Controller: " + names[i]);
            if (names[i].Length == 19) //wireless controller
            {
                Debug.Log("PS4 controller is connected");
                PS4Controller = 1;
                XboxOneController = 0;
            }
            if (names[i].Contains("Xbox"))
            {
                Debug.Log("Xbox controller is connected");
                PS4Controller = 0;
                XboxOneController = 1;
            }
            if (string.IsNullOrEmpty(names[i]))
            {
                PS4Controller = 0;
                XboxOneController = 0;
            }
        }

        if (XboxOneController == 1)
        {
            //Remap
            JOYSTICK_HORIZONTAL_TURN = "XboxHorizontalTurn";
            JOYSTICK_VERTICAL_TURN = "XboxVerticalTurn";
            JOYSTICK_WEAPON_SWITCH = "XboxWeaponSwitch";

            INPUT_FIRE = "XboxFire";
            INPUT_DROP = "XboxDrop";
            INPUT_PAUSE = "XboxPause";
            INPUT_PICKUP = "XboxPickup";

            INPUT_USE_POTION_A = "XboxUsePotionA";
            INPUT_USE_POTION_B = "XboxUsePotionB";

            pickup = "X";
            drop = "Y";
            usePotionA = "LB";
            usePotionB = "RB";
        }
        else if (PS4Controller == 1)
        {
            JOYSTICK_HORIZONTAL_TURN = "PS4HorizontalTurn";
            JOYSTICK_VERTICAL_TURN = "PS4VerticalTurn";
            JOYSTICK_WEAPON_SWITCH = "PS4WeaponSwitch";

            INPUT_FIRE = "PS4Fire";
            INPUT_DROP = "PS4Drop";
            INPUT_PAUSE = "PS4Pause";
            INPUT_PICKUP = "PS4Pickup";

            INPUT_USE_POTION_A = "PS4UsePotionA";
            INPUT_USE_POTION_B = "PS4UsePotionB";

            pickup = "Square";
            drop = "Triangle";
            usePotionA = "L1";
            usePotionB = "R1";
        }
        else
        {
            JOYSTICK_HORIZONTAL_TURN = "XboxHorizontalTurn";
            JOYSTICK_VERTICAL_TURN = "XboxVerticalTurn";
            JOYSTICK_WEAPON_SWITCH = "XboxWeaponSwitch";

            INPUT_USE_POTION_A = "UsePotionA";
            INPUT_USE_POTION_B = "UsePotionB";

            INPUT_FIRE = "Fire1";
            INPUT_PICKUP = "Pickup";
            INPUT_DROP = "Drop";
            INPUT_PAUSE = "Pause";

            pickup = "F";
            drop = "G";
            usePotionA = "Q";
            usePotionB = "E";
        }
    }

}
