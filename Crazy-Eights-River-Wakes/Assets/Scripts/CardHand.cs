using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// A physical card hand for holding cards. NOT the actual metaphorical hand that the characters use.
public class CardHand : MonoBehaviour
{
    public GameObject socketPrefab;
    public int maxHandSize = 20;
    public float fanSpread = 2.0f;
    [SerializeField] private bool isFanned;
    private BaseCharacter owner;
    private Transform lastKnownSocketPosition;
    private List<Card> heldCards;
    private Transform sockets;
    private HandSocket mainSocket;
    private Rigidbody rb;
    private Transform cardContainer;
    private Transform respawnAnchor;
    public int maxDistanceFromSpawn = 5;

    public Card socketIgnoreCard;

    public UnityEvent<Card> cardAdded;
    public UnityEvent<Card> cardRemoved;

    private bool useSocketInteractions = true;
    [SerializeField] private BoxCollider deckCollider;
    private XRGrabInteractable parentGrab;

    /* This is for a CRAZY trick I figured out for dealing with nested colliders
        when deck is already grabbed in one hand, reduce its collider to be reeeeeeeally small.
        This way it stays paired to the grabbing hand, but the other hand grabs the card instead of
        re-grabbing the deck
    */
    [SerializeField] private float fannedColliderScale = 0.001f;

    private Vector3 originalDeckColliderSize;
    private Vector3 originalDeckColliderCenter;

    // custom setter so that changing IsFanned in inspector actually fans/unfans
    public bool IsFanned
    {
        get => isFanned;
        set
        {
            isFanned = value;

            if (isFanned)
            {
                MakeCardFan();
            }
            else
            {
                MakeCardNotFan();
            }
        }
    }

    void Awake()
    {
        InitializeHand();
        parentGrab = GetComponent<XRGrabInteractable>();
        if (deckCollider != null)
        {
            originalDeckColliderSize = deckCollider.size;
            originalDeckColliderCenter = deckCollider.center;
        }

        parentGrab.selectEntered.AddListener(OnDeckGrabbed);
        parentGrab.selectExited.AddListener(OnDeckReleased);
        isFanned = false;
    }

    void Update()
    {
        // If the hand has a spawn anchor, check distance and warp back if too far
        if (respawnAnchor == null) return;

        float dist = Vector3.Distance(transform.position, respawnAnchor.position);
        if (dist > maxDistanceFromSpawn)
        {
            Debug.Log($"CardHand strayed {dist:F1}m from spawn — warping back.");
            Warpback();
        }
    }

    void OnValidate()
    {
        IsFanned = isFanned;
    }

    public void InitializeHand()
    {
        heldCards = new List<Card>();

        rb = GetComponent<Rigidbody>();
        sockets = transform.GetChild(1);
        cardContainer = transform.GetChild(2);
        mainSocket = cardContainer.GetChild(0).GetComponent<HandSocket>();

        mainSocket.selectEntered.AddListener(delegate {AttachCardFromMainSocket();});
    }

    public void DisableSocketInteractions()
    {
        mainSocket.gameObject.SetActive(false);
        useSocketInteractions = false;
    }

    public void SetRespawnAnchor(Transform respawnAnchor) => this.respawnAnchor = respawnAnchor;

    public void SetOwner(BaseCharacter owner) => this.owner = owner;
    public List<Card> PopAllCards()
    {
        mainSocket.enabled = false;
        List<Card> replacedCards = new List<Card>(heldCards);
        foreach (Card c in replacedCards)
        {
            RemoveCardFromHand(c);
        }
        heldCards.Clear();
        
        StartCoroutine(reenableSocket());

        return replacedCards;
    }

    IEnumerator reenableSocket()
    {
        yield return new WaitForSeconds(0.2f);
        mainSocket.enabled = true;
    }

    public List<Card> GetHeldCards() => heldCards;

    public bool HasCardInHand(Card targetCard) => heldCards.Contains(targetCard);
    public void AddCardToHand(Card targetCard)
    {
        targetCard.gameObject.SetActive(true);
        targetCard.DisablePhysics();
        heldCards.Add(targetCard);
        StartCoroutine(DelayedEnableGrabInteractions(targetCard));

        targetCard.transform.SetParent(cardContainer);
        if (owner.CardShouldFan())
        {
            MakeCardFan();
        }
        
        else {
            MakeCardNotFan();
        }

        // ignore collisions between CardHand and cards. Stops weird self-intersecting collisions
        Collider collider = GetComponent<Collider>();
        Physics.IgnoreCollision(collider, targetCard.GetComponent<Collider>());

        cardAdded.Invoke(targetCard);
    }

