using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SuitSelectionUI : MonoBehaviour
{
    public Button heartsBtn;
    public Button clubsBtn;
    public Button spadesBtn;
    public Button diamondsBtn;
    public Transform anchorPoint;

    private BaseCharacter requestingPlayer;
    private Canvas canvas;

    public TMPro.TMP_Text announcementText;

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
        PositionAtAnchor();
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

    public void PositionAtAnchor()
    {   
        if (anchorPoint == null)
        {
            Debug.LogWarning($"{name}: No anchor point assigned.");
            return;
        }

        transform.position = anchorPoint.position;
        transform.rotation = anchorPoint.rotation;

        if (canvas != null)
            canvas.worldCamera = Camera.main;
    }

    public void ShowAnnouncement(CardSuit suit, float duration = 10f)
    {
        if (announcementText != null)
            announcementText.text = "Suit changed to: " + suit.ToString();

        heartsBtn.gameObject.SetActive(false);
        clubsBtn.gameObject.SetActive(false);
        spadesBtn.gameObject.SetActive(false);
        diamondsBtn.gameObject.SetActive(false);

        PositionAtAnchor();
        gameObject.SetActive(true);
        StartCoroutine(AutoHide(duration));
    }

    private IEnumerator AutoHide(float duration)
    {
        yield return new WaitForSeconds(duration);
        heartsBtn.gameObject.SetActive(true);
        clubsBtn.gameObject.SetActive(true);
        spadesBtn.gameObject.SetActive(true);
        diamondsBtn.gameObject.SetActive(true);
        Hide();
    }

}