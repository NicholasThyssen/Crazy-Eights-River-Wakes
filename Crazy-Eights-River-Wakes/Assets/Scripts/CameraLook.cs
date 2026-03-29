using UnityEngine;

public class CameraLook : MonoBehaviour
{
    public float mouseSensitivity = 50f;

    float xRotation = 0f;
    float yRotation = 0f;

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
       

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -45f, 45f);

            yRotation += mouseX;

            transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
        }
    }
}