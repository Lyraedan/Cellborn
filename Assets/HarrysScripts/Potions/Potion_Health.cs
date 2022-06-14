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
        if (PlayerStats.instance.currentHP < 100)
        {
            PlayerStats.instance.currentHP += healthRefill;
            
            if (PlayerStats.instance.currentHP > 100)
            {
                PlayerStats.instance.healthText.text = PlayerStats.instance.maxHP + " / " + PlayerStats.instance.maxHP;
            }
            else
            {
                PlayerStats.instance.healthText.text = PlayerStats.instance.currentHP + " / " + PlayerStats.instance.maxHP;
            }
        }
    }
}
