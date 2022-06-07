using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallStencil : MonoBehaviour
{
    public GameObject target;
    public LayerMask mask;

    Vector3 baseScale = Vector3.one;

    private void Start()
    {
        baseScale = target.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if(Physics.Raycast(Camera.main.transform.position, (target.transform.position - Camera.main.transform.position).normalized, out hit, Mathf.Infinity, mask))
        {
            if(hit.collider.gameObject.CompareTag("SphereMask"))
            {
                target.transform.localScale = new Vector3(0, 0, 0);
            } else
            {
                target.transform.localScale = baseScale;
            }
        }
    }
}
