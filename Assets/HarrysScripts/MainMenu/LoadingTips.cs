using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingTips : MonoBehaviour
{
    public TextMeshProUGUI tipText;
    [TextArea(3, 10)]
    public List<string> tips;
    public float tipTime;

    int tipNumber;
    float timer;

    void Start()
    {
        SetTip();
    }

    void Update()
    {
        timer += 1f * Time.deltaTime;

        if (timer >= tipTime)
        {
            SetTip();
            timer = 0f;
        }
    }

    public void SetTip()
    {
        tipNumber = Random.Range(0, tips.Count);
        tipText.text = "Tip: " + tips[tipNumber];
    }
}
