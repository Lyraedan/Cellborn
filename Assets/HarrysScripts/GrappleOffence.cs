using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleOffence : MonoBehaviour
{
    public int attackDamage;
    public bool isAttacking;
    public GameObject fire1;
    public GameObject fire2;
    public int timer = 0;
    public int timeBeforeFire;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > timeBeforeFire)
        {
            timer = 0;
        }
        else
        {
            timer++;
        }

        if (isAttacking)
        {
            if (fire1.activeSelf == false && timer == timeBeforeFire)
            {
                fire1.SetActive(true);
                fire2.SetActive(true);
            }
        }

        if (!isAttacking && fire1.activeSelf == true && timer == timeBeforeFire)
        {
            fire1.SetActive(false);
            fire2.SetActive(false);
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (isAttacking)
        {
            if (other.tag == "Enemy")
            {
                var enemy = other.gameObject.GetComponent<EnemyScript>();
                Temperature temperature = go.GetComponent<Temperature>();

                if (enemy != null)
                {
                    if (WeaponManager.instance.currentWeapon.colour == enemy.colour)
                    {
                        enemy.currentHP -= (attackDamage * 2);
                    }
                    else if (WeaponManager.instance.currentWeapon.colour == Color.grey)
                    {
                        enemy.currentHP -= attackDamage;
                    }
                    else
                    {
                        enemy.currentHP -= (attackDamage / 2);
                    }

                    if (WeaponManager.instance.currentWeapon.colour == Color.red)
                    {
                        temperature.temperature += 10;
                    }
                    else if (WeaponManager.instance.currentWeapon.colour == Color.blue)
                    {
                        temperature.temperature -= 10;
                    }
                }

                if (other.gameObject.GetComponent<AITest>() != null)
                {
                    other.gameObject.GetComponent<AITest>().Hit();
                }
                else if (other.gameObject.GetComponent<AIFairy>() != null)
                {
                    other.gameObject.GetComponent<AIFairy>().Hit();
                }
                else if (other.gameObject.GetComponent<AIWizard>() != null)
                {
                    other.gameObject.GetComponent<AIWizard>().Hit();
                }
            }
        }
    }
}
