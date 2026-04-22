using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatPath : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 5f;
    public float turnSpeed = 2f;
    private int currentWaypoint = 0;
    public bool isMoving = false;

    private bool hasStartedMoving = false;
    private readonly Quaternion offset = Quaternion.Euler(0, 90f, 0);
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!isMoving) return;
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypoint];
        Vector3 targetOffset = target.position - rb.position;
        if (targetOffset.sqrMagnitude < 0.0001f)
        {
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Length)
                currentWaypoint = 0;
            return;
        }

        Vector3 direction = targetOffset.normalized;

        Quaternion lookDirection = Quaternion.LookRotation(direction) * offset;
        lookDirection.y = 0f;

        if (!hasStartedMoving)
        {
            rb.MoveRotation(lookDirection);
            hasStartedMoving = true;
        }

        Vector3 horizontalDirection = new Vector3(direction.x, 0f, direction.z);
        if (horizontalDirection.sqrMagnitude > 0f)
        {
            horizontalDirection.Normalize();

            Vector3 currentHorizontalVelocity = new Vector3(
                rb.linearVelocity.x,
                0f,
                rb.linearVelocity.z
            );
            Vector3 desiredHorizontalVelocity = horizontalDirection * speed;
            Vector3 velocityChange = desiredHorizontalVelocity - currentHorizontalVelocity;

            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }

        float rawTurn = Vector3.SignedAngle(transform.right, direction, Vector3.up);
        float turnAmount = Mathf.Lerp(0, rawTurn, 0.1f);
        turnAmount = Mathf.Clamp(turnAmount, -30f, 30f);

        float tiltStrength = 0.2f;
        Quaternion tilt = Quaternion.Euler(0, 0, -turnAmount * tiltStrength);
        Quaternion targetRotation = lookDirection;

        rb.MoveRotation(Quaternion.Slerp(
            rb.rotation,
            targetRotation * tilt,
            Time.fixedDeltaTime * turnSpeed
        ));

        if (Vector3.Distance(rb.position, target.position) < 1f)
        {
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Length)
                currentWaypoint = 0;
        }
    }
}
