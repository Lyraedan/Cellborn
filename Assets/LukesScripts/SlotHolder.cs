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
        image.rectTransform.localScale = new Vector3(2f, 2f, 1);
    }

    public void DeselectSlot()
    {
        isSelected = false;
        gameObject.GetComponent<Image>().CrossFadeAlpha(0.75f, 0.01f, false);
        image.rectTransform.localScale = new Vector3(1.5f, 1.5f, 1);
    }
}
