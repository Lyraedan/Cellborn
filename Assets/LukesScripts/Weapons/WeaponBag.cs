using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WeaponBag : MonoBehaviour
{
    public static WeaponBag instance;
    public int dropChance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public List<GameObject> weaponBag = new List<GameObject>();
    public List<GameObject> backupBag = new List<GameObject>();

    private void Start()
    {
        RefillBag();
    }

    public void RefillBag()
    {
        weaponBag = backupBag.ToList();
    }
}
