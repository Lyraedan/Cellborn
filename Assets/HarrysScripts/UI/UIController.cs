using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    public Canvas canvas;

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

    [Header("Boss Health")]
    public GameObject bossHealthGroup;
    public Slider bossHealthBar;
    public TextMeshProUGUI bossHealthText;

    public Camera UICamera;

    void Start()
    {
        UICamera = GameObject.FindWithTag("UICamera").GetComponent<Camera>();
        
        canvas.worldCamera = UICamera;
        canvas.planeDistance = 1f;
        UICamera.enabled = true;
        
        Minimap.instance.OnMapGenerated += () =>
        {
            minimapBG.SetNativeSize();
            minimapBG.rectTransform.position = Minimap.instance.canvas.rectTransform.position;
            minimapBG.rectTransform.sizeDelta = new Vector2(Minimap.instance.canvas.sprite.rect.width * 4, Minimap.instance.canvas.sprite.rect.height * 4);
        };
    }

}
