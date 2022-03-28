using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCulling : MonoBehaviour
{
    [SerializeField] private new Collider collider;
    [SerializeField] private MeshRenderer[] meshRenderers;
    [SerializeField] private LightCulling lights;
    [SerializeField] private ParticleCulling particles;

    private void Update()
    {
        if (RoomGenerator.instance == null)
            return;

        if (RoomGenerator.instance.enableCulling)
        {
            bool isVisible = ColliderIsInCameraView(collider);
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                if(meshRenderers[i] != null)
                    meshRenderers[i].enabled = isVisible;
            }
            if (lights != null)
            {
                lights.Cull(isVisible);
            }

            if (particles != null)
            {
                particles.Cull(isVisible);
            }
        } else
        {
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                if (meshRenderers[i] != null)
                    meshRenderers[i].enabled = true;
            }
            if (lights != null)
            {
                lights.Cull(true);
            }

            if (particles != null)
            {
                particles.Cull(true);
            }
        }
    }

    public bool ColliderIsInCameraView(Collider collider)
    {
        if (collider == null)
        {
            Debug.LogError($"{gameObject.name} does not have a collider assigned to culling. Defaulting to active renderer");
            return true;
        }

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(CameraManager.instance.cull);
        return GeometryUtility.TestPlanesAABB(planes, collider.bounds);
    }
}
