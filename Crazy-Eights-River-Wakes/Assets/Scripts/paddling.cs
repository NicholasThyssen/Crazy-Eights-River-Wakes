using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PaddleForce : MonoBehaviour
{
    public Rigidbody boatRigidbody;
    public float paddleForce = 10f;
    
    private Vector3 lastPosition;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool isGrabbed = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
        
        // Paddle sits still until grabbed
        rb.isKinematic = true;
        lastPosition = transform.position;
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        // Enable physics when grabbed
        rb.isKinematic = false;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        // Freeze paddle when released so it doesnt fall
        rb.isKinematic = true;
        // Paddle stays wherever you let go
    }

    void FixedUpdate()
    {
        if (isGrabbed && boatRigidbody != null)
        {
            // Calculate paddle velocity
            Vector3 velocity = (transform.position - lastPosition) 
                               / Time.fixedDeltaTime;
            
            // Only use horizontal movement for paddling
            Vector3 force = -velocity * paddleForce;
            force.y = 0;
            
            // Apply force to boat
            boatRigidbody.AddForce(force);
        }
        lastPosition = transform.position;
    }
}