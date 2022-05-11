using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PotionBase : MonoBehaviour
{
    public bool isInPlayerInventory = false;
    public AudioSource source;
    public AudioClip useSound;
    
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

    public void UsePotion()
    {
        Use();
        source.clip = useSound;
        source.Play();
    }

    public abstract void Use();
}
