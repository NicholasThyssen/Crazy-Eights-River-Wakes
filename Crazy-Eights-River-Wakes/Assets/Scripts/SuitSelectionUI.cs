using UnityEngine;
using UnityEngine.UI;

public class SuitSelectionUI : MonoBehaviour
{
    public Button heartsBtn;
    public Button clubsBtn;
    public Button spadesBtn;
    public Button diamondsBtn;

    private BaseCharacter requestingPlayer;
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

        heartsBtn.onClick.AddListener(() => Choose(CardSuit.Hearts));
        clubsBtn.onClick.AddListener(() => Choose(CardSuit.Clubs));
        spadesBtn.onClick.AddListener(() => Choose(CardSuit.Spades));
        diamondsBtn.onClick.AddListener(() => Choose(CardSuit.Diamonds));
    }

    public void Show(BaseCharacter player)
    {
        requestingPlayer = player;
        PositionInFrontOfPlayer();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Choose(CardSuit suit)
    {
        CardGameManager.instance.OnSuitChosen(requestingPlayer, suit);
        Hide();
    }

    private void PositionInFrontOfPlayer()
    {
        // Find camera — works for both regular and VR setups
        Camera cam = Camera.main;
        if (cam == null)
            cam = FindFirstObjectByType<Camera>();
        if (cam == null)
        {
            Debug.LogWarning("SuitSelectionUI: no camera found.");
            return;
        }

        // Assign event camera so World Space canvas buttons receive clicks
        if (canvas != null)
            canvas.worldCamera = cam;

        transform.position = cam.transform.position + cam.transform.forward * 1.0f;
        transform.LookAt(cam.transform);
        transform.Rotate(0, 180, 0);
    }
}