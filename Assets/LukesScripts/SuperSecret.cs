using Fragsurf.Movement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperSecret : MonoBehaviour
{

    public static SuperSecret instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public static bool secretEnabled = false; // Enable first person mode (Must be done before the game starts)

    public PlayerMovementTest ogMovement;
    public CapsuleCollider playerCollider;
    public GameObject cameraHolder;
    [SerializeField] private Camera fpsCam;
    public SurfCharacter fps;

    void Start()
    {
        fps.enabled = secretEnabled;
        cameraHolder.SetActive(secretEnabled);
        playerCollider.enabled = !secretEnabled;

        if (secretEnabled)
        {
            RoomGenerator.instance.enableCulling = false;
            ogMovement.playerCollider = fps.collider;
            RoomGenerator.instance.roofMesh.gameObject.layer = LayerMask.NameToLayer("Default");
            RoomGenerator.instance.floorMesh.gameObject.layer = LayerMask.NameToLayer("Default");
            RoomGenerator.instance.wallMesh.gameObject.layer = LayerMask.NameToLayer("Default");
            StartCoroutine(SetupCameras());
        }
    }

    IEnumerator SetupCameras()
    {
        yield return new WaitUntil(() => CameraManager.instance != null);
        CameraManager.instance.main.transform.position = fpsCam.transform.position;
        CameraManager.instance.main.transform.rotation = fpsCam.transform.rotation;

        CameraManager.instance.cull.transform.position = fpsCam.transform.position;
        CameraManager.instance.cull.transform.rotation = fpsCam.transform.rotation;

        CameraManager.instance.main.transform.SetParent(cameraHolder.transform);
    }

    private void LateUpdate()
    {

    }


}
