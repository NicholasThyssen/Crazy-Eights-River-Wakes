using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject menuUI;
    public Transform boat;
    public CameraLook cameraLook;

    public BoatPath boatPath;

    public Transform cameraAnchor;

    public void PlayGame()
    {
        menuUI.SetActive(false);

        Camera.main.transform.SetParent(cameraAnchor);
        Camera.main.transform.localPosition = Vector3.zero;
        Camera.main.transform.localRotation = Quaternion.identity;

        cameraLook.isGameMode = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        boatPath.isMoving = true;
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}