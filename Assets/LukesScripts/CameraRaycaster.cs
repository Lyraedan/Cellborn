using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRaycaster : MonoBehaviour
{
    public static CameraRaycaster instance;

    private GameObject last;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public RaycastHit hitInfo;

    void Update()
    {
        // I hate this but can't be arsed to do a better implementation
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, Mathf.Infinity))
        {
            if (last != null)
            {
                if (last.GetComponent<HideWall>())
                {
                    if(!last.name.Equals(hitInfo.transform.gameObject))
                        last.GetComponent<HideWall>().Show();
                }
            }

            Debug.DrawRay(transform.position, transform.forward * hitInfo.distance, Color.yellow);
            if(hitInfo.transform.gameObject.GetComponent<HideWall>())
            {
                hitInfo.transform.gameObject.GetComponent<HideWall>().Hide();
            }
            last = hitInfo.transform.gameObject;
        }
    }
}
