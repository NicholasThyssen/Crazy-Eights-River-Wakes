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
    }

    void OnEnable()
    {
        clubsBtn.onClick.AddListener(() => Choose(CardSuit.Clubs));
        diamondsBtn.onClick.AddListener(() => Choose(CardSuit.Diamonds));
        heartsBtn.onClick.AddListener(() => Choose(CardSuit.Hearts));
        spadesBtn.onClick.AddListener(() => Choose(CardSuit.Spades));
    }

    // Shows UI based on the differnt suits available
    public void Show(BaseCharacter player)
    {
        requestingPlayer = player;
        gameObject.SetActive(true);
    }

    // Hides UI when player selects
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // Will show all buttons and wait for player to choose
    void Start()
    {
        
    }

    // After they choose, we make the new suit needed as top
    private void Choose(CardSuit suit)
    {
        Hide();
        CardGameManager.instance.OnSuitChosen(suit);
    }
}