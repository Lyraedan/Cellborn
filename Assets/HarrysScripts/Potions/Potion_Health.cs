using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion_Health : PotionBase
{
    [Header("Potion Settings")]
    public int healthRefill;
    
    public override void Init()
    {
    }

    public override void Tick()
    {
    }

    public override void Use()
    {
        PlayerStats stats = FindObjectOfType<PlayerStats>();
        stats.currentHP += healthRefill;
    }
}
