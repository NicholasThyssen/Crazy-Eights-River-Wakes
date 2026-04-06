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
    private BaseCharacter owner;
    private Transform lastKnownSocketPosition;
    private List<Card> heldCards;
    private Transform sockets;
    private HandSocket mainSocket;
    private Rigidbody rb;
    private Transform cardContainer;
    private Transform respawnAnchor;

    public Card socketIgnoreCard;

    public UnityEvent<Card> cardAdded;
    public UnityEvent<Card> cardRemoved;

    private bool useSocketInteractions = true;

    void Awake()
    {
        InitializeHand();
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

    public void SetRespawnAnchor(Transform respawnAnchor)
    {
        this.respawnAnchor = respawnAnchor;
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
        targetCard.gameObject.SetActive(true);
        targetCard.DisablePhysics();
        heldCards.Add(targetCard);
        StartCoroutine(DelayedEnableGrabInteractions(targetCard));

        targetCard.transform.SetParent(cardContainer);
        MakeCardFan();

        cardAdded.Invoke(targetCard);
    }

    IEnumerator DelayedEnableGrabInteractions(Card targetCard)
    {
        yield return new WaitForSeconds(0.2f);
        targetCard.GetComponent<XRGrabInteractable>().selectEntered.AddListener(delegate {CardGrabbedFromHand(targetCard);});
    }

    public void RemoveCardFromHand(Card targetCard)
    {
        mainSocket.socketActive = false;
        targetCard.GetComponent<XRGrabInteractable>().selectEntered.RemoveAllListeners();
        heldCards.Remove(targetCard);
        targetCard.transform.SetParent(null);
        

        MakeCardFan();

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
        if (targetCard != null)
        {
            mainSocket.socketActive = false;
            socketIgnoreCard = targetCard;
            RemoveCardFromHand(targetCard);
            targetCard.EnablePhysics();
            targetCard.GetComponent<XRGrabInteractable>().selectExited.AddListener(delegate {CardGrabExited(targetCard);});
            //StartCoroutine(reactivateSocket());
            mainSocket.socketActive = true;
        }
    }

    IEnumerator reactivateSocket()
    {
        yield return new WaitForSeconds(0.2f);
        mainSocket.socketActive = true;
    }

    public void CardGrabExited(Card targetCard)
    {
        socketIgnoreCard = null;
        targetCard.GetComponent<XRGrabInteractable>().selectExited.RemoveAllListeners();
        targetCard.transform.SetParent(null);
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

        MakeCardFan();

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
        targetCard.gameObject.transform.position = mainSocket.transform.position;
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
        int cardCount = heldCards.Count;
        if (cardCount == 0 || cardContainer == null) return;

        float fanAngle = 55f;
        float radius = 0.45f;
        float tilt = 15f;

        Vector3 center = cardContainer.position;
        Quaternion rotation = cardContainer.rotation;

        float startAngle = -fanAngle / 2f;
        float angleStep = cardCount > 1 ? fanAngle / (cardCount - 1) : 0;

        for (int i = 0; i < cardCount; i++)
        {
            float angle = startAngle + angleStep * i;

            // Position cards in an arc
            Vector3 offset = Quaternion.Euler(0, angle, 0) * (Vector3.forward * radius);
            Vector3 localPos = rotation * offset * 0.5f;

            // IMPORTANT FIX: rotate inward, not outward
            Quaternion cardRot = rotation * Quaternion.Euler(tilt, -angle, 0f);

            // TODO: Make the card fan not be awkwardly offset from the origin

            heldCards[i].transform.localPosition = localPos;
            heldCards[i].transform.rotation = cardRot;
        }
    }

    public void Warpback()
    {
        transform.position = respawnAnchor.position;
        transform.rotation = respawnAnchor.rotation;
    }
}
