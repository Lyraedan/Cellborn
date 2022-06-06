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

    public float lifetime;
    public float t;
    public Color colour;
    public bool red;
    public bool blue;
    public bool yellow;

    private void Start()
    {
        if (red)
        {
            colour = Color.red;
        }
        else if (blue)
        {
            colour = Color.blue;
        }
        else if (yellow)
        {
            colour = Color.yellow;
        }

        OnPlayerStoodInAcid?.AddListener(() =>
        {
            PlayerStats.instance.DamagePlayer(playerDamage);
            if (colour == Color.red)
            {
                PlayerStats.instance.temperature.temperature += 6;
            }
            else if (colour == Color.blue)
            {
                PlayerStats.instance.temperature.temperature -= 4;
            }
            else if (colour == Color.yellow)
            {
                PlayerStats.instance.temperature.shockDuration = 4;
            }
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

        t += 1f * Time.deltaTime;

        if (t >= lifetime)
        {
            Destroy(gameObject);
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