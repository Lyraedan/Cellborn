using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMovement : MonoBehaviour
{
    public GameObject targetObject;
    public Camera mainCam;
    public LayerMask hitLayers;
    
    Vector3 mouse;
    Ray castPoint;

    void Update()
    {
       if(mainCam == null)
        {
            Debug.Log("Cast point has not been assigned in target.");
            return;
        }
        mouse = Input.mousePosition;
        castPoint = mainCam.ScreenPointToRay(mouse);
        RaycastHit hit;

        if (Physics.Raycast(castPoint, out hit, Mathf.Infinity, hitLayers))
        {
            targetObject.transform.position = hit.point;
        }
    }
}
