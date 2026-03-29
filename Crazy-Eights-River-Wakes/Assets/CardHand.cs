using UnityEngine;
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
    private XRSocketInteractor activeSocket;

    private List<bool> socketStates;

    public Transform cardAnchor;

    void Awake()
    {
        sockets = transform.GetChild(1);
        activeSocket = sockets.GetComponentInChildren<XRSocketInteractor>();

        InitializeHand();
    }

    private void InitializeHand()
    {
        heldCards = new List<Card>();
        socketStates = new List<bool>();
        
        for (int i=0; i<maxHandSize; i++)
        {
            socketStates.Add(false);
        }
    }

    public void AttachCardToHand()
    {
        Debug.Log("Card attached to hand.");
        var selected = activeSocket.GetOldestInteractableSelected();
        if (selected != null)
        {
            Card addedCard = selected.transform.gameObject.GetComponent<Card>();
            addedCard.DisablePhysics();
            heldCards.Add(addedCard);
        }

        UpdateActiveSocket();
        MakeCardFan();

        Debug.Log("You now have " + heldCards.Count + " cards in your hand.");
    }

    public void UpdateActiveSocket()
    {
        if (heldCards == null)
        {
            heldCards = new List<Card>();
        }

        if (heldCards.Count < maxHandSize)
        {
            activeSocket = sockets.GetChild(heldCards.Count).GetComponent<XRSocketInteractor>();
            activeSocket.gameObject.SetActive(true);

        }
    }

    public void PopCardFromHand(int index = -1)
    {
        Debug.Log("Card detached from hand.");
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
