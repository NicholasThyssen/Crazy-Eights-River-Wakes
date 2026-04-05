using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public CardSuit suit;
    public CardRank rank;

    public BaseCharacter owner;
    protected Rigidbody rb;
    protected BoxCollider collider;
    protected XRGrabInteractable grab;

    public UnityEvent<Card> fallbackWarpTriggered;

    public bool currentlyHeld = false;

    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        collider = gameObject.GetComponent<BoxCollider>();
        grab = gameObject.GetComponent<XRGrabInteractable>();
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

    public void SetOwner(BaseCharacter owner)
    {
        this.owner = owner;
    }

    public void EnablePhysics()
    {
        rb.isKinematic = false;
        //collider.enabled = true;
    }

    public void DisablePhysics()
    {
        rb.isKinematic = true;
        //collider.enabled = false;
    }

    public void EnableGrab()
    {
        grab = gameObject.GetComponent<XRGrabInteractable>();
        grab.enabled = true;
    }

    public void DisableGrab()
    {
        grab = gameObject.GetComponent<XRGrabInteractable>();
        grab.enabled = false;
    }

    public bool IsValidMatch(Card rhs)
    {
        if (rank == CardRank.Eight || rhs.rank == CardRank.Eight)
        {
            return true;
        }
        else if (rank == CardRank.Swap || rhs.rank == CardRank.Swap)
        {
            return true;
        }
        else
        {
            return rank == rhs.rank || suit == rhs.suit;
        }
    }

}


public enum CardSuit
{
    Hearts = 0,
    Clubs = 1,
    Spades = 2,
    Diamonds = 3,
    None // used for 8 and Swap
}

public enum CardRank
{
    Ace = 0,
    Two = 1,
    Three = 2,
    Four = 3,
    Five = 4,
    Six = 5,
    Seven = 6,
    Eight = 7,      // special
    Nine = 8,
    Ten = 9,
    Reverse = 10,    // special / Jack
    Skip = 11,       // special / Queen
    PlusOne = 12,    // special / King
    Swap = 13,       // special (For the Swap card, should we make it a Joker? Since we've already represented all the standard ranks in a 52-card deck)
}


