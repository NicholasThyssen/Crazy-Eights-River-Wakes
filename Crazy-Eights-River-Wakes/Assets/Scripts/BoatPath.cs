using UnityEngine;

public class BoatPath : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 5f;
    public float turnSpeed = 2f;

    private int currentWaypoint = 0;
    public bool isMoving = false;

    void Update()
    {
        if (!isMoving) return;
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypoint];

        Vector3 direction = (target.position - transform.position).normalized;

        transform.position += direction * speed * Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(-direction);

        float rawTurn = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
    float turnAmount = Mathf.Lerp(0, rawTurn, 0.1f);

        turnAmount = Mathf.Clamp(turnAmount, -30f, 30f);

        float tiltStrength = 0.2f;

        Quaternion tilt = Quaternion.Euler(0, 0, -turnAmount * tiltStrength);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation * tilt,
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