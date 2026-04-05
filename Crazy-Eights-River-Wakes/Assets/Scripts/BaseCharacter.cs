using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCharacter : MonoBehaviour
{
    protected Animator animator;
    protected CardDeck cardDeck;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        //SpawnCardDeck();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
    public abstract void BeginCardTurn();

    public void EndTurn(Card cardPlayed) {
        CardGameManager.instance.EndTurn(this, cardPlayed);
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



    // Spawns the card deck at a given location. For AI characters this is overridden to use the deckAttach object as parent 
    protected virtual void SpawnCardDeck()
    {
        GameObject cardDeckGameObj = new GameObject("CardDeck");
        this.cardDeck = cardDeckGameObj.AddComponent<CardDeck>();
        /*the 2 lines below are from AICharacter. They don't work here since we don't have deckAttach
        I left them for reference as a guide
        */
        
        // cardDeck.transform.SetParent(this.deckAttach.transform);
        // cardDeck.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    // Needed just so that user can fan out hand
    protected virtual void FanOutHand()
    {
        // Default implementation does nothing.
        // HumanPlayer will override this to visually fan out cards.
    }
}