    public void BulkAddCards(List<Card> cardsToAdd)
    {
        foreach (Card c in cardsToAdd)
        {
            AddCardToHand(c);
        }
    }

    IEnumerator DelayedEnableGrabInteractions(Card targetCard)
    {
        yield return new WaitForSeconds(0.2f);

        targetCard.EnableGrab();
        targetCard.ReRegisterGrabListeners(); // ? re-adds OnCardGrabbed + clears stale listeners

        var grabInteractable = targetCard.GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(delegate { CardGrabbedFromHand(targetCard); });
    }

    public void RemoveCardFromHand(Card targetCard)
    {
        var grabInteractable = targetCard.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
            grabInteractable.selectEntered.RemoveAllListeners();

        heldCards.Remove(targetCard);
        targetCard.transform.SetParent(null);
        targetCard.gameObject.SetActive(false); // ? hide the card visually

        // Reset socket
        if (mainSocket != null && mainSocket.gameObject.activeSelf)
        {
            mainSocket.gameObject.SetActive(false);
            mainSocket.gameObject.SetActive(true);
        }

        if (owner.CardShouldFan())
        {
            MakeCardFan(); // ? refresh fan after removal
        }
        else {
            MakeCardNotFan();
        }

        cardRemoved.Invoke(targetCard);
    }

    public void AttachCardFromMainSocket()
    {
        Debug.Log("Card attached to hand.");
        var selected = mainSocket.GetOldestInteractableSelected();
        if (selected != null)
        {
            mainSocket.interactionManager.SelectExit(mainSocket, selected);
            Card targetCard = selected.transform.gameObject.GetComponent<Card>();
            AddCardToHand(targetCard);
        }
    }

    public void CardGrabbedFromHand(Card targetCard)
    {
        if (targetCard == null) return;

        Debug.Log("CardGrabbedFromHand: " + targetCard.name);

        heldCards.Remove(targetCard);

        // Unparent BEFORE XR moves it so it's free in world space
        targetCard.transform.SetParent(null, true);
        targetCard.StopPhysicsMode();

        // Re-enable physics so card can be thrown/dropped naturally
        targetCard.EnablePhysics();

        var grabInteractable = targetCard.GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.RemoveAllListeners(); // clear hand listener
        grabInteractable.selectExited.AddListener(delegate { CardGrabExited(targetCard); });
    }


    IEnumerator reactivateSocket()
    {
        yield return new WaitForSeconds(0.2f);
        mainSocket.socketActive = true;
    }

    public void CardGrabExited(Card targetCard)
    {
        targetCard.GetComponent<XRGrabInteractable>().selectExited.RemoveAllListeners();
        targetCard.transform.SetParent(null, true);
        // Remove EnablePhysics() call � Force Gravity On Detach handles this now
    }

    public void AttachCardToHand(XRSocketInteractor eventSocket = null)
    {
        Debug.Log("Card attached to hand.");
        var selected = eventSocket.GetOldestInteractableSelected();
        if (selected != null)
        {
            Card addedCard = selected.transform.gameObject.GetComponent<Card>();
            addedCard.DisablePhysics();
            heldCards.Add(addedCard);
            cardAdded.Invoke(addedCard);
        }

        if (owner.CardShouldFan())
        {
            MakeCardFan();
        }
        
        else {
            MakeCardNotFan();
        }

        Debug.Log("You now have " + heldCards.Count + " cards in your hand.");
    }

    public void AddCardFromTeleport(Card targetCard)
    {
        AddCardToHand(targetCard);
    }

    // "Summon" the card to the hand
    public void SummonCardToHand(Card targetCard)
    {
        targetCard.DisablePhysics();
        targetCard.transform.SetParent(cardContainer, false);

        if (!heldCards.Contains(targetCard))
            heldCards.Add(targetCard);

        if (owner.CardShouldFan())
        {
            MakeCardFan();
        }
        
        else {
            MakeCardNotFan();
        }
        targetCard.StoreOriginalPosition();

        // ADD � re-enable grab after placement
        StartCoroutine(DelayedEnableGrabInteractions(targetCard));
    }


    public void ReturnHandToPlayer()
    {
        rb.linearVelocity = new Vector3(0.0f, 0.0f, 0.0f);
        // Play smoke effect?
        transform.position = lastKnownSocketPosition.position;
        transform.rotation = lastKnownSocketPosition.rotation;
    }
    

    // Use fanAngleOverride getFanAngle(cardsCount-1) if you want to add a card to deck without moving the other cards
    public void MakeCardFan(bool animate = true, float fanAngleOverride = -1f)
    {
        Debug.Log("FANNING");
        int cardCount = heldCards.Count;
        if (cardCount == 0 || cardContainer == null) return;

        float fanAngle = fanAngleOverride < 0 ? GetFanAngle(cardCount) : fanAngleOverride;
        float radius = 0.45f;
        float tilt = 15f;

        Vector3 center = cardContainer.position;
        Quaternion rotation = cardContainer.rotation;

        float startAngle = -fanAngle / 2f;
        float angleStep = cardCount > 1 ? fanAngle / (cardCount - 1) : 0f;

        for (int i = 0; i < cardCount; i++)
        {
            float angle = startAngle + angleStep * i;

            // Position cards in an arc
            Vector3 offset = Quaternion.Euler(0f, angle, 0f) * (Vector3.forward * radius);
            Vector3 localPos = rotation * offset * 0.5f;

            // IMPORTANT FIX: rotate inward, not outward
            Quaternion cardRot = rotation * Quaternion.Euler(tilt, -angle, 0f);

            // TODO: Make the card fan not be awkwardly offset from the origin
            StartCoroutine(Utils.AnimateTransform(heldCards[i].transform, localPos, cardRot, true, false, animate ? 0.5f : 0));
            // heldCards[i].transform.localPosition = localPos;
            // heldCards[i].transform.rotation = cardRot;
        }
        ShrinkDeckCollider();

        XRGrabInteractable[] childGrabs = GetComponentsInChildren<XRGrabInteractable>(true);
        foreach (var childGrab in childGrabs)
        {
            if (childGrab == parentGrab) continue;

            // make sure all children (cards) are grabbable
            childGrab.enabled = true;
            childGrab.interactionLayers = GameManager.instance.grabbableLayer;
        }

    }

    // vertically stacked instead of fanned
    protected void MakeCardNotFan(bool animate = false)
    {
        Debug.Log("UNFANNING");
        int cardCount = heldCards?.Count ?? 0;
        if (cardCount == 0 || cardContainer == null) return;

        float startZOffset = 0f;
        float offsetStep = 0.004f;

        for (int i = 0; i < cardCount; i++)
        {
            float offsetZ = startZOffset + offsetStep * i;

            Vector3 localPos = Vector3.forward * offsetZ;

            StartCoroutine(Utils.AnimateTransform(heldCards[i].transform, localPos, Quaternion.identity, true, true, animate ? 1.5f : 0 ));
        }

        RestoreDeckCollider();

        XRGrabInteractable[] childGrabs = GetComponentsInChildren<XRGrabInteractable>(true);
        foreach (var childGrab in childGrabs)
        {
            if (childGrab == parentGrab) continue;

            // individual cards should not be grabbable while not fanned
            childGrab.enabled = false;
            childGrab.interactionLayers = GameManager.instance.notGrabbableLayer;
        }
    }

    private void OnDeckGrabbed(SelectEnterEventArgs args)
    {
        Debug.Log("DECK GRABBED");
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        StartCoroutine(FanNextFrame());
    }

    private IEnumerator FanNextFrame()
    {
        yield return null;
        MakeCardFan();
    }

    private IEnumerator UnFanNextFrame()
    {
        yield return null;
        MakeCardNotFan();
    }

    private void OnDeckReleased(SelectExitEventArgs args)
    {
        Debug.Log("DECK RELEASED");
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        StartCoroutine(UnFanNextFrame());
    }

    private void ShrinkDeckCollider()
    {
        if (deckCollider == null) return;

        deckCollider.size = originalDeckColliderSize * fannedColliderScale;
        deckCollider.center = originalDeckColliderCenter;
    }

    private void RestoreDeckCollider()
    {
        if (deckCollider == null) return;

        deckCollider.size = originalDeckColliderSize;
        deckCollider.center = originalDeckColliderCenter;
    }

    public void Warpback()
    {
        if (respawnAnchor == null)
        {
            Debug.LogWarning("CardHand.Warpback: no respawnAnchor set.");
            return;
        }

        // Stop all motion before teleporting
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        transform.position = respawnAnchor.position;
        transform.rotation = respawnAnchor.rotation;
        Debug.Log("CardHand warped back to spawn.");
    }

    private float GetFanAngle(int cardCount)
    {
        if (cardCount <= 1) return 0f;

        float anglePerCard = 15f;
        float maxFanAngle = 115f;

        return Mathf.Min(maxFanAngle, anglePerCard * (cardCount - 1));
    }

    [HideInInspector] public bool GetIsFanned()
    {
        return this.isFanned;
    }
}
