using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCullingSkinned : MonoBehaviour
{
    [SerializeField] private new Collider collider;
    [SerializeField] private SkinnedMeshRenderer[] meshRenderers;

    private void Update()
    {
        if (RoomGenerator.instance.enableCulling)
        {
            bool isVisible = ColliderIsInCameraView(collider);
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].enabled = isVisible;
            }
        }
        else
        {
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].enabled = true;
            }
        }
    }

    public bool ColliderIsInCameraView(Collider collider)
    {
        if (collider == null)
            return false;

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        return GeometryUtility.TestPlanesAABB(planes, collider.bounds);
    }
}