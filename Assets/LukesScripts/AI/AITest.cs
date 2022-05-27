using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LukesScripts.AI;
using UnityEngine;

public class AITest : AI
{

    private float secondsBetweenAttacks = 1f;
    private float attackDelay = 0;
    public PlayerStats playerStats;
    private int damage = 1;

    public GameObject acid;
    public GameObject acidPoint;
    
    public Animator animController;

    [SerializeField] private float remainingDistance = 0f;

    float velocity;

    public override void Init()
    {

    }

    public override void Tick()
    {
        attackDelay += 1f * Time.deltaTime;

        velocity = agent.velocity.magnitude;

        if (velocity > 0)
        {
            animController.SetBool("IsMoving", true);
        }
        else
        {
            animController.SetBool("IsMoving", false);
        }
    }

    public override void Attack()
    {
        if (attackDelay >= secondsBetweenAttacks)
        {
            Debug.Log("Do attack!");
            //AudioManager.instance.Play("SnakeAttack");
            animController.SetTrigger("Attack");

            attackDelay = 0;
        }
    }

    public override void OnHit()
    {
        Debug.Log("Notice player");
    }

    public override void OnDeath()
    {
        
    }

    void OnCollisionEnter(Collision other)
    {

    }


    public override void DrawGizmos()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            var nearest = GetNearestMuffin();
            if(nearest != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, nearest.transform.position);
                UnityEditor.Handles.Label(transform.position + (nearest.transform.position / 2), $"{Vector3.Distance(transform.position, nearest.transform.position)}");
            }
        }
#endif
    }


    public GameObject GetNearestMuffin()
    {
        var muffins = GameObject.FindObjectsOfType<Muffin>().ToList();
        if (muffins.Count > 0)
        {
            muffins.Sort((a, b) =>
            {
                return a.DistanceFrom(transform.position).CompareTo(b.DistanceFrom(transform.position));
            });
            return muffins[0].gameObject;
        }
        else
            return null;
    }

    public void SpawnAcid()
    {
        Instantiate(acid, acidPoint.transform.position, Quaternion.identity);
    }
}
