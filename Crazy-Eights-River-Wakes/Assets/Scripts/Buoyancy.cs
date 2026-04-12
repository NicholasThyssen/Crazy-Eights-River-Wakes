using UnityEngine;

public class Buoyancy : MonoBehaviour
{
    [Header("Water")]
    public float waterY = 149f; // match your Water_Plane Y position

    [Header("Buoyancy")]
    public float buoyancyForce = 15f;
    public float waterDrag = 3f;
    public float waterAngularDrag = 5f;

    private Rigidbody rb;
    private float defaultDrag;
    private float defaultAngularDrag;
    private bool inWater = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        defaultDrag = rb.linearDamping;
        defaultAngularDrag = rb.angularDamping;
    }

   void FixedUpdate()
{
    if (transform.position.y < waterY)
    {
        float depth = waterY - transform.position.y;
        rb.AddForce(Vector3.up * buoyancyForce * depth, ForceMode.Force);

        // Stabilize upright rotation
        Vector3 predictedUp = Quaternion.AngleAxis(
            rb.angularVelocity.magnitude * Mathf.Rad2Deg,
            rb.angularVelocity) * transform.up;

        Vector3 torqueVector = Vector3.Cross(predictedUp, Vector3.up);
        rb.AddTorque(torqueVector * buoyancyForce * 0.5f);

        rb.linearDamping = waterDrag;
        rb.angularDamping = waterAngularDrag;
    }
    else
    {
        rb.linearDamping = defaultDrag;
        rb.angularDamping = defaultAngularDrag;
    }
}
}