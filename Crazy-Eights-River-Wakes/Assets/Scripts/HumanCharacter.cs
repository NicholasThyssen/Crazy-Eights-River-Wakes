using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

public class HumanPlayer : BaseCharacter
{

    public Transform handAttach;
    public Card cardPrefab;
    public Transform cardAnchor;

    public bool usingPhysicalHand = true;

    //TESTING ONLY
    /*void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            if (cardPrefab != null)
            {
                Card newCard = Instantiate(cardPrefab, this.transform);
                Debug.Log("Instantiated card: " + newCard.name);
                AddCard(newCard);
            }
            else
            {
                Debug.LogWarning("Card prefab is not assigned!");
            }
        }
    }*/

    public void CreateHand()
    {
        GameObject playerHandObject = Instantiate(cardHandPrefab);
        playerHand.InitializeHand();
        playerHand = playerHandObject.GetComponent<CardHand>();
        playerHand.SetOwner(this);
        if (!usingPhysicalHand)
        {
            playerHand.DisableSocketInteractions();
        }
        //playerHand.DisableSocketInteractions();
        playerHandObject.transform.SetParent(handAttach);
        playerHandObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        playerHandObject.SetActive(true);
    }

    public void AssignListeners()
    {
        CardGameManager cgm = CardGameManager.instance;
        // Assign listeners to our own signal
        playerPlayedCard.AddListener(cgm.PlayerPlayedCard);
        playerTurnEnded.AddListener(cgm.PlayerTurnEnded);
        // Listen to the manager's signals
        cgm.beginPlayerTurn.AddListener(BeginPlayerTurn);
        // Connect to the discard pile
        if (usingPhysicalHand) {
            CardDeck pile = cgm.discardPile;
            pile.cardPlayedToDeck.AddListener(PlayedToDiscardPile);
        }
    }


    public override void BeginPlayerTurn(BaseCharacter player)
    {
        if (player == this)
        {
            
            Debug.Log("Player begin turn event received!");
            ShowPlayableCards();
        }
    }

    public override void FinishPlayerTurn(BaseCharacter player)
    {
        if (player == this)
        {
            EndTurn();
        }
    }

    public void PlayedToDiscardPile(BaseCharacter player, Card card)
    {
        if (player == this)
        {
            playerPlayedCard.Invoke(this, card);
        }
    }

    // Example method to highlight playable cards
    private void ShowPlayableCards()
    {
        foreach (var card in GetHand())
        {
            bool canPlay = CardGameManager.instance.CanPlayCard(card);
            // Update your card UI to enable/disable selection based on canPlay
            // (Implementation depends on your UI system)
        }
    }

    // Call this from your UI when a card is selected
    public void OnCardSelected(Card selectedCard)
    {
        // If we can play card we do!
        if (CardGameManager.instance.CanPlayCard(selectedCard))
        {
            PlayCardToDeck(selectedCard, CardGameManager.instance.discardPile);
            playerPlayedCard.Invoke(this, selectedCard);
        }
        // If we cannot play card warn in debug
        else
        {
            Debug.Log("Cannot play this card!");
        }
    }

    // Call this from your UI when the player chooses to draw
    public void OnDrawCard()
    {
        Card drawnCard = CardGameManager.instance.deck.Pop();
        TeleportNewCardToHand(drawnCard);
        // Optionally, check if the drawn card is playable and allow immediate play
    }

    protected override void FanOutHand()
    {
        playerHand.MakeCardFan();
        /* 
        List<Card> hand = GetHand();
        int cardCount = hand.Count;
        if (cardCount == 0 || cardAnchor == null) return;

        float fanAngle = 55f;
        float radius = 0.45f;
        float tilt = 15f;

        Vector3 center = cardAnchor.position;
        Quaternion rotation = cardAnchor.rotation;

        float startAngle = -fanAngle / 2f;
        float angleStep = cardCount > 1 ? fanAngle / (cardCount - 1) : 0;

        for (int i = 0; i < cardCount; i++)
        {
            float angle = startAngle + angleStep * i;

            // Position cards in an arc
            Vector3 offset = Quaternion.Euler(0, angle, 0) * (Vector3.forward * radius);
            Vector3 worldPos = center + rotation * offset;

            // IMPORTANT FIX: rotate inward, not outward
            Quaternion cardRot = rotation * Quaternion.Euler(tilt, -angle, 0f);

            hand[i].transform.position = worldPos;
            hand[i].transform.rotation = cardRot;

            //TESTING
            hand[i].transform.localPosition = hand[i].transform.localPosition;
            hand[i].GetComponent<Card>().StoreOriginalPosition();

        }
        */
    }


}

