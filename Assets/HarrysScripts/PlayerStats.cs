using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats instance;

    public int maxHP;
    public int currentHP;
    public TextMeshProUGUI win;

    #region Player Components
    
    public TextMeshProUGUI healthText;
    public GameObject deathEffect;

    public MeshRenderer[] meshRenderers;
    public PlayerMovementTest moveScript;
    public CapsuleCollider playerCollider;

    public List<WeaponProperties> weaponsInScene = new List<WeaponProperties>();

    public LaserControl laserControl;

    #endregion

    public bool isDead;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    void Start()
    {
        currentHP = maxHP;
    }

    void Update()
    {
        healthText.text = "HP: " + currentHP + " / " + maxHP;
        
        if (currentHP <= 0)
        {
            currentHP = 0;
            
            if(!isDead)
            {
                KillPlayer();
            }
        }

        if (currentHP >= 10)
        {
            currentHP = maxHP;
        }

        weaponsInScene.Sort((x, y) => x.DistanceFromPlayer.CompareTo(y.DistanceFromPlayer));

        if(weaponsInScene.Count > 0)
        {
            var closestPickup = weaponsInScene[0];
            if (closestPickup.DistanceFromPlayer <= 1f)
            {
                WeaponManager.instance.toPickup = closestPickup;
                WeaponManager.instance.pickupText.text = $"{WeaponManager.instance.pickupKey.ToString()} - Pick Up {closestPickup.weaponName}";
            } else
            {
                WeaponManager.instance.toPickup = null;
                WeaponManager.instance.pickupText.text = string.Empty;
            }
        } else
        {
            WeaponManager.instance.toPickup = null;
            WeaponManager.instance.pickupText.text = string.Empty;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        var collObj = collision.gameObject;

        if (/*collObj.tag == "Projectile" || */collObj.CompareTag("EnemyProjectile"))
        {     
            currentHP -= collObj.GetComponentInChildren<ProjectileBehaviour>().playerDamage;
        }
    }

    //Casual Matilde Function
    public void DamagePlayer(int damage)
    {
        currentHP -= damage;
    }

    public void KillPlayer()
    {
        currentHP = 0;
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].enabled = false;
        }
        moveScript.enabled = false;
        playerCollider.enabled = false;
        laserControl.laserParticles.Stop();
        isDead = true;

        int seconds = 5;
        Debug.Log($"You are dead. Respawning in {seconds} seconds");
        StartCoroutine(TimedReset(seconds));
    }

    IEnumerator TimedReset(int seconds)
    {
        if(seconds == 0)
        {
            Debug.Log("Resetting!");
            RoomGenerator.instance.Regenerate();
            yield break;
        } else
        {
            Debug.Log(seconds);
            yield return new WaitForSeconds(1f);
            StartCoroutine(TimedReset(seconds - 1));
        }
    }
}