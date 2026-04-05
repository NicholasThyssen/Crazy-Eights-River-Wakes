using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class AICharacter : BaseCharacter
{
    public GameObject deckAttach;
    public GameObject cardAttach;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        Initialize();
    }

    public override void BeginPlayerTurn(BaseCharacter player)
    {
        if (player == this)
        {
            StartCoroutine(HandlePlayerTurn());
        }
        
    }

    IEnumerator HandlePlayerTurn()
    {
        yield return new WaitForSeconds(1.5f);

        List<Card> playableCards = GetPlayableCards();
        Card selectedCard = null;

        if (playableCards.Count > 0)
        {
            selectedCard = ChooseRandomCard(playableCards);
        }
        else
        {
            AIPlayerDrawCard();
            yield return new WaitForSeconds(1f);
            playableCards = GetPlayableCards();
            if (playableCards.Count > 0)
            {
                selectedCard = ChooseRandomCard(playableCards);
            }
        }

        if (selectedCard != null)
        {
            PullCardToHandObject(selectedCard, cardAttach.transform);
            yield return new WaitForSeconds(1.0f);
            AIPlayerPlayCard(selectedCard);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            EndTurn();
        }  
    }

    public override void FinishPlayerTurn(BaseCharacter player)
    {
        if (player == this)
        {
            EndTurn();
        }
    }

    public List<Card> GetPlayableCards()
    {
        return playerHand.GetHeldCards().Where(x => CardGameManager.instance.CanPlayCard(x)).ToList();
    }

    public void CreateHand()
    {
        GameObject playerHandObject = Instantiate(cardHandPrefab);
        playerHand.InitializeHand();
        playerHand = playerHandObject.GetComponent<CardHand>();
        playerHand.SetOwner(this);
        playerHand.DisableSocketInteractions();
        playerHandObject.transform.SetParent(deckAttach.transform);
        playerHandObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        playerHandObject.SetActive(true);
    }


    private void AIPlayerDrawCard()
    {
        Card drawnCard = CardGameManager.instance.deck.Pop();
        TeleportNewCardToHand(drawnCard);
    }

    private void AIPlayerPlayCard(Card selectedCard)
    {
        PlayCardToDeck(selectedCard, CardGameManager.instance.discardPile);
        playerPlayedCard.Invoke(this, selectedCard);
    }

    // Chooses a card that is valid 
    private Card ChooseRandomCard(List<Card> validCards)
    {
        if (validCards == null || validCards.Count < 1)
            throw new ArgumentException(nameof(validCards), "validCards cannot be null or empty");

        int randomIdx = UnityEngine.Random.Range(0, validCards.Count);
        Card drawnCard = validCards[randomIdx];
        return drawnCard;
    }

    void Start()
    {

    }

    void Update()
    {

    }
}