using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.InputSystem;

using UnityEngine.Events;

// TODO : WILL NEED TO FIX THIS TO ENSURE IT WORKS AS INTENDED
// TODO : Check to make sure that the merge commit didn't shatter anything.
public class CardGameManager : MonoBehaviour
{
    public UnityEvent<int> currentPlayerChanged;
    public static CardGameManager instance;
    private int currentTurnIdx;
    private BaseCharacter currentPlayerTurn;
    private bool waitingForEffect = false;

    public CardDeck deck;
    public CardDeck discardPile;

    private CardSuit currSuit;
    private CardRank currRank;
    private bool reversed = false;

    public SuitSelectionUI suitUI;
    public SwapSelectionUI swapUI;

    public UnityEvent<BaseCharacter> beginPlayerTurn;
    public UnityEvent<BaseCharacter> cardPlayResolved;

    public UnityEvent<BaseCharacter, SuitSelectionUI> requestSuit;
    public UnityEvent<BaseCharacter, SwapSelectionUI, List<BaseCharacter>> requestSwap;

    public GameOverUI gameOverUI;
    private TurnIndicator turnIndicator;

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

        StartCoroutine(WaitToStart());
    }

    IEnumerator WaitToStart()
    {
        yield return new WaitForSeconds(0.1f);

        BeginFirstTurn();
    }

    void Update() {
        
        // TEMP TESTING � remove before shipping
        if ((Keyboard.current.digit3Key.wasPressedThisFrame))
        {
            Debug.Log("W pressed, gameOverUI = " + gameOverUI); 
            TriggerGameOver(true);  // test win screen
        }
            
        if ((Keyboard.current.digit4Key.wasPressedThisFrame))
        {
            Debug.Log("L pressed, gameOverUI = " + gameOverUI);
            TriggerGameOver(false);
        }

        // Add this with your other test keys
        if (Keyboard.current.digit5Key.wasPressedThisFrame)
        {
            HumanPlayer human = null;
            foreach (BaseCharacter p in GetPlayers())
            {
                if (p is HumanPlayer h) { human = h; break; }
            }
            if (human != null)
            {
                // Clear hand properly through CardHand so both lists are emptied
                List<Card> cards = new List<Card>(human.GetHandCards());
                foreach (Card c in cards)
                {
                    human.RemoveCardFromOwned(c); // removes from ownedCards AND CardHand
                }
                Debug.Log("Human cards cleared � hand: " + human.GetHandCards().Count + " owned: " + human.GetOwnedCardsCount());
                CheckGameOver();
            }
        }
        if (Keyboard.current.digit6Key.wasPressedThisFrame)
        {
            suitUI.ShowAnnouncement(CardSuit.Hearts, 10f);
        }
       

    }

    private void BeginFirstTurn()
    {
        Debug.Log("Beginning pre-round actions...");
       // start off at player 0's turn
        currentTurnIdx = 0;
        List<BaseCharacter> players = SetupPlayers();

        if (players != null && players.Count > 0)
        {
            // Enable XR interactions for draw and discard decks
            deck.EnableActivateDraw();
            discardPile.EnableAcceptSocket();

            currentPlayerTurn = players[currentTurnIdx];

            // Spawn turn indicator and put it above currentPlayerTurn's head
            this.turnIndicator = Instantiate(GlobalData.Instance.turnIndicatorPrefab, currentPlayerTurn.GetTransform()).GetComponent<TurnIndicator>();
            turnIndicator.SetParent(currentPlayerTurn);
            beginPlayerTurn.Invoke(currentPlayerTurn);
        }        
    }

    private List<BaseCharacter> SetupPlayers()
    {
        int playerId = 0;
        Debug.Log("Performing card setup...");
        List<BaseCharacter> players = GetPlayers();
        foreach (BaseCharacter player in players)
        {
            player.playerId = playerId;
            playerId++;
            player.AssignListeners();
        }

        // Deal each player five cards
        DealCards(players);

        return players;
    }

    private void DealCards(List<BaseCharacter> players)
    {
        Debug.Log("Drawing cards for each player...");

        for (int i = 0; i < 5; i++)
        {
            foreach(BaseCharacter player in players)
            {
                Card topCard = deck.Pop();
                topCard.SetOwner(player);
                player.TeleportNewCardToHand(topCard);
            }     
        }

        Debug.Log("Cards dealt. # of remaining cards in draw deck: " + deck.GetCardCount());
    }

    // Getter function to see all players
    public List<BaseCharacter> GetPlayers()
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

    public void EndTurn(BaseCharacter player, Card cardPlayed)
    {

    }

    public void PlayerPlayedCard(BaseCharacter player, Card cardPlayed)
    {
        if (player == currentPlayerTurn)
        {
            if (cardPlayed != null)
            {
                if (cardPlayed.suit != CardSuit.None)
                    currSuit = cardPlayed.suit;
                currRank = cardPlayed.rank;

                // Check win immediately after card is played
                CheckGameOver();      // ? ADD
                if (!enabled) return; // ? ADD � stop if game over

                HandleCardEffects(player, cardPlayed);
            }
        }
        else
        {
            throw new System.Exception("PlayerCardPlayed was invoked by a player while it was not their turn");
        }
    }

    public void PlayerTurnEnded(BaseCharacter player)
    {
        if (player == currentPlayerTurn)
        {
            Debug.Log("Player " + player.playerId + " turn end event received. Cards: " + player.GetOwnedCardsCount());
            Debug.Log("Draw deck: " + deck.GetCardCount() + ", discard pile: " + discardPile.GetCardCount());

            CheckGameOver(); // ? ADD before advancing turn

            if (!enabled) return; // game over was triggered, stop here

            AdvanceTurn();
        }
        else
        {
            throw new System.Exception("PlayerTurnEnded was called by a player while it was not their turn");
        }
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
        if (deck.PeekTop() == null || deck.GetCardCount() <= 1)
        {
            ReshuffleDiscardIntoDeck();
        }
        return deck.Pop();
    }

    private void ReshuffleDiscardIntoDeck()
    {
        Card topCard = discardPile.PeekTop(); // keep the current top card in place

        List<Card> discardCards = discardPile.PopAllCards();

        foreach (Card card in discardCards)
        {
            if (card == topCard) continue; // leave the top discard card alone
            deck.AddCard(card);
        }

        deck.ShuffleDeck();
        Debug.Log("Deck reshuffled from discard pile. New deck size: " + deck.GetCardCount());
    }

    public BaseCharacter GetCurrentPlayer()
    {
        return currentPlayerTurn;
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

    public CardSuit GetCurrSuit() => currSuit;
    public CardRank GetCurrRank() => currRank;

    public bool IsPlayerTurn(BaseCharacter player) => currentPlayerTurn == player;

    public bool CanPlayCard(Card candidateCard)
    {
        Card topCard = discardPile.PeekTop();
        if (topCard == null) return true;

        // Eight is always playable on anything
        if (candidateCard.rank == CardRank.Eight) return true;

        // Swap is always playable
        if (candidateCard.rank == CardRank.Swap || topCard.rank == CardRank.Swap) return true;

        // If top card is an 8, match against chosen suit
        if (topCard.rank == CardRank.Eight)
            return candidateCard.suit == currSuit;

        return candidateCard.rank == topCard.rank || candidateCard.suit == currSuit;
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
                AudioManager.Instance.Play(SoundName.Skip);
                Debug.Log("Next player skipped!");
                break;

            case CardRank.Reverse:
                ReverseTurnOrder();
                Debug.Log("Turn order reversed!");
                break;

            case CardRank.PlusOne:
                
                    int count = GetPlayers().Count;

                    // Determine direction-aware next player
                    int nextIndex = !reversed
                        ? (currentTurnIdx + 1) % count
                        : (currentTurnIdx - 1 + count) % count;

                    BaseCharacter nextPlayer = GetPlayers()[nextIndex];

                    // Give them a card
                    nextPlayer.TeleportNewCardToHand(deck.Pop());

                    Debug.Log("Next player draws +1 (direction-aware)");
                    break;
                

            case CardRank.Swap:
                waitingForEffect = true;
                RequestSwapChoice(player);
                Debug.Log("Hands swapped!");
                return;
        }

        cardPlayResolved.Invoke(player);
    }

    public void RequestSuitChoice(BaseCharacter player) {
        requestSuit.Invoke(player, suitUI);
    }

    public void RequestSwapChoice(BaseCharacter player)
    {
        requestSwap.Invoke(player, swapUI, GetPlayers());
    }

    public void OnSuitChosen(BaseCharacter player, CardSuit chosenSuit)
    {
        if (player == currentPlayerTurn)
        {
            currSuit = chosenSuit;
            Debug.Log("Suit chosen: " + chosenSuit);

            discardPile.UpdateTopCardSuitDisplay(chosenSuit); // ? ADD
            suitUI.ShowAnnouncement(chosenSuit, 3f);

            ContinueTurnAfterEffect();
        }
    }

    public void OnSwapChosen(BaseCharacter player, BaseCharacter target)
    {
        if (player == currentPlayerTurn)
        {
            currentPlayerTurn.SwapCardsWithPlayer(target);
            Debug.Log("Swapped hands with: " + target.name);

            // Chould make sure that when sawp played, discard pile updates to show card on top
            discardPile.UpdateCardBlob();

            ContinueTurnAfterEffect();
        }
    }

    private void ReverseTurnOrder() => reversed = !reversed;

    private void ContinueTurnAfterEffect()
    {
        waitingForEffect = false;

        CheckGameOver(); // ? ADD here too for card effects that empty a hand
        if (!enabled) return;

        AdvanceTurn();
    }

    void CheckGameOver()
    {
        List<BaseCharacter> players = GetPlayers();
        HumanPlayer human = null;

        foreach (BaseCharacter p in players)
        {
            if (p is HumanPlayer h) { human = h; break; }
        }

        if (human == null) return;

        // Lose � any AI has 0 cards
        foreach (BaseCharacter p in players)
        {
            if (p is AICharacter && p.GetOwnedCardsCount() == 0)
            {
                Debug.Log("AI " + p.playerId + " won � human loses!");
                TriggerGameOver(false);
                return;
            }
        }

        // Win � check BOTH owned cards AND hand cards are 0
        bool handEmpty = human.GetHandCards().Count == 0;
        bool ownedEmpty = human.GetOwnedCardsCount() == 0;

        Debug.Log("Win check � hand: " + human.GetHandCards().Count + " owned: " + human.GetOwnedCardsCount());

        if (handEmpty && ownedEmpty)
        {
            Debug.Log("Human player wins!");
            TriggerGameOver(true);
        }
    }

    private void TriggerGameOver(bool playerWon)
    {
        enabled = false;
        if (gameOverUI != null)
            gameOverUI.Show(playerWon);
    }

    public bool CurrentPlayerIs(BaseCharacter player) => currentPlayerTurn == player;

    private void AdvanceTurn()
    {
        // Don't advance if we're waiting for a player choice
        if (waitingForEffect) return; // ? ADD THIS

        int count = GetPlayers().Count;

        if (!reversed)
            currentTurnIdx = (currentTurnIdx + 1) % count;
        else
            currentTurnIdx = (currentTurnIdx - 1 + count) % count;

        currentPlayerTurn = GetPlayers()[currentTurnIdx];
        // move turn indicator
        turnIndicator.SetParent(currentPlayerTurn);
        beginPlayerTurn.Invoke(currentPlayerTurn);
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
        Card cardInHand = humanPlayer.GetHandCards().Find(c => c.GetInstanceID() == clickedID);

        if (cardInHand == null)
        {
            Debug.Log($"Card {card.rank} of {card.suit} not found in hand. Hand has {humanPlayer.GetOwnedCardsCount()} cards.");
            foreach (var c in humanPlayer.GetHandCards())
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
        humanPlayer.OnCardSelected(cardInHand);
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