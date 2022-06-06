using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion_Speed : PotionBase
{
    [Header("Potion Settings")]
    public float speedMultiplier;
    public float speedUpTime;
    
    public override void Init()
    {
    }

    public override void Tick()
    {
    }

    public override void Use()
    {
        PlayerMovementTest.instance.potionSpeedMultiplier = speedMultiplier;
        PlayerMovementTest.instance.speedUpTime = speedUpTime;
        PlayerMovementTest.instance.isSpedUp = true;
        PlayerMovementTest.instance.isSpeedPotioned = true;
    }
}
