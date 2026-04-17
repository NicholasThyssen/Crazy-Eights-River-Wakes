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

    public Transform anchorPoint;

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

            BaseCharacter captured = p;
            btnObj.GetComponent<Button>().onClick.AddListener(() => Choose(captured));
        }

        PositionAtAnchor();
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

    public void PositionAtAnchor()
    {
        if (anchorPoint == null)
        {
            Debug.LogWarning($"{name}: No anchor point assigned.");
            return;
        }

        transform.position = anchorPoint.position;
        transform.rotation = anchorPoint.rotation;

        // Optional: ensure UI uses the main camera for world-space rendering
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
            canvas.worldCamera = Camera.main;
    }

}