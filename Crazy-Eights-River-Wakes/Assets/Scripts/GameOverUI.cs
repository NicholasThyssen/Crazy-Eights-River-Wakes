using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public string mainMenuSceneName = "Scenes/Menu";
    public Button mainMenuBtn;
    public GameObject winText;  // your "You Win!" text object
    public GameObject loseText; // your "You Lose!" text object

    private Canvas canvas;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
    }

    void Start()
    {
        gameObject.SetActive(false);
        mainMenuBtn.onClick.AddListener(OnMainMenuClicked);
    }

    public void Show(bool playerWon)
    {
        gameObject.SetActive(true); // activate first

        winText.SetActive(playerWon);
        loseText.SetActive(!playerWon);

        PositionInFrontOfPlayer(); // now runs on an active object
    }

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    private void PositionInFrontOfPlayer()
    {
        Camera cam = Camera.main;
        if (cam == null)
            cam = FindFirstObjectByType<Camera>();
        if (cam == null)
        {
            Debug.LogWarning("GameOverUI: no camera found, cam is null");
            return;
        }

        Debug.Log("Camera found: " + cam.name + " at position: " + cam.transform.position);

        if (canvas != null)
            canvas.worldCamera = cam;

        transform.position = cam.transform.position + cam.transform.forward * 1.0f;
        transform.LookAt(cam.transform);
        transform.Rotate(0, 180, 0);

        Debug.Log("Panel positioned at: " + transform.position);
    }
}