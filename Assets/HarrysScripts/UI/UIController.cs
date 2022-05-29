using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

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

    public PostProcessProfile profile;
    private ColorGrading _colorGrading;

    void Start()
    {
        canvas.worldCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        canvas.planeDistance = 1f;

        profile.TryGetSettings(out _colorGrading);
        _colorGrading.active = true;
        
        Minimap.instance.OnMapGenerated += () =>
        {
            minimapBG.SetNativeSize();
            minimapBG.rectTransform.position = Minimap.instance.canvas.rectTransform.position;
            minimapBG.rectTransform.sizeDelta = new Vector2(Minimap.instance.canvas.sprite.rect.width * 4, Minimap.instance.canvas.sprite.rect.height * 4);
        };
    }
}
