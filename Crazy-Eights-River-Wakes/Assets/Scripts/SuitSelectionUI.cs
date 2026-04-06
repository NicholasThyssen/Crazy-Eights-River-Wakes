using UnityEngine;
using UnityEngine.UI;

public class SuitSelectionUI : MonoBehaviour
{
    public Button clubsBtn;
    public Button diamondsBtn;
    public Button heartsBtn;
    public Button spadesBtn;

    private BaseCharacter requestingPlayer;

    void Awake()
    {
        gameObject.SetActive(false);

        // Add listeners ONCE
        clubsBtn.onClick.AddListener(() => Choose(CardSuit.Clubs));
        diamondsBtn.onClick.AddListener(() => Choose(CardSuit.Diamonds));
        heartsBtn.onClick.AddListener(() => Choose(CardSuit.Hearts));
        spadesBtn.onClick.AddListener(() => Choose(CardSuit.Spades));
    }

    void OnDisable()
    {
        // Prevent listener stacking
        clubsBtn.onClick.RemoveAllListeners();
        diamondsBtn.onClick.RemoveAllListeners();
        heartsBtn.onClick.RemoveAllListeners();
        spadesBtn.onClick.RemoveAllListeners();

        // Re-add them cleanly
        clubsBtn.onClick.AddListener(() => Choose(CardSuit.Clubs));
        diamondsBtn.onClick.AddListener(() => Choose(CardSuit.Diamonds));
        heartsBtn.onClick.AddListener(() => Choose(CardSuit.Hearts));
        spadesBtn.onClick.AddListener(() => Choose(CardSuit.Spades));
    }

    public void Show(BaseCharacter player)
    {
        requestingPlayer = player;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Choose(CardSuit suit)
    {
        Hide();
        CardGameManager.instance.OnSuitChosen(suit);
    }
}