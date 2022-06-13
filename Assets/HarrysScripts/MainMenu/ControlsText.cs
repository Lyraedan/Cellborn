using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ControlsText : MonoBehaviour
{
    public TextMeshProUGUI controlsText;
    
    void Update()
    {
        if (ControlManager.ControllerConnected)
        {
            controlsText.text = "Left Stick - Movement \n\nRight Stick - Aim \n\n" + 
                                "RT - Fire \n\n" + 
                                "D Pad Up/Down - Select Weapon \n\n" +
                                ControlManager.pickup + " - Pick Up Weapon / Potion \n\n" +
                                ControlManager.drop + " - Drop Currently Selected Weapon \n\n" +
                                ControlManager.usePotionA + " & " + ControlManager.usePotionB + " - Use Potion \n\n" +
                                "Start - Pause"; 
        } 
        else
        {
            controlsText.text = "WASD - Movement \n\nMouse - Aim \n\n" + 
                                "Left Click - Fire \n\n" + 
                                "Scrollwheel / 1, 2, 3 - Select Weapon \n\n" +
                                ControlManager.pickup + " - Pick Up Weapon / Potion \n\n" +
                                ControlManager.drop + " - Drop Currently Selected Weapon \n\n" +
                                ControlManager.usePotionA + " & " + ControlManager.usePotionB + " - Use Potion \n\n" +
                                "Escape - Pause"; 
        }      
    }
}
