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

    public void Initialize()
    {
        ownedCards = new List<Card>();
        CreateHand();
    }

    public void CreateHand()
    {
        GameObject playerHandObject = Instantiate(cardHandPrefab);
        playerHand = playerHandObject.GetComponent<CardHand>();
        playerHand.SetOwner(this);
        playerHand.DisableSocketInteractions();
        playerHandObject.transform.SetParent(deckAttach.transform);
        playerHandObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        playerHandObject.SetActive(true);
    }

    public override void BeginPlayerTurn(BaseCharacter player)
    {
        if (player == this)
        {
            Debug.Log("AI player (" + playerId + ")'s begin turn event received!");
            StartCoroutine(HandlePlayerTurn());
        }
        
    }

    IEnumerator HandlePlayerTurn()
    {
        yield return new WaitForSeconds(1.5f);
        animator.SetTrigger("Look At Cards");
        yield return new WaitForSeconds(3f);

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
            animator.SetTrigger("Select Card");
            TeleportCardToHand(selectedCard, cardAttach.transform);
            yield return new WaitForSeconds(3.0f);
            AIPlayerPlayCard(selectedCard);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            animator.SetTrigger("End Look At Cards");
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

    public List<Card> GetPlayableCards() => playerHand.GetHeldCards().Where(x => CardGameManager.instance.CanPlayCard(x)).ToList();

    private void AIPlayerDrawCard()
    {
        Card drawnCard = CardGameManager.instance.deck.Pop();
        TeleportNewCardToHand(drawnCard);
    }

    private void AIPlayerPlayCard(Card selectedCard)
    {
        PlayCardToDeck(selectedCard, CardGameManager.instance.discardPile);
        playerPlayedCard.Invoke(this, selectedCard);
        animator.SetTrigger("End Look At Cards");
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

    protected override void HandleSuitRequest(BaseCharacter player, SuitSelectionUI suitUI)
    {
        if (player == this)
        {
           // Choose a random suit that the AI player has
            HashSet<CardSuit> possibleValues = new HashSet<CardSuit>();
            CardSuit targetSuit = CardSuit.Hearts;
            foreach (Card c in ownedCards)
            {
                if (c.suit != CardSuit.None) {
                    possibleValues.Add(c.suit);
                }
            }
            List<CardSuit> possibleList = possibleValues.ToList();
            if (possibleList.Count > 0) {
                int randomize = UnityEngine.Random.Range(0, possibleList.Count);
                targetSuit = possibleList[randomize];
            }
        
            suitSelected.Invoke(this, targetSuit);         
        }
    }

    protected override void HandleSwapRequest(BaseCharacter player, SwapSelectionUI swapUI, List<BaseCharacter> players)
    {
        if (player == this)
        {
            // Pick the player with the least amount of cards
            CardGameManager cgm = CardGameManager.instance;
            BaseCharacter target = players[0];
            int lowest = 999;
        
            foreach(BaseCharacter candidate in players)
            {
                if (candidate == this) continue;
                if (candidate.GetOwnedCardsCount() < lowest)
                {
                    lowest = candidate.GetOwnedCardsCount();
                    target = candidate;
                }
            }

            swapSelected.Invoke(this, target);
        }

    }

    public override bool CardShouldFan()
    {
        return false;   // don't fan cards for AI
    }

    private void TeleportCardToHand(Card targetCard, Transform handObject, bool flying = false)
    {
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