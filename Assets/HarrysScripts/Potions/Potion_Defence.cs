using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Potion_Defence : PotionBase
{
    [Header("Potion Settings")]
    public float defenseMultiplier;
    public float defenseTime;
    
    public override void Init()
    {
    }

    public override void Tick()
    {
    }

    public override void Use()
    {
        PlayerStats.instance.defenseMultiplier = defenseMultiplier;
        PlayerStats.instance.defenseTime = defenseTime;
        AudioManager.instance.Play("DefenseOn");
    }
}
