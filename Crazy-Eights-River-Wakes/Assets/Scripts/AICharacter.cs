using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class AICharacter : BaseCharacter
{
    public GameObject deckAttach;
    public GameObject cardAttach;
    public override void BeginCardTurn()
    {
        StartCoroutine(HandleCardTurn());
    }

    IEnumerator HandleCardTurn()
    {
        yield return new WaitForSeconds(2f);

        // If player has a card that can be played (correct rank/suit), play it
        List<Card> validCards = this.cardDeck.GetCards().Where(card=>card.suit == CardGameManager.instance.GetCurrSuit() || card.rank == CardGameManager.instance.GetCurrRank() || card.suit == CardSuit.None).ToList();
        Card cardToPlay = null;
        if (validCards.Count > 0)
        {
            cardToPlay = ChooseRandomCard(validCards);
        }
        else    // otherwise, draw one card from the deck. If it can be played, then play it. Otherwise, skip
        {
            DrawNewCard();
            yield return new WaitForSeconds(1f);
            validCards = this.cardDeck.GetCards().Where(card=>card.suit == CardGameManager.instance.GetCurrSuit() || card.rank == CardGameManager.instance.GetCurrRank() || card.suit == CardSuit.None).ToList();
            cardToPlay = ChooseRandomCard(validCards);
        }


        // move to right hand to signal that this card is about to be placed
        if (cardToPlay != null)
        {
            cardToPlay.transform.SetParent(cardAttach.transform);
            cardToPlay.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
        
        yield return new WaitForSeconds(2.5f);

        // play the card
        if (cardToPlay != null)
        {
            PlayCard(cardToPlay);
            yield return new WaitForSeconds(1f);
        }

        // End the turn
        EndTurn(cardToPlay);
    }

    // this represents drawing a NEW card from the main deck
    private void DrawNewCard()
    {
        Card drawnCard = CardGameManager.instance.deck.Pop();
        
        // add the drawn card to our deck
        this.cardDeck.AddCard(drawnCard);
    }

    // Chooses a card that is valid 
    private Card ChooseRandomCard(List<Card> validCards)
    {
        if (validCards == null || validCards.Count < 1)
            throw new ArgumentException(nameof(validCards), "validCards cannot be null or empty");

        int randomIdx = UnityEngine.Random.Range(0, validCards.Count);
        Card drawnCard = validCards[randomIdx];
        return drawnCard;
    }

    void Start()
    {

    }

    void Update()
    {

    }

    // this function is for placing a card down (in the discard pile)
    private void PlayCard(Card cardToPlay)
    {
        // remove card from player's cards
        this.cardDeck.RemoveCard(cardToPlay);
        CardGameManager.instance.discardPile.AddCard(cardToPlay);
    }

    protected override void SpawnCardDeck()
    {
        GameObject cardDeckGameObj = new GameObject("CardDeck");
        this.cardDeck = cardDeckGameObj.AddComponent<CardDeck>();
        cardDeck.transform.SetParent(this.deckAttach.transform);
        cardDeck.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }
}