using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Acid : MonoBehaviour
{
    public int playerDamage;

    bool stoodIn = false;

    public UnityEvent OnPlayerStoodInAcid;

    public float damageDelay = 1f;
    private float timer = 0f;

    private void Start()
    {
        OnPlayerStoodInAcid?.AddListener(() =>
        {
            PlayerStats.instance.DamagePlayer(playerDamage);
        });
    }

    private void Update()
    {
        if(stoodIn)
        {
            timer += 1f * Time.deltaTime;
            if (timer >= damageDelay)
            {
                OnPlayerStoodInAcid?.Invoke();
                timer = 0;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            Debug.Log("Entered acid");
            stoodIn = true;
            timer = 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            Debug.Log("Left acid");
            stoodIn = false;
            timer = 0f;
        }
    }
}