using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SwapSelectionUI : MonoBehaviour
{
    public Transform buttonContainer;
    public GameObject playerButtonPrefab;

    private BaseCharacter requestingPlayer;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    // Shows the UI based on players available
    public void Show(BaseCharacter player, List<BaseCharacter> players)
    {
        requestingPlayer = player;

        // Clear old buttons
        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        // Create a button for each other player
        foreach (var p in players)
        {
            if (p == player) continue;

            GameObject btnObj = Instantiate(playerButtonPrefab, buttonContainer);
            btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = p.name;

            btnObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                Choose(p);
            });
        }

        gameObject.SetActive(true);
    }

    // After they choose it hides the buttons
    public void Hide()
    {
        gameObject.SetActive(false);
    }


    private void Choose(BaseCharacter target)
    {
        Hide();
        CardGameManager.instance.OnSwapChosen(target);
    }
}