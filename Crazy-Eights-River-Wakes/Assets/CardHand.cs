using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class CardHand : MonoBehaviour
{
    public GameObject socketPrefab;
    public int playerId = 0;
    public int maxHandSize = 5;

    public float fanSpread = 2.0f;

    private List<Card> heldCards;
    
    private Transform sockets;
    private XRSocketInteractor currentSocket;

    private List<XRSocketInteractor> activeSockets;

    private Stack<XRSocketInteractor> inactiveSockets;
    
    private List<(XRSocketInteractor, Card)> socketCardPairs;

    public Transform cardAnchor;

    void Awake()
    {
        sockets = transform.GetChild(1);
        InitializeHand();
    }

    private void InitializeHand()
    {
        heldCards = new List<Card>();

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

    public void AttachCardToHand(XRSocketInteractor eventSocket = null)
    {
        Debug.Log("Card attached to hand.");
        var selected = eventSocket.GetOldestInteractableSelected();
        if (selected != null)
        {
            Card addedCard = selected.transform.gameObject.GetComponent<Card>();
            addedCard.DisablePhysics();
            heldCards.Add(addedCard);
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
        eventSocket.gameObject.SetActive(false);

        MakeCardFan();

        Debug.Log("You now have " + heldCards.Count + " cards in your hand.");
    }
    
    public void MakeCardFan()
    {
        foreach (Transform cardTransform in cardAnchor)
        {
            continue;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
