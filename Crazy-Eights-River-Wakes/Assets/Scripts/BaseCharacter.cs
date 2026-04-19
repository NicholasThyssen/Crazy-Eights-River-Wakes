using System;
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

    public UnityEvent<BaseCharacter, CardSuit> suitSelected;

    public UnityEvent<BaseCharacter, BaseCharacter> swapSelected;

    protected List<Card> queue;


    void Awake()
    {
        animator = GetComponent<Animator>();
        Initialize();
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
    }

    public List<Card> GetHandCards() => playerHand.GetHeldCards();
    public CardHand GetHandObject() => playerHand;
    public void AssignListeners()
    {
        CardGameManager cgm = CardGameManager.instance;
        // Assign listeners to our own signal
        playerPlayedCard.AddListener(cgm.PlayerPlayedCard);
        playerTurnEnded.AddListener(cgm.PlayerTurnEnded);
        suitSelected.AddListener(cgm.OnSuitChosen);
        swapSelected.AddListener(cgm.OnSwapChosen);
        // Listen to the manager's signals
        cgm.beginPlayerTurn.AddListener(BeginPlayerTurn);
        cgm.cardPlayResolved.AddListener(FinishPlayerTurn);
        cgm.requestSuit.AddListener(HandleSuitRequest);
        cgm.requestSwap.AddListener(HandleSwapRequest);
    }

    // This should handle what happens when CardManager notifies this player that it is their turn

    public abstract void BeginPlayerTurn(BaseCharacter player);

    public abstract void FinishPlayerTurn(BaseCharacter player);

    public void EndTurn() {
        playedThisTurn = false;
        playerTurnEnded.Invoke(this);
    }

    public List<Card> GetOwnedCards() => ownedCards;

    public int GetOwnedCardsCount() => ownedCards.Count;

    public bool HasCard(Card targetCard) => ownedCards.Contains(targetCard);

    public void SetOwnedCards(List<Card> newOwnedCards)
    {
        ownedCards.Clear();
        // Make a copy first to avoid modifying the list we're iterating
        List<Card> copy = new List<Card>(newOwnedCards);
        ownedCards = new List<Card>();
        foreach (Card c in copy)
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
        targetCard.StopPhysicsMode(); // ? freeze physics BEFORE hand placement
        targetCard.DisableGrab();     // ? prevent XR fighting the placement
        AddCardToOwned(targetCard);
        playerHand.AddCardFromTeleport(targetCard);
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
        AudioManager.Instance.Play(SoundName.PlaceCard, targetDeck.gameObject);
        RemoveCardFromOwned(targetCard);
        if (flying)
        {
            
        }
        else
        {
            targetDeck.PlayCardToDeck(targetCard);
        }
    }

    public void SwapCardsWithPlayer(BaseCharacter other)
    {
        CardHand otherHand = other.GetHandObject();
        List<Card> myCards = new List<Card>(playerHand.PopAllCards());
        List<Card> otherCards = new List<Card>(otherHand.PopAllCards());

        // Fixes swap cards to show top of deck
        foreach (Card c in myCards) c.gameObject.SetActive(true);
        foreach (Card c in otherCards) c.gameObject.SetActive(true);

        SetOwnedCards(otherCards);
        other.SetOwnedCards(myCards);
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

    public void WarpCardToHand(Card targetCard)
    {
        targetCard.StopPhysicsMode(); // ? ADD THIS
        targetCard.DisableGrab();     // ? ADD THIS
        if (!HasCard(targetCard))
        {
            AddCardToOwned(targetCard);
        }
        playerHand.SummonCardToHand(targetCard);
    }

    // Needed just so that user can fan out hand
    protected virtual void FanOutHand()
    {
        // Default implementation does nothing.
        // HumanPlayer will override this to visually fan out cards.
    }

    public virtual void TryPlayCard(Card card)
    {
        // Default behavior for AI or characters without custom logic
        Debug.Log(name + " TryPlayCard called, but no override implemented.");
    }

    protected virtual void HandleSuitRequest(BaseCharacter player, SuitSelectionUI suitUI)
    {
        
    }


    protected virtual void HandleSwapRequest(BaseCharacter player, SwapSelectionUI suitUI, List<BaseCharacter> players)
    {
        
    }

    public abstract bool CardShouldFan();

    // Transform is the CAMERA position for VR player
    public abstract Transform GetTransform();

    public virtual float GetCameraHeight()
    {
        return 0f;
    }

}
