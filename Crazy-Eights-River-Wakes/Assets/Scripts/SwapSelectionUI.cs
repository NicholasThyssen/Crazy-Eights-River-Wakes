using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SwapSelectionUI : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform buttonContainer;

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
    }

    public void Show(BaseCharacter player, List<BaseCharacter> players)
    {
        requestingPlayer = player;

        // Clear old buttons
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        // Create a button for each other player
        foreach (BaseCharacter p in players)
        {
            if (p == player) continue;

            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
            // Support both legacy Text and TextMeshPro
            var legacyText = btnObj.GetComponentInChildren<Text>();
            if (legacyText != null)
                legacyText.text = p.name;
            var tmpText = btnObj.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
                tmpText.text = p.name;

            // Capture p in a local variable to avoid closure bug
            BaseCharacter captured = p;
            btnObj.GetComponent<Button>().onClick.AddListener(() => Choose(captured));
        }

        PositionInFrontOfPlayer();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Choose(BaseCharacter target)
    {
        CardGameManager.instance.OnSwapChosen(requestingPlayer, target);
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
            Debug.LogWarning("SwapSelectionUI: no camera found.");
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