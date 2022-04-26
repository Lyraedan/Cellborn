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
    private int _currentHP;
    public int currentHP { get
        {
            return _currentHP;
        }
        set {
            if(currentHP != value)
            {
                UIController.instance.healthBar.value = value;
                _currentHP = value;
            }
        }
    }
    public TextMeshProUGUI win;

    [HideInInspector] public float defenseMultiplier = 1;
    [HideInInspector] public float defenseTime;
    float defenseTimer;

    #region Player Components
    
    public TextMeshProUGUI healthText;
    public GameObject deathEffect;

    public SkinnedMeshRenderer[] meshRenderers;
    public PlayerMovementTest moveScript;
    public CapsuleCollider playerCollider;

    public List<WeaponProperties> weaponsInScene = new List<WeaponProperties>();
    public List<PotionProperties> potionsInScene = new List<PotionProperties>();

    public LaserControl laserControl;

    #endregion

    #region UI

    public Image damageRed;
    public float indicFadeTime;

    public Image armourBlue;

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
        damageRed.CrossFadeAlpha(0, 0f, false);
        armourBlue.CrossFadeAlpha(0, 0f, false);
        UIController.instance.healthBar.maxValue = maxHP;

    }

    void Update()
    {
        healthText.text = currentHP + " / " + maxHP;
        
        if (currentHP <= 0)
        {
            currentHP = 0;
            
            if(!isDead)
            {
                KillPlayer();
            }
        }

        if (currentHP >= maxHP)
        {
            currentHP = maxHP;
        }

        weaponsInScene.Sort((x, y) => x.DistanceFromPlayer.CompareTo(y.DistanceFromPlayer));
        potionsInScene.Sort((x, y) => x.DistanceFromPlayer.CompareTo(y.DistanceFromPlayer));

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
        } 
        
        if(potionsInScene.Count > 0)
        {
            var closestPickup = potionsInScene[0];
            if (closestPickup.DistanceFromPlayer <= 1f)
            {
                PotionManager.instance.toPickup = closestPickup;
                PotionManager.instance.pickupText.text = $"{PotionManager.instance.pickupKey.ToString()} - Pick Up {closestPickup.potionName}";
            } else
            {
                PotionManager.instance.toPickup = null;
                PotionManager.instance.pickupText.text = string.Empty;
            }
        } 

        damageRed.CrossFadeAlpha(0f, indicFadeTime, false);        

        if (defenseMultiplier != 1)
        {
            armourBlue.CrossFadeAlpha(1f, 0.25f, false);
            defenseTimer += 1f * Time.deltaTime;

            if (defenseTimer >= defenseTime)
            {
                defenseTime = 0f;
                defenseTimer = 0f;
                defenseMultiplier = 1;
                armourBlue.CrossFadeAlpha(0f, 2f, false);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        var collObj = collision.gameObject;

        if (/*collObj.tag == "Projectile" || */collObj.CompareTag("EnemyProjectile"))
        {     
            if(collObj.GetComponent<FairyMagic>() != null)
            {
                collObj.GetComponent<FairyMagic>().HitPlayer();
            }
            else
            {
                DamagePlayer(collObj.GetComponentInChildren<ProjectileBehaviour>().playerDamage);
            }
        }
    }

    //Casual Matilde Function
    public void DamagePlayer(int damage)
    {
        if (!isDead)
        {
            damageRed.CrossFadeAlpha(1, 0f, false);
            AudioManager.instance.Play("PlayerHurt");
            currentHP -= (int)(damage * defenseMultiplier);
        }
    }

    public void KillPlayer()
    {
        currentHP = 0;
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if(meshRenderers[i] != null)
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