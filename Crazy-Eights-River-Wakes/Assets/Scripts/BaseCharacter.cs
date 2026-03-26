using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCharacter : MonoBehaviour
{
    protected Animator animator;
    protected CardDeck cardDeck;
    

    private void Awake()
    {
        animator = GetComponent<Animator>();
        SpawnCardDeck();
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
        this.cardDeck.AddCard(card);
    }

    // This should handle what happens when CardManager notifies this player that it is their turn
    public abstract void BeginCardTurn();

    public void EndTurn(Card cardPlayed) {
        CardGameManager.instance.EndTurn(this, cardPlayed);
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
}
