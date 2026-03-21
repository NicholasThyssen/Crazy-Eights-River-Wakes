using System;
using System.Collections.Generic;
using UnityEngine;

public class CardGameManager : MonoBehaviour
{
    private int currentTurnIdx;
    private BaseCharacter currentPlayerTurn;
    public CardDeck deck;

    private CardSuit currSuit;
    private CardRank currRank;


    void Start()
    {

        // initialize first card (beginning suit/rank)
        Card firstCard = deck.DrawRandomCard();
        currRank = firstCard.rank;
        currSuit = firstCard.suit;

        // If suit is none, choose a random suit (other than none)
        if (currSuit == CardSuit.None)
        {
            Array values = System.Enum.GetValues(typeof(CardSuit));
            List<CardSuit> suits = new List<CardSuit>();

            foreach (CardSuit suit in values)
            {
                if (suit != CardSuit.None)
                {
                    suits.Add(suit);
                }
                
            }
            currSuit = suits[UnityEngine.Random.Range(0, suits.Count)];
        }
        
        // start off at player 0's turn
        currentTurnIdx = 0;
        List<BaseCharacter> players = GetPlayers();

        // give them all cards to start off with
        foreach(BaseCharacter character in players)
        {
            for(int i = 0; i < 5; i++)
            {
                character.AddCard(this.deck.DrawRandomCard());
            }
            
        }

        if (players != null && players.Count > 0)
        {
            currentPlayerTurn = players[currentTurnIdx];
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void HandleCardDrawn(BaseCharacter playerDrawing, Suit cardSuit, Rank cardRank)
    {

    }

    private List<BaseCharacter> GetPlayers()
    {
        return GameManager.instance.characters;
    }

    // Call this from BaseCharacter when done drawing
    public void EndTurn(BaseCharacter player)
    {
        if (player != currentPlayerTurn)
        {
            throw new System.Exception("EndTurn was called by a player while it was not their turn");
        }

        // move onto next player
        currentTurnIdx = (currentTurnIdx + 1) % GetPlayers().Count;
        currentPlayerTurn = GetPlayers()[currentTurnIdx];

        // notify player that it is their turn
        currentPlayerTurn.BeginCardTurn();
    }
}

public enum Suit
{
    Clubs,
    Diamonds,
    Hearts,
    Spades
}

public enum Rank
{
    Ace,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,      // special
    Nine,
    Ten,
    Reverse,    // special
    Swap,       // special
    Skip,       // special
    PlusOne     // special
}