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

    public Transform anchorPoint;

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
        winText.SetActive(playerWon);
        loseText.SetActive(!playerWon);

        PositionAtAnchor();
        gameObject.SetActive(true);
    }


    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    private void PositionAtAnchor()
    {
        if (anchorPoint == null)
        {
            Debug.LogWarning("GameOverUI: No anchor point assigned.");
            return;
        }

        transform.position = anchorPoint.position;
        transform.rotation = anchorPoint.rotation;

        if (canvas != null)
            canvas.worldCamera = Camera.main;
    }

}