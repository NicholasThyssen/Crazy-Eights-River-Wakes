using System;
using System.Collections.Generic;
using UnityEngine;

public class CardGameManager : MonoBehaviour
{
    public static CardGameManager instance;
    private int currentTurnIdx;
    
    private BaseCharacter currentPlayerTurn;
    public CardDeck deck;
    public CardDeck discardPile;    // this is where placed cards go. Players do not draw from here

    private CardSuit currSuit;
    private CardRank currRank;

    public SuitSelectionUI suitUI;
    public SwapSelectionUI swapUI;

    void Awake()
    {
        instance = this;
    }


    void Start()
    {

        Debug.Log("Manager deck = " + deck, deck);
        Debug.Log("Manager deck card count = " + (deck == null ? -1 : deck.GetCards().Count));

        // initialize first card (beginning suit/rank)
        Card firstCard = deck.DrawRandomCard();
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

    // Getter function to see all players
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

        // update curr suit/rank on top of stack
        if (cardPlayed != null)
        {
            currRank = cardPlayed.rank;
            if (cardPlayed.suit != CardSuit.None)
            {
                currSuit = cardPlayed.suit;
            } 
        }

        CheckWin(player); // If they win they win and game is over!

        // Looks at card played to see its effects on the game
        HandleCardEffects(player, cardPlayed);

        int count = GetPlayers().Count;

        // Move onto the next player by checking if reversed first
        if (!reversed)
        {
            currentTurnIdx = (currentTurnIdx + 1) % count;

        }
        else
        {
            currentTurnIdx = (currentTurnIdx - 1 + count) % count;
        }

        currentPlayerTurn = GetPlayers()[currentTurnIdx];

        // notify player that it is their turn
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

    // Checks if card can be played or not. True if playable else it can't be.
    public bool CanPlayCard(Card card)
    {
        // If 8, then playable
        if (card.rank == CardRank.Eight)
            return true;

        // If same rank or same suit, then playable
        if (card.suit == currSuit || card.rank == currRank)
            return true;

        return false;
    }

    // This handles different card scenarios
    private void HandleCardEffects(BaseCharacter player, Card cardPlayed)
    {
        if (cardPlayed == null)
            return;

        switch (cardPlayed.rank)
        {
            case CardRank.Eight:
                // Player chooses suit — for now pick random
                RequestSuitChoice(player);
                Debug.Log("Suit changed!");
                return;

            // Skips the next player who would go in the turn order
            case CardRank.Skip:
                currentTurnIdx = (currentTurnIdx + 1) % GetPlayers().Count;
                Debug.Log("Next player skipped!");
                break;

            // Reverses turn order of players
            case CardRank.Reverse:
                ReverseTurnOrder();
                Debug.Log("Turn order reversed!");
                break;

            // For now simply adds one card to next player but could make it so if next player has +1 they could play it
            case CardRank.PlusOne:
                BaseCharacter next = GetPlayers()[(currentTurnIdx + 1) % GetPlayers().Count];
                next.AddCard(deck.DrawRandomCard());
                Debug.Log("Next player draws +1");
                break;

            // Will swap hands with another player
            case CardRank.Swap:
                RequestSwapChoice(player);
                Debug.Log("Hands swapped!");
                return;
        }
    }

    // Will wit until they choose
    public void RequestSuitChoice(BaseCharacter player)
    {
        suitUI.Show(player);
    }

    public void RequestSwapChoice(BaseCharacter player)
    {
        swapUI.Show(player, GetPlayers());
    }

    public void OnSuitChosen(CardSuit chosenSuit)
    {
        currSuit = chosenSuit;
        Debug.Log("Suit chosen: " + chosenSuit);

        ContinueTurnAfterEffect();
    }

    public void OnSwapChosen(BaseCharacter target)
    {
        var players = GetPlayers();

        // Swap hands
        var temp = currentPlayerTurn.GetHand();
        currentPlayerTurn.SetHand(target.GetHand());
        target.SetHand(temp);

        Debug.Log("Swapped hands with: " + target.name);

        ContinueTurnAfterEffect();
    }


    // Initially set to false as we go in the correct turn order
    private bool reversed = false;

    // Reverse the turn order
    private void ReverseTurnOrder()
    {
        reversed = !reversed;
    }

    private void ContinueTurnAfterEffect()
    {
        int count = GetPlayers().Count;

        if (!reversed)
            currentTurnIdx = (currentTurnIdx + 1) % count;
        else
            currentTurnIdx = (currentTurnIdx - 1 + count) % count;

        currentPlayerTurn = GetPlayers()[currentTurnIdx];
        currentPlayerTurn.BeginCardTurn();
    }

    void CheckWin(BaseCharacter player)
    {
        if (player.hand.Count == 0)
        {
            Debug.Log(player.name + " WINS!");
            // optionally stop game
            enabled = false;
        }
    }

    public bool CurrentPlayerIs(BaseCharacter player)
    {
        return currentPlayerTurn == player;
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