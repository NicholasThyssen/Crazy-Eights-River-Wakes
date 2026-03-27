using System;
using System.Collections.Generic;
using UnityEngine;

public class CardGameManager : MonoBehaviour
{
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
            currentPlayerTurn.BeginCardTurn();  // notify player that it is their turn
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private List<BaseCharacter> GetPlayers()
    {
        return GameManager.instance.characters;
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

    public void AllDrawOne()
    {
        // Play an effect here, all other players aside from the current draw one card   
    }

    public void SkipNextPlayer()
    {
        // Play an effect here, set something to skip the next player
    }

    public void ReverseTurnOrder()
    {
        // Flip the bool, maybe play an effect here
        reverseOrder = !reverseOrder;
    }

    public void ProgressTurnOrder(bool skipping = false)
    {
        // If skipping, turn order progresses by 2 instead of 1
        orderProgression = (skipping ? 2 : 1) * (reverseOrder ? -1 : 1);
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
        progressTurnOrder();
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