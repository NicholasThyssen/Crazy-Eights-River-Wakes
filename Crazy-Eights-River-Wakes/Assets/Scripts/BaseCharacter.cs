using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class BaseCharacter : MonoBehaviour
{
    public GameObject cardHandPrefab;
    protected Animator animator;

    // The list of the player's OWNED cards (i.e. those in their hand + any loose cards).
    protected List<Card> ownedCards;

    // The physical representation of a player's hand.
    protected CardHand playerHand;

    protected bool playedThisTurn = false;

    public int playerId = -1;

    public UnityEvent<BaseCharacter, Card> playerPlayedCard;

    public UnityEvent<BaseCharacter> playerTurnEnded;

    protected List<Card> queue;


    void Awake()
    {
        animator = GetComponent<Animator>();
        Initialize();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize()
    {
        ownedCards = new List<Card>();
        CreateHand();
    }

    public void CreateHand()
    {
        GameObject playerHandObject = Instantiate(cardHandPrefab);
        playerHand = playerHandObject.GetComponent<CardHand>();
        playerHand.InitializeHand();
        playerHand.SetOwner(this);
        playerHandObject.transform.SetParent(this.transform);
        playerHandObject.SetActive(true);
        //playerHand.gameObject.transform.SetParent(this.deckAttach.transform);
        // playerHand.gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        
    }

    public void AssignListeners()
    {
        CardGameManager cgm = CardGameManager.instance;
        // Assign listeners to our own signal
        playerPlayedCard.AddListener(cgm.PlayerPlayedCard);
        playerTurnEnded.AddListener(cgm.PlayerTurnEnded);
        // Listen to the manager's signals
        cgm.beginPlayerTurn.AddListener(BeginPlayerTurn);
        cgm.cardPlayResolved.AddListener(FinishPlayerTurn);
    }

    public void AddCard(Card card)
    {
        if (card == null) return;

        hand.Add(card);
        card.transform.SetParent(this.transform); // or a hand anchor
                                                  // optionally position it here
        FanOutHand();
    }

    // This should handle what happens when CardManager notifies this player that it is their turn

    public abstract void BeginPlayerTurn(BaseCharacter player);

    public abstract void FinishPlayerTurn(BaseCharacter player);

    public void EndTurn() {
        playedThisTurn = false;
        playerTurnEnded.Invoke(this);
    }

    public List<Card> GetOwnedCards()
    {
        return ownedCards;
    }

    public int GetOwnedCardsCount()
    {
        return ownedCards.Count;
    }

    public bool HasCard(Card targetCard)
    {
        return ownedCards.Contains(targetCard);
    }

    public void SetOwnedCards(List<Card> newOwnedCards)
    {
        ownedCards.Clear();
        //playerHand.Clear();
        //playerHand.ClearHeldCards();
        ownedCards = newOwnedCards;
        foreach (Card c in ownedCards)
        {
            TeleportNewCardToHand(c, false);
        }
    }

    public void AddCardToOwned(Card targetCard)
    {
        targetCard.SetOwner(this);
        ownedCards.Add(targetCard);   
    }

    public void RemoveCardFromOwned(Card targetCard)
    {
        ownedCards.Remove(targetCard);
        if (playerHand.HasCardInHand(targetCard))
        {
            playerHand.RemoveCardFromHand(targetCard);
        }
    }

    public void TeleportNewCardToHand(Card targetCard, bool flying = false)
    {
        AddCardToOwned(targetCard);
        playerHand.AddCardFromTeleport(targetCard);
        // playerHand.SummonCardToHand(targetCard);
    }

    public void PullCardToHandObject(Card targetCard, Transform handObject, bool flying = false)
    {
        if (playerHand.HasCardInHand(targetCard))
        {
            playerHand.RemoveCardFromHand(targetCard);
            if (flying)
            {
            
            }
            else
            {
                targetCard.gameObject.transform.SetParent(handObject);
                targetCard.gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
        }

    }

    public void PlayCardToDeck(Card targetCard, CardDeck targetDeck, bool flying = false)
    {
        RemoveCardFromOwned(targetCard);
        if (flying)
        {
            
        }
        else
        {
            targetDeck.PlayCardToDeck(targetCard);
        }
    }


    // List of player's hand
    public List<Card> hand = new List<Card>();

    // We remove card from hand
    public void RemoveCard(Card card)
    {
        hand.Remove(card);
        FanOutHand(); // visual for card fan
    }

    // We get their hand
    public List<Card> GetHand()
    {
        return hand;

    }

    // Set hand to something new
    public void SetHand(List<Card> newHand)
    {
        hand = newHand;
        FanOutHand();
    }

    // Lets player see UI to choose suit to change (after playing an 8)
    public void ShowSuitSelectionUI()
    {
        CardGameManager.instance.suitUI.Show(this);
    }

    // Lets player see UI to choose player to swap (after playing a swap)
    public void ShowSwapSelectionUI(List<BaseCharacter> players)
    {
        CardGameManager.instance.swapUI.Show(this, players);
    }

    private void RefreshHandUI()
    {
        // TODO: redraw the cards visually
        // This depends on your existing UI system
    }

    // Needed just so that user can fan out hand
    protected virtual void FanOutHand()
    {
        // Default implementation does nothing.
        // HumanPlayer will override this to visually fan out cards.
    }
}
