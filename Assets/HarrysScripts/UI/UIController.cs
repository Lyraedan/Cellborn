using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    [Header("Health")]
    public Slider healthBar;

    [Header("Ammo")]
    public GameObject ammoContainer;
    public Slider ammoBar;

    [Header("Minimap")]
    public Image minimapBG;

    [Header("Effects")]
    public Image damageIndicator;
    public Image defenseIndicator;

    void Start()
    {
        Minimap.instance.OnMapGenerated += () =>
        {
            minimapBG.SetNativeSize();
            minimapBG.rectTransform.position = Minimap.instance.canvas.rectTransform.position;
            minimapBG.rectTransform.sizeDelta = new Vector2(Minimap.instance.canvas.sprite.rect.width * 4, Minimap.instance.canvas.sprite.rect.height * 4);
        };

        Debug.Log(Screen.currentResolution.width + "," + Screen.currentResolution.height);

        damageIndicator.rectTransform.sizeDelta = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
        defenseIndicator.rectTransform.sizeDelta = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
    }
}
