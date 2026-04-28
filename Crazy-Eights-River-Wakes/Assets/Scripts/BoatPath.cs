using UnityEngine;

public class BoatPath : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed;
    public float turnSpeed = 2f;
    private int currentWaypoint = 0;
    public bool isMoving = false;

    private bool hasStartedMoving = false;
    private readonly Quaternion offset = Quaternion.Euler(0, 90f, 0);
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        speed = 3.5f;
    }

    void FixedUpdate()
    {
        if (!isMoving) return;
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypoint];
        Vector3 direction = (target.position - transform.position).normalized;

        direction.y = 0f;
        direction.Normalize();

        Quaternion lookDirection = Quaternion.LookRotation(direction) * offset;

        if (!hasStartedMoving)
        {
            transform.rotation = lookDirection;
            hasStartedMoving = true;
        }

        Vector3 newPosition = rb.position + direction * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);

        // transform.position += direction * speed * Time.deltaTime;

        Quaternion newRotation = Quaternion.Slerp(
            rb.rotation,
            lookDirection,
            Time.fixedDeltaTime * turnSpeed
        );
        rb.MoveRotation(newRotation);

        // transform.rotation = Quaternion.Slerp(
        //     transform.rotation,
        //     lookDirection,
        //     Time.deltaTime * turnSpeed
        // );

        if (Vector3.Distance(transform.position, target.position) < 1f)
        {
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Length)
                currentWaypoint = 0;
        }
    }
}