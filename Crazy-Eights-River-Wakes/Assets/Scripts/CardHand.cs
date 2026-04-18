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
    private XRGrabInteractable parentGrab;

    private BoxCollider boxCollider;

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

        parentGrab.selectEntered.AddListener(OnDeckGrabbed);
        parentGrab.selectExited.AddListener(OnDeckReleased);
        boxCollider = GetComponent<BoxCollider>();
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

        UpdateCollider();   // size of hand grows when cards are added

        // ignore collisions between CardHand and cards. Stops weird self-intersecting collisions
        Collider collider = GetComponent<Collider>();
        if (collider)
        {
            Physics.IgnoreCollision(collider, targetCard.GetComponent<Collider>());
        }
        

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

        UpdateCollider();   // size of deck (hand) shrinks when cards are removed

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
        isFanned = true;
        int cardCount = heldCards.Count;
        if (cardCount == 0 || cardContainer == null) return;

        float fanAngle = fanAngleOverride < 0 ? GetFanAngle(cardCount) : fanAngleOverride;
        float radius = 0.45f;
        float tilt = 15f;

        float startAngle = -fanAngle / 2f;
        float angleStep = cardCount > 1 ? fanAngle / (cardCount - 1) : 0f;

        for (int i = 0; i < cardCount; i++)
        {
            float angle = startAngle + angleStep * i;

            // Position cards in an arc
            Vector3 offset = Quaternion.Euler(0f, angle, 0f) * (Vector3.back * radius);
            Vector3 localPos = offset * 0.5f + Vector3.forward * 0.225f;

            Quaternion localRot = Quaternion.Euler(tilt, angle, 0f);

            StartCoroutine(

                Utils.AnimateTransform(
                    heldCards[i].transform,
                    localPos,
                    localRot,
                    true,
                    true,
                    animate ? 0.5f : 0f
                )
            );
        }

        UpdateCollider();   // shrinks collider to size 0 so player doesn't grab this deck with other hand

        XRGrabInteractable[] childGrabs = GetComponentsInChildren<XRGrabInteractable>(true);
        foreach (var childGrab in childGrabs)
        {
            if (childGrab == parentGrab) continue;

            // make sure all children (cards) are grabbable
            childGrab.enabled = true;
            SetChildGrabLayerState(childGrab, true);
        }

    }

    // vertically stacked instead of fanned
    protected void MakeCardNotFan(bool animate = false)
    {
        Debug.Log("UNFANNING");
        isFanned = false;
        int cardCount = heldCards?.Count ?? 0;
        if (cardCount == 0 || cardContainer == null) return;

        float startZOffset = 0f;
        float offsetStep = 0.004f;

        for (int i = 0; i < cardCount; i++)
        {
            float offsetZ = startZOffset + offsetStep * i;

            Vector3 localPos = Vector3.forward * offsetZ;

            StartCoroutine(
                Utils.AnimateTransform(
                    heldCards[i].transform,
                    localPos,
                    Quaternion.identity,
                    true,
                    true,
                    animate ? 1.5f : 0f
                )
            );
        }

        UpdateCollider();

        XRGrabInteractable[] childGrabs = GetComponentsInChildren<XRGrabInteractable>(true);
        foreach (var childGrab in childGrabs)
        {
            if (childGrab == parentGrab) continue;

            // individual cards should not be grabbable while not fanned
            childGrab.enabled = false;
            SetChildGrabLayerState(childGrab, false);
        }
    }

    // switches between the "grabbable" and "not grabbale" layer without affecting other layers
    private void SetChildGrabLayerState(XRGrabInteractable childGrab, bool isGrabbable)
    {
        int currentLayers = childGrab.interactionLayers.value;
        int grabbableLayer = GameManager.instance.grabbableLayer.value;
        int notGrabbableLayer = GameManager.instance.notGrabbableLayer.value;

        // remove both Grabbale and NotGrabbable layers
        currentLayers &= ~grabbableLayer;
        currentLayers &= ~notGrabbableLayer;

        currentLayers |= isGrabbable ? grabbableLayer : notGrabbableLayer;

        // add new currentLayers to a mask and apply that mask
        InteractionLayerMask newMask = new InteractionLayerMask();
        newMask.value = currentLayers;
        childGrab.interactionLayers = newMask;
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

        float anglePerCard = 20f;
        float maxFanAngle = 115f;

        return Mathf.Min(maxFanAngle, anglePerCard * (cardCount - 1));
    }

    [HideInInspector] public bool GetIsFanned()
    {
        return this.isFanned;
    }

    private void UpdateCollider()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0 || isFanned)
        {
            boxCollider.size = Vector3.zero;
            return;
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        // Convert world bounds to local
        Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
        Vector3 localSize = transform.InverseTransformVector(bounds.size);

        // add some padding just in case collider gets blocked by child colliders?
        Vector3 padding = new Vector3(0.004f, 0.04f, 0.04f);
        boxCollider.center = localCenter;
        boxCollider.size = localSize + padding;
    }


}
