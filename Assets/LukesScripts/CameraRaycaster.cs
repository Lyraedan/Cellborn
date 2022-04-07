using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRaycaster : MonoBehaviour
{
    private GameObject last;
    public RaycastHit hitInfo;

    public float directionMultiplier = 0;

    void Update()
    {
        var direction = transform.forward + (transform.right * directionMultiplier);
        // I hate this but can't be arsed to do a better implementation
        if (Physics.Raycast(transform.position, direction, out hitInfo, Mathf.Infinity))
        {
            if (last != null)
            {
                if (last.GetComponent<HideWall>())
                {
                    if(!last.name.Equals(hitInfo.transform.gameObject))
                        last.GetComponent<HideWall>().Show();
                }
            }

            Debug.DrawRay(transform.position, direction * hitInfo.distance, Color.yellow);
            if(hitInfo.transform.gameObject.GetComponent<HideWall>())
            {
                hitInfo.transform.gameObject.GetComponent<HideWall>().Hide();
            }
            last = hitInfo.transform.gameObject;
        }
    }

    private void OnDrawGizmos()
    {
        if (directionMultiplier == 0f)
        {
            Gizmos.color = Color.green;
            Vector2 aspectRatio = new Vector2(1, 1);
            float ar = aspectRatio.x / aspectRatio.y;
            Gizmos.matrix = Matrix4x4.TRS(Camera.main.transform.position, Camera.main.transform.rotation, new Vector3(1.0f, ar, 1.0f));
            Gizmos.DrawFrustum(Vector3.zero, Camera.main.fieldOfView, Camera.main.farClipPlane, Camera.main.nearClipPlane, Camera.main.aspect);
        }
    }
}
