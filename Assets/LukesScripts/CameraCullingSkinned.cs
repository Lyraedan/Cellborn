using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCullingSkinned : MonoBehaviour
{
    [SerializeField] private new Collider collider;
    [SerializeField] private SkinnedMeshRenderer[] meshRenderers;
    [SerializeField] private LightCulling lights;
    [SerializeField] private ParticleCulling particles;

    private void Update()
    {
        if (RoomGenerator.instance.enableCulling)
        {
            bool isVisible = ColliderIsInCameraView(collider);
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                if(meshRenderers[i] != null)
                    meshRenderers[i].enabled = isVisible;
            }

            if(lights != null)
            {
                lights.Cull(isVisible);
            }

            if(particles != null)
            {
                particles.Cull(isVisible);
            }
        }
        else
        {
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                if (meshRenderers[i] != null)
                    meshRenderers[i].enabled = true;
            }
            if (lights != null)
            {
                lights.Cull(false);
            }

            if (particles != null)
            {
                particles.Cull(false);
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
