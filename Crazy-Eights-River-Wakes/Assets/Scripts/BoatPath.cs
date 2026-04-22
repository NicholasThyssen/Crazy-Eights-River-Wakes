using UnityEngine;

public class BoatPath : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 5f;
    public float turnSpeed = 2f;
    private int currentWaypoint = 0;
    public bool isMoving = false;

    private bool hasStartedMoving = false;
    private readonly Quaternion offset = Quaternion.Euler(0, 90f, 0);

    void Update()
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

        transform.position += direction * speed * Time.deltaTime;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookDirection,
            Time.deltaTime * turnSpeed
        );

        if (Vector3.Distance(transform.position, target.position) < 1f)
        {
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Length)
                currentWaypoint = 0;
        }
    }
}