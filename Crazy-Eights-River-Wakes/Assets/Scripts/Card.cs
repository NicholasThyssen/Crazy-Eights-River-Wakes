using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public CardSuit suit;
    public CardRank rank;

    public BaseCharacter owner;
    protected Rigidbody rb;
    protected BoxCollider collider;
    protected XRGrabInteractable grab;

    public UnityEvent<Card> fallbackWarpTriggered;

    public bool currentlyHeld = false;

    protected virtual void Awake()
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

    private bool forcePhysics = false;

    public void ForcePhysicsMode()
    {
        forcePhysics = true;
        rb.isKinematic = false;
        rb.useGravity = true;
        transform.SetParent(null, true);
    }

    public void StopPhysicsMode()
    {
        forcePhysics = false;
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;      // ? ADD
        rb.angularVelocity = Vector3.zero;     // ? ADD
        transform.hasChanged = false;
    }

    private void Update()
    {
        // If card falls too far, warp back to hand
        if (transform.position.y < -1.0f) // adjust threshold as needed
        {
            Warpback();
        }
    }

    private void LateUpdate()
    {
        if (forcePhysics)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            transform.hasChanged = false;
        }

        // ? ADD: while held by controller, force kinematic every frame
        if (currentlyHeld)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }




    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!rb.isKinematic) return; // physics mode ? do NOT override transform

        if (isHovered) return;
        isHovered = true;

        transform.localPosition = originalLocalPos + new Vector3(0, 0.05f, -0.1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!rb.isKinematic) return; // physics mode ? do NOT override transform

        if (!isHovered) return;
        isHovered = false;

        transform.localPosition = originalLocalPos;
    }



    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked card:" + name);
        CardGameManager.instance.OnCardClicked(this);
    }



    // Call this when card is added to hand, so we can return to this position after hover
    public void StoreOriginalPosition()
    {
        originalLocalPos = transform.localPosition;
    }

    // Alternative idea: perform an activation interaction on cards to play them when in XR mode
    public void OnActivated(ActivateEventArgs activateEvent)
    {
        
    }

    public void SetOwner(BaseCharacter owner) => this.owner = owner;

    public void EnablePhysics()
    {
        Debug.Log("EnablePhysics CALLED on " + name);
        Debug.Log("EnablePhysics: parent=" + transform.parent);
        rb.isKinematic = false;
        rb.useGravity = true;
    }


    public void DisablePhysics()
    {
        Debug.Log("DisablePhysics CALLED on " + name);
        rb.isKinematic = true;
        rb.useGravity = false;
    }


    public void EnableGrab()
    {
        if (grab == null)
        {
            grab = gameObject.GetComponent<XRGrabInteractable>();
        }
        // TESTING: IS CARD BEING HOVERED OVER?
        //if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        grab.enabled = true;
    }

    public void DisableGrab()
    {
        if (grab == null)
        {
            grab = gameObject.GetComponent<XRGrabInteractable>();
        }
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

    public void Warpback()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        StopPhysicsMode(); // ? ADD THIS — clears forcePhysics before hand tries to place it
        if (owner != null)
        {
            owner.WarpCardToHand(this);
        }
        else
        {
            CardGameManager cgm = CardGameManager.instance;
            transform.position = cgm.deck.transform.position;
            transform.rotation = cgm.deck.transform.rotation;
        }
    }


    protected void OnEnable()
    {
        if (grab == null)
            grab = GetComponent<XRGrabInteractable>();

        grab.selectEntered.AddListener(OnCardGrabbed);
    }

    protected void OnDisable()
    {
        if (grab != null)
            grab.selectEntered.RemoveListener(OnCardGrabbed);
    }

    private void OnCardGrabbed(SelectEnterEventArgs args)
    {
        if (owner == null)
        {
            Debug.Log("Card grabbed from deck: " + name);

            BaseCharacter current = CardGameManager.instance.GetCurrentPlayer();

            CardGameManager.instance.deck.RemoveCard(this); // remove from deck

            current.TeleportNewCardToHand(this);

            StoreOriginalPosition();
            DisableGrab();

            // ? ADD THIS — grabbing from deck ends your turn
            HumanPlayer human = current as HumanPlayer;
            if (human != null && CardGameManager.instance.IsPlayerTurn(human))
            {
                human.IncrementCardDraws();
                human.EndTurn();
            }
        }
    }

    // In Card.cs — add this public method
    public virtual void ReRegisterGrabListeners()
    {
        if (grab == null) grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.RemoveAllListeners();
        grab.selectExited.RemoveAllListeners();
        grab.selectEntered.AddListener(OnCardGrabbed);
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


