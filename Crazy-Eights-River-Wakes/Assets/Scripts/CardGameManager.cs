using System;
using System.Collections.Generic;
using UnityEngine;

// TODO : WILL NEED TO FIX THIS TO ENSURE IT WORKS AS INTENDED
public class CardGameManager : MonoBehaviour
{
    public static CardGameManager instance;
    private int currentTurnIdx;

    private BaseCharacter currentPlayerTurn;
    private bool waitingForEffect = false;

    public CardDeck deck;
    public CardDeck discardPile;

    private CardSuit currSuit;
    private CardRank currRank;

    public SuitSelectionUI suitUI;
    public SwapSelectionUI swapUI;

    private bool reversed = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Debug.Log("Manager deck = " + deck, deck);
        Debug.Log("Manager deck card count = " + (deck == null ? -1 : deck.GetCards().Count));

        Card firstCard = deck.DrawRandomCard();
        Debug.Log(firstCard);
        if (discardPile != null)
        {
            discardPile.AddCard(firstCard);
        }
        currRank = firstCard.rank;
        currSuit = firstCard.suit;

        if (currSuit == CardSuit.None)
        {
            Array values = System.Enum.GetValues(typeof(CardSuit));
            List<CardSuit> suits = new List<CardSuit>();
            foreach (CardSuit suit in values)
            {
                if (suit != CardSuit.None)
                    suits.Add(suit);
            }
            currSuit = suits[UnityEngine.Random.Range(0, suits.Count)];
        }

        currentTurnIdx = 0;
        List<BaseCharacter> players = GetPlayers();

        foreach (BaseCharacter character in players)
        {
            for (int i = 0; i < 5; i++)
            {
                character.AddCard(this.deck.DrawRandomCard());
            }
        }

        if (players != null && players.Count > 0)
        {
            currentPlayerTurn = players[currentTurnIdx];
            currentPlayerTurn.BeginCardTurn();
        }
    }

    void Update() { }

    private List<BaseCharacter> GetPlayers()
    {
        return GameManager.instance.characters;
    }

    public void EndTurn(BaseCharacter player, Card cardPlayed)
    {
        if (player != currentPlayerTurn)
        {
            throw new System.Exception("EndTurn was called by a player while it was not their turn");
        }

        if (cardPlayed != null)
        {
            currRank = cardPlayed.rank;
            if (cardPlayed.suit != CardSuit.None)
            {
                currSuit = cardPlayed.suit;
            }
        }

        CheckWin(player);
        HandleCardEffects(player, cardPlayed);

        if (!waitingForEffect)
        {
            AdvanceTurn();
        }
    }

    public CardSuit GetCurrSuit() => currSuit;
    public CardRank GetCurrRank() => currRank;

    public bool CanPlayCard(Card card)
    {
        if (card.rank == CardRank.Eight) return true;
        if (card.suit == CardSuit.None) return true;
        if (card.suit == currSuit || card.rank == currRank) return true;
        return false;
    }

    private void HandleCardEffects(BaseCharacter player, Card cardPlayed)
    {
        if (cardPlayed == null) return;

        switch (cardPlayed.rank)
        {
            case CardRank.Eight:
                waitingForEffect = true;
                RequestSuitChoice(player);
                Debug.Log("Suit changed!");
                return;

            case CardRank.Skip:
                currentTurnIdx = (currentTurnIdx + 1) % GetPlayers().Count;
                Debug.Log("Next player skipped!");
                break;

            case CardRank.Reverse:
                ReverseTurnOrder();
                Debug.Log("Turn order reversed!");
                break;

            case CardRank.PlusOne:
                BaseCharacter next = GetPlayers()[(currentTurnIdx + 1) % GetPlayers().Count];
                next.AddCard(deck.DrawRandomCard());
                Debug.Log("Next player draws +1");
                break;

            case CardRank.Swap:
                waitingForEffect = true;
                RequestSwapChoice(player);
                Debug.Log("Hands swapped!");
                return;
        }
    }

    public void RequestSuitChoice(BaseCharacter player) => suitUI.Show(player);
    public void RequestSwapChoice(BaseCharacter player) => swapUI.Show(player, GetPlayers());

    public void OnSuitChosen(CardSuit chosenSuit)
    {
        currSuit = chosenSuit;
        Debug.Log("Suit chosen: " + chosenSuit);
        ContinueTurnAfterEffect();
    }

    public void OnSwapChosen(BaseCharacter target)
    {
        var temp = currentPlayerTurn.GetHand();
        currentPlayerTurn.SetHand(target.GetHand());
        target.SetHand(temp);
        Debug.Log("Swapped hands with: " + target.name);
        ContinueTurnAfterEffect();
    }

    private void ReverseTurnOrder() => reversed = !reversed;

    private void ContinueTurnAfterEffect()
    {
        waitingForEffect = false;
        AdvanceTurn();
    }

    void CheckWin(BaseCharacter player)
    {
        if (player.hand.Count == 0)
        {
            Debug.Log(player.name + " WINS!");
            enabled = false;
        }
    }

    public bool CurrentPlayerIs(BaseCharacter player) => currentPlayerTurn == player;

    private void AdvanceTurn()
    {
        int count = GetPlayers().Count;

        if (!reversed)
            currentTurnIdx = (currentTurnIdx + 1) % count;
        else
            currentTurnIdx = (currentTurnIdx - 1 + count) % count;

        currentPlayerTurn = GetPlayers()[currentTurnIdx];
        currentPlayerTurn.BeginCardTurn();
    }

    // Called by Card.OnPointerClick
    public void OnCardClicked(Card card)
    {
        // Must be the human player's turn
        HumanPlayer humanPlayer = currentPlayerTurn as HumanPlayer;
        if (humanPlayer == null)
        {
            Debug.Log("It's not the human's turn.");
            return;
        }

        // Find card in hand by instance ID to avoid reference mismatch
        int clickedID = card.GetInstanceID();
        Card cardInHand = humanPlayer.hand.Find(c => c.GetInstanceID() == clickedID);

        if (cardInHand == null)
        {
            Debug.Log($"Card {card.rank} of {card.suit} not found in hand. Hand has {humanPlayer.hand.Count} cards.");
            foreach (var c in humanPlayer.hand)
                Debug.Log($"  Hand card: {c.rank} of {c.suit} (ID: {c.GetInstanceID()})");
            Debug.Log($"  Clicked card ID: {clickedID}");
            return;
        }

        // Card must be valid to play
        if (!CanPlayCard(cardInHand))
        {
            Debug.Log($"Cannot play {cardInHand.rank} of {cardInHand.suit}. Current suit: {currSuit}, current rank: {currRank}.");
            return;
        }

        // Remove from hand, add to discard, end turn
        Debug.Log($"Playing {cardInHand.rank} of {cardInHand.suit}.");
        humanPlayer.RemoveCard(cardInHand);
        discardPile.AddCard(cardInHand);
        EndTurn(humanPlayer, cardInHand);
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
    Eight,
    Nine,
    Ten,
    Reverse,
    Swap,
    Skip,
    PlusOne
}