using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// A physical card hand for holding cards. NOT the actual metaphorical hand that the characters use.
public class CardHand : MonoBehaviour
{
    public GameObject socketPrefab;
    public int playerId = 0;
    public int maxHandSize = 5;

    public float fanSpread = 2.0f;

    private BaseCharacter owner;

    private Transform lastKnownSocketPosition;

    private List<Card> heldCards;
    
    private Transform fallDetector;

    private Transform sockets;
    private XRSocketInteractor currentSocket;

    private List<XRSocketInteractor> activeSockets;

    private Stack<XRSocketInteractor> inactiveSockets;
    
    private List<(XRSocketInteractor, Card)> socketCardPairs;

    private Rigidbody rb;

    public Transform cardAnchor;

    public UnityEvent<Card> cardAdded;

    public UnityEvent<Card> cardRemoved;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        sockets = transform.GetChild(1);
        InitializeHand();
    }

    private void InitializeHand()
    {
        heldCards = new List<Card>();

        fallDetector = transform.GetChild(2);

        activeSockets = new List<XRSocketInteractor>();
        inactiveSockets = new Stack<XRSocketInteractor>();
        socketCardPairs = new List<(XRSocketInteractor, Card)>();

        for (int i=0; i<maxHandSize; i++)
        {
            var newSocket = Instantiate(socketPrefab);
            newSocket.transform.parent = sockets;
            newSocket.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            newSocket.transform.localPosition += new Vector3(-0.04f * i, 0.0f, 0.01f * i);
            XRSocketInteractor interactor = newSocket.GetComponent<XRSocketInteractor>();
            interactor.selectEntered.AddListener(delegate {AttachCardToHand(interactor);});
            interactor.selectExited.AddListener(delegate {PopCardFromHand(interactor);});
            inactiveSockets.Push(interactor);
            newSocket.SetActive(false);
        }

        UpdateCurrentSocket();
    }

    public void Clear()
    {
        
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

    // "Summon" the card to the hand, e.g.
    public void SummonCardToHand(Card targetCard, bool instant = false)
    {
        XRGrabInteractable targetGrab = targetCard.gameObject.GetComponent<XRGrabInteractable>();
        targetCard.DisablePhysics();
        targetGrab.enabled = false;
        if (instant)
        {
            targetCard.gameObject.transform.position = currentSocket.transform.position;
            targetGrab.enabled = true;
        }
        else
        {
            targetCard.gameObject.transform.position = currentSocket.transform.position;
            targetGrab.enabled = true;
        }

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
        foreach (Transform cardTransform in cardAnchor)
        {
            continue;
        }
    }
}
