using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PotionBase : MonoBehaviour
{
    [Header("Base Settings")]
    public float timeBetweenUses = 1f;
    protected bool canDrink = true;
    protected float timer = 0;
    public bool infiniteUses = false;

    void Start()
    {
        Init();
    }
    
    void Update()
    {
        Tick();
    }

    public abstract void Init();
    public abstract void Tick();
    public abstract void Use();
}
