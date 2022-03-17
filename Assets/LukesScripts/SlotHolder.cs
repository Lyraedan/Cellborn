using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotHolder : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI number;
    public bool isSelected;

    public void SelectSlot()
    {
        isSelected = true;
        gameObject.GetComponent<Image>().CrossFadeAlpha(1f, 0.01f, false);
    }

    public void DeselectSlot()
    {
        isSelected = false;
        gameObject.GetComponent<Image>().CrossFadeAlpha(0.75f, 0.01f, false);
    }
}
