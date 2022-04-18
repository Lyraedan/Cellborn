using UnityEngine;

public class Muffin : MonoBehaviour {

    public float DistanceFrom(Vector3 point)
    {
        return Vector3.Distance(transform.position, point);
    }
}
