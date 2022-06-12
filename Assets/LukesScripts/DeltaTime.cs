using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeltaTime : MonoBehaviour
{
    public static DeltaTime instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(this);

    }

    private DateTime tp1;
    private DateTime tp2;
    public float deltaTime { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        tp1 = DateTime.Now;
        tp2 = DateTime.Now;
    }

    // Update is called once per frame
    void Update()
    {
        tp2 = DateTime.Now;
        deltaTime = (float)((tp2.Ticks - tp1.Ticks) / 10000000.0);
        tp1 = tp2;
    }
}
