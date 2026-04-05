using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public CardSuit suit;
    public CardRank rank;

    private Vector3 originalLocalPos;
    private bool isHovered = false;

    void Start()
    {
        originalLocalPos = transform.localPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Hover ENTER on card: " + name);

        if (isHovered) return;
        isHovered = true;

        transform.localPosition = originalLocalPos + new Vector3(0, 0.05f, -0.1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Hover EXIT on card: " + name);

        if (!isHovered) return;
        isHovered = false;

        transform.localPosition = originalLocalPos;
    }

    public void StoreOriginalPosition()
    {
        originalLocalPos = transform.localPosition;
    }
    void Update()
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            Debug.Log("Raycast hit: " + hit.collider.name);
        }
    }

}



public enum CardSuit
{
    Clubs,
    Diamonds,
    Hearts,
    Spades,
    None // used for 8 and Swap
}

public enum CardRank
{
    Ace,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,      // special
    Nine,
    Ten,
    Reverse,    // special
    Swap,       // special
    Skip,       // special
    PlusOne     // special
}


