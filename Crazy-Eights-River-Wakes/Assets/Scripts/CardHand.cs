using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

public class MainSocketFilter : MonoBehaviour, IXRHoverFilter
{
    public bool canProcess => isActiveAndEnabled;

    public List<Card> filterList;

    public void Awake()
    {
        filterList = new List<Card>();
    }

    public void UpdateFilterList(List<Card> filterList)
    {
        this.filterList = filterList;
    }

    public bool Process(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
    {
        Card interactorCard = interactable.transform.gameObject.GetComponent<Card>();
        if (interactorCard == null)
        {
            return false;
        }
        return !filterList.Contains(interactorCard);
    }
}

// A physical card hand for holding cards. NOT the actual metaphorical hand that the characters use.
public class CardHand : MonoBehaviour
{
    public GameObject socketPrefab;
    public int maxHandSize = 20;
    public float fanSpread = 2.0f;
    private BaseCharacter owner;
    private Transform lastKnownSocketPosition;
    private MainSocketFilter filter;
    private List<Card> heldCards;
    private Transform fallDetector;
    private Transform sockets;
    private XRSocketInteractor currentSocket;
    private XRSocketInteractor mainSocket;
    private List<XRSocketInteractor> activeSockets;
    private Stack<XRSocketInteractor> inactiveSockets;
    private List<(XRSocketInteractor, Card)> socketCardPairs;
    private Rigidbody rb;
    private Transform cardContainer;

    public UnityEvent<Card> cardAdded;
    public UnityEvent<Card> cardRemoved;

    void Awake()
    {
        InitializeHand();
    }

    public void InitializeHand()
    {
        heldCards = new List<Card>();

        rb = GetComponent<Rigidbody>();
        sockets = transform.GetChild(1);
        fallDetector = transform.GetChild(2);
        cardContainer = transform.GetChild(3);
        mainSocket = cardContainer.GetChild(0).GetComponent<XRSocketInteractor>();
        filter = new MainSocketFilter();

        //mainSocket.hoverFilters.Add(filter);
        filter.UpdateFilterList(heldCards);

        activeSockets = new List<XRSocketInteractor>();
        inactiveSockets = new Stack<XRSocketInteractor>();
        socketCardPairs = new List<(XRSocketInteractor, Card)>();

        mainSocket.selectEntered.AddListener(delegate {AttachCardFromMainSocket();});
    }

    public void SetOwner(BaseCharacter owner)
    {
        this.owner = owner;
    }

    public void Clear()
    {
        
    }

    public List<Card> GetHeldCards()
    {
        return heldCards;
    }

    public bool HasCardInHand(Card targetCard)
    {
        return heldCards.Contains(targetCard);
    }

    public void AddCardToHand(Card targetCard)
    {
        mainSocket.enabled = false;
        targetCard.transform.SetParent(cardContainer);
        targetCard.DisablePhysics();
        heldCards.Add(targetCard);

        targetCard.GetComponent<XRGrabInteractable>().selectEntered.AddListener(delegate {CardGrabbedFromHand(targetCard);});

        filter.UpdateFilterList(heldCards);
        MakeCardFan();
        mainSocket.enabled = true;

        cardAdded.Invoke(targetCard);
    }

    public void RemoveCardFromHand(Card targetCard)
    {
        mainSocket.enabled = false;
        targetCard.GetComponent<XRGrabInteractable>().selectEntered.RemoveAllListeners();
        heldCards.Remove(targetCard);
        targetCard.transform.SetParent(null);

        filter.UpdateFilterList(heldCards);
        MakeCardFan();
        mainSocket.enabled = true;

        cardRemoved.Invoke(targetCard);
    }

    public void AttachCardFromMainSocket()
    {
        Debug.Log("Card attached to hand.");
        var selected = mainSocket.GetOldestInteractableSelected();
        if (selected != null)
        {
            Card targetCard = selected.transform.gameObject.GetComponent<Card>();
            AddCardToHand(targetCard);
        }
    }

    public void CardGrabbedFromHand(Card targetCard)
    {
        if (targetCard != null)
        {
            RemoveCardFromHand(targetCard);
            targetCard.EnablePhysics();
        }
    }

    public void OnFallbackWarpTriggered(Card targetCard)
    {
        if (!heldCards.Contains(targetCard) && owner.HasCard(targetCard))
        {
            
        }
        else
        {
            targetCard.fallbackWarpTriggered.RemoveListener(OnFallbackWarpTriggered);
        }
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
            socketCardPairs.Add((currentSocket, addedCard));
        }

        UpdateCurrentSocket();
        MakeCardFan();

        Debug.Log("You now have " + heldCards.Count + " cards in your hand.");
    }

    public void UpdateCurrentSocket()
    {
        if (heldCards.Count < maxHandSize)
        {
            XRSocketInteractor nextSocketCandidate = inactiveSockets.Pop();
            currentSocket = nextSocketCandidate;
            activeSockets.Add(currentSocket);
            currentSocket.gameObject.SetActive(true);
        }
    }

    public void AddCardFromTeleport(Card targetCard)
    {
        AddCardToHand(targetCard);
    }

    // "Summon" the card to the hand
    public void SummonCardToHand(Card targetCard)
    {
        targetCard.DisablePhysics();
        targetCard.gameObject.transform.position = mainSocket.transform.position;
    }

    public void PopCardFromHand(XRSocketInteractor eventSocket = null)
    {
        Debug.Log("Card detached from socket " + eventSocket + ".");
        Debug.Log(socketCardPairs);
        var foundTuple = socketCardPairs.Find(x => x.Item1 == eventSocket);
        socketCardPairs.Remove(foundTuple);
        Debug.Log("Event socket pair: " + foundTuple);
        heldCards.Remove(foundTuple.Item2);
        activeSockets.Remove(eventSocket);
        inactiveSockets.Push(eventSocket);
        cardRemoved.Invoke(foundTuple.Item2);
        eventSocket.gameObject.SetActive(false);

        MakeCardFan();

        Debug.Log("You now have " + heldCards.Count + " cards in your hand.");
    }

    public void ReturnHandToPlayer()
    {
        rb.velocity = new Vector3(0.0f, 0.0f, 0.0f);
        // Play smoke effect?
        transform.position = lastKnownSocketPosition.position;
        transform.rotation = lastKnownSocketPosition.rotation;
    }
    
    public void MakeCardFan()
    {
        foreach (Transform cardTransform in cardContainer)
        {
            cardTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
    }
}
