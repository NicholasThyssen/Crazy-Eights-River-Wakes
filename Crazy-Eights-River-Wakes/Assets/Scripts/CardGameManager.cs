using System;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

public class CardGameManager : MonoBehaviour
{
    public UnityEvent<int> currentPlayerChanged;
    public static CardGameManager instance;
    private int currentTurnIdx;
    private bool reverseOrder = false;
    
    private BaseCharacter currentPlayerTurn;
    public CardDeck deck;
    public CardDeck discardPile;    // this is where placed cards go. Players do not draw from here

    private CardSuit currSuit;
    private CardRank currRank;

    void Awake()
    {
        instance = this;
    }


    void Start()
    {

        // initialize first card (beginning suit/rank)
        Card firstCard = deck.DrawRandomCard();
        Debug.Log("FIRST CARD IS");
        Debug.Log(firstCard);
        if (discardPile != null)
        {
            discardPile.AddCard(firstCard);
        }
        currRank = firstCard.rank;
        currSuit = firstCard.suit;

        Debug.Log("First card rank/suit assigned");

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

        BeginFirstTurn();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void BeginFirstTurn()
    {
        Debug.Log("Beginning pre-round actions...");
       // start off at player 0's turn
        currentTurnIdx = 0;
        List<BaseCharacter> players = GetPlayers();

        Debug.Log("Drawing cards for each player...");

        // give them all cards to start off with
        foreach(BaseCharacter character in players)
        {
            for(int i = 0; i < 5; i++)
            {
                character.AddCard(deck.Pop());
            }
            
        }

        if (players != null && players.Count > 0)
        {
            Debug.Log("First player starts their turn.");
            currentPlayerTurn = players[currentTurnIdx];
            currentPlayerTurn.BeginCardTurn();  // notify player that it is their turn
        }        
    }

    private List<BaseCharacter> GetPlayers()
    {
        if (GameManager.instance.characters.Count > 0) {
            return GameManager.instance.characters;
        }
        else
        {
            // Let's put out this fire before it starts
            GameManager.instance.characters = GameManager.instance.BuildCharactersArray();
            return GameManager.instance.characters;
        }
    }

    // Call this from BaseCharacter when done drawing
    public void EndTurn(BaseCharacter player, Card cardPlayed)
    {
        if (player != currentPlayerTurn)
        {
            throw new System.Exception("EndTurn was called by a player while it was not their turn");
        }

        // update curr suit/rank
        if (cardPlayed != null)
        {
            currRank = cardPlayed.rank;
            if (cardPlayed.suit != CardSuit.None)
            {
                currSuit = cardPlayed.suit;
            } 
        }
        
        // TODO: Actually handle card logic here!!!!
        

        // move onto next player
        currentTurnIdx = (currentTurnIdx + 1) % GetPlayers().Count;
        currentPlayerTurn = GetPlayers()[currentTurnIdx];

        // notify player that it is their turn
        currentPlayerTurn.BeginCardTurn();
    }

    // Draw a single card from the main deck and add it to the XR hand the player is using to draw from the deck in addition to the player's card hand.
    public void DrawCardFromDeck(int playerIndex, Transform targetAttach)
    {
        Card nextCard = GetNextCardFromDeck();
    }

    // Draw any amount of cards from the main deck and add them to that player's card hand, with accompany animations/effects.
    public void DrawCardsForPlayer(int playerIndex, int amount = 1)
    {
        BaseCharacter targetPlayer = GetPlayers()[currentTurnIdx];

        Card nextCard = GetNextCardFromDeck();

        // Add cards to the target player's hand (animate them flying into the visible hand?)
    }

    public Card GetNextCardFromDeck()
    {
        if (deck.PeekTop() == null)
        {
            // If the deck is empty, replace the deck with the discard pile (sans the top card) and shuffle.
            // If the discard pile is somehow empty too (how???), spawn a fresh deck.
        }
        Card nextCard = deck.Pop();
        return nextCard;
    }

    public void SwapHands()
    {
        // Perform a swap action here with animations        
    }

    public void AllDrawOne()
    {
        // Play an effect here, all other players aside from the current draw one card
        // Animate cards auto-adding to player's hands
    }

    public void SkipNextPlayer()
    {
        // Play an effect here, set something to skip the next player
        // When progressing turn order, skipping = true
    }

    public void ReverseTurnOrder()
    {
        // Flip the bool, play an effect here
        reverseOrder = !reverseOrder;
    }

    public void ProgressTurnOrder(bool skipping = false)
    {
        // If skipping, turn order progresses by 2 instead of 1
        int orderProgression = (skipping ? 2 : 1) * (reverseOrder ? -1 : 1);
        // Move to next player in turn order
        // % is remainder not modulo, so add player count to handle negative cases
        // e.g. going from 3 to 2: (3-1+4) % 4 = 2
        // going from 2 to 0: (2+2+4) % 4 = 0
        // going from 0 to 1: (0+1+4) % 4 = 1
        // going from 1 to 3: (1-2+4) % 4 = 3
        currentTurnIdx = (currentTurnIdx + orderProgression + GetPlayers().Count) % GetPlayers().Count;
        // Set the current player to next in the order
        currentPlayerTurn = GetPlayers()[currentTurnIdx];
    }

    public void MoveToNextTurn()
    {
        // Nove to next player in the turn order;
        ProgressTurnOrder();
        // Notify player it's their turn
        // May want to send an event to the player here? But that can come when the logic is more fleshed out and animations are a bit furtehr developed.
        currentPlayerTurn.BeginCardTurn();
    }

    public CardSuit GetCurrSuit()
    {
        return this.currSuit;
    }

    public CardRank GetCurrRank()
    {
        return this.currRank;
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