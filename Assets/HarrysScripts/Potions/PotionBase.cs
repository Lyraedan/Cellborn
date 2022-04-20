using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PotionBase : MonoBehaviour
{
    public bool isInPlayerInventory = false;
    
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
