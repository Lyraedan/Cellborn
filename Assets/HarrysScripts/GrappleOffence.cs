using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleOffence : MonoBehaviour
{
    public int attackDamage;
    public bool isAttacking;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (isAttacking)
        {
            if (other.tag == "Enemy")
            {
                var enemy = other.gameObject.GetComponent<EnemyScript>();

                if (enemy != null)
                {
                    enemy.currentHP -= attackDamage;
                }
            }
        }
    }
}
