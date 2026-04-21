using UnityEngine;

public class BoatXRSync : MonoBehaviour
{
    private Transform boat;
    private Quaternion lastBoatRotation;

    void Start()
    {
        boat = transform.parent;
        lastBoatRotation = boat.rotation;
    }

    void Update()
    {
        Quaternion rotDelta = boat.rotation * Quaternion.Inverse(lastBoatRotation);
        transform.rotation = rotDelta * transform.rotation;
        lastBoatRotation = boat.rotation;
    }
}