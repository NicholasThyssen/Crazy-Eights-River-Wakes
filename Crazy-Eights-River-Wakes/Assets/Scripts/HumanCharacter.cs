using UnityEngine;
using System.Collections.Generic;

public class HumanPlayer : BaseCharacter
{

    // Card image and when in front of player we want to anchor card when they grab it
    public Card cardPrefab;
    public Transform cardAnchor;

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

    // This is called by the game manager when it's this player's turn
    public override void BeginCardTurn()
    {
        Debug.Log("Human player's turn started: " + name);

        // 1. Highlight/select only playable cards in the UI
        ShowPlayableCards();


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
            RemoveCard(selectedCard);
            CardGameManager.instance.discardPile.AddCard(selectedCard);
            EndTurn(selectedCard);
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
        Card drawnCard = CardGameManager.instance.deck.DrawRandomCard();
        AddCard(drawnCard);
        // Optionally, check if the drawn card is playable and allow immediate play
    }

    public override void TryPlayCard(Card card)
    {
        // Not your turn
        Debug.Log("Is it my turn? " + CardGameManager.instance.CurrentPlayerIs(this));

        if (!CardGameManager.instance.CurrentPlayerIs(this))
            return;

        // Illegal move
        if (!CardGameManager.instance.CanPlayCard(card))
            return;

        // Remove from hand
        hand.Remove(card);

        // Add to discard pile
        CardGameManager.instance.discardPile.AddCard(card);

        // Notify manager
        CardGameManager.instance.EndTurn(this, card);
    }


    protected override void FanOutHand()
    {
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
    }


}

