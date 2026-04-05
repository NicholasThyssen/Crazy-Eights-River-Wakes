using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CardDeck : MonoBehaviour
{
    private List<Card> cards;

    private Transform respawnAnchor;
    private Transform cardBlob;

    private Transform cardContainer;

    private XRSocketInteractor acceptSocket;

    public bool faceDownDeck = false;
    public bool spawnCardsOnAwake = false;
    public GameObject cardPrefab;

    public UnityEvent<string> deckShuffled;
    public UnityEvent<bool> deckRespawned;

    public UnityEvent<BaseCharacter, Card> cardPlayedToDeck;

    public void Awake()
    {
        // Initialize card list
        cards = new List<Card>(GetComponentsInChildren<Card>());
        if (transform.childCount > 0)
        {
            cardBlob = transform.GetChild(0);
            acceptSocket = transform.GetChild(1).GetComponent<XRSocketInteractor>();
            cardContainer = transform.GetChild(2);
        }
        // Spawn cards if necessary
        if (spawnCardsOnAwake)
        {
            SpawnCards();
        }
    }

    public void EnableAcceptSocket()
    {
        acceptSocket.gameObject.SetActive(true);
    }

    public void SetRespawnAnchor(Transform respawnAnchor)
    {
        this.respawnAnchor = respawnAnchor;
    }

    // When the deck need to respawn at its anchor (typically because it got tossed around), call this function and tell the anchor to play a particle effect.
    public void RespawnAtAnchor()
    {
        transform.position = respawnAnchor.position;
        transform.rotation = respawnAnchor.rotation;
    }

    // Shouldn't be needed; only used for testing purposes
    public void SetCards(List<Card> cardsList)
    {
        this.cards = cardsList;
        foreach(var card in cards)
        {
            card.gameObject.transform.SetParent(cardContainer);
        }
    }

    // Returns the card at the top of the deck, but does not remove it from the deck
    public Card PeekTop()
    {
        return cards.Count > 0 ? cards[cards.Count-1] : null;
    }

    public List<Card> GetCards()
    {
        return this.cards;
    }

    public int GetCardCount()
    {
        return cards.Count;
    }

    // Returns the card at the top of the deck AND removes it from the deck
    public Card Pop()
    {
        Card cardAtTop = this.PeekTop();
        if (cardAtTop != null)
        {
            RemoveCard(cardAtTop);
        }
        cardAtTop.gameObject.SetActive(true);
        cardAtTop.EnableGrab();
        return cardAtTop;
    }

    // Returns (and removes) a random card. Maybe useful for +1?
    public Card DrawRandomCard()
    {
        if (cards == null || cards.Count == 0)
        {
            return null;
        }

        int randomIdx = Random.Range(0, cards.Count);
        Card drawnCard = this.cards[randomIdx];

        // remove from deck
        RemoveCard(drawnCard);
        drawnCard.transform.SetParent(null);

        return drawnCard;
    }

    public void ShuffleDeck()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            int randomIndex = Random.Range(i, cards.Count);

            // swap
            Card temp = cards[i];
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;

            // swap transforms
            Vector3 position1 = cards[i].transform.localPosition;
            Vector3 position2 = cards[randomIndex].transform.localPosition;
            Quaternion rotation1 = cards[i].transform.localRotation;
            Quaternion rotation2 = cards[randomIndex].transform.localRotation;

            cards[i].transform.localPosition = position2;
            cards[i].transform.localRotation = rotation2;

            cards[randomIndex].transform.localPosition = position1;
            cards[randomIndex].transform.localRotation = rotation1;
        }
    }

    // Spawn a full set of cards (animate this later?)
    public void SpawnCards()
    {
        if (cards == null)
        {
            cards = new List<Card>();
        }
        foreach (CardSuit suit in System.Enum.GetValues(typeof(CardSuit)))
        {
            foreach (CardRank rank in System.Enum.GetValues(typeof(CardRank)))
            {
                if (suit == CardSuit.None)
                {
                    if (!(rank == CardRank.Swap || rank == CardRank.Eight))
                    {
                        continue;
                    }
                } else if (rank == CardRank.Swap || rank == CardRank.Eight)
                {
                    continue;
                }

                GameObject newCardObj = Instantiate(cardPrefab);
                TestCard newCard = newCardObj.GetComponent<TestCard>();
                newCard.Initialize(suit, rank);
                AddCard(newCard);
            }
        }

        ShuffleDeck();
    }

    public void UpdateCardBlob()
    {
        Vector3 localForward = transform.worldToLocalMatrix.MultiplyVector(transform.forward);
        Vector3 topOffset = 0.001f * cards.Count * localForward;
        Card topCard = PeekTop();

        if (topCard == null)
        {
            return;
        }

        // If <3 cards, hide completely
        if (cards.Count < 3)
        {
            if (cardBlob != null)
            {
                cardBlob.gameObject.SetActive(false);
            }

            if (cards.Count == 2) {
                cards[0].gameObject.SetActive(false);
            }
            topCard.gameObject.SetActive(true);
            topCard.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            topCard.gameObject.transform.localPosition += topOffset;
        }
        // Otherwise, show card blob expanding/shrinking
        else
        {
            cards[cards.Count - 2].gameObject.SetActive(false);
            topCard.gameObject.SetActive(true);
            topCard.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            topCard.gameObject.transform.localPosition += topOffset;

            if (cardBlob != null)
            {
                Vector3 blobOffset = 0.001f * cards.Count * localForward;
                cardBlob.gameObject.SetActive(true);
                cardBlob.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                cardBlob.localPosition = new Vector3(blobOffset.x / 2.0f, blobOffset.y / 2.0f, blobOffset.z / 2.0f); // Why the hell does Unity not natively support dividing vectors by scalars (another point in Godot's favor)
                cardBlob.localScale = new Vector3(1.0f, 1.0f, 1.0f * cards.Count - 2);
            }
        }
    }

    // add a card to the deck. todo: add animation?
    public void AddCard(Card card)
    {
        cards.Add(card);
        card.transform.SetParent(cardContainer);
        card.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        //Debug.Log("AddCard called. card={(card == null ? "NULL" : card.name)}, cardsList={(cards == null ? "NULL" : "OK")}", this);

        if (faceDownDeck)
        {
            card.transform.SetLocalPositionAndRotation(new Vector3(180, 0, 0), Quaternion.identity);
        }

        // move card to be physically in front of the others (so they aren't all at the same location)
        Vector3 localForward = transform.worldToLocalMatrix.MultiplyVector(transform.forward);
        Vector3 offset = 0.001f * cards.Count * localForward;
        card.gameObject.transform.localPosition += offset;

        card.DisablePhysics();
        //card.DisableGrab();
        
        UpdateCardBlob();
        Debug.Log("Currently have # of cards:" + cards.Count);
    }

    public void PlayCardFromSocket()
    {   
        var selected = acceptSocket.GetOldestInteractableSelected();
        if (selected != null)
        {
            acceptSocket.interactionManager.SelectExit(acceptSocket, selected);
            Card targetCard = selected.transform.gameObject.GetComponent<Card>();
            AddCard(targetCard);
            cardPlayedToDeck.Invoke(targetCard.owner, targetCard);
        }
    }

    public void PlayCardToDeck(Card card)
    {
        AddCard(card);
        cardPlayedToDeck.Invoke(card.owner, card);
    }

    public void DrawCardFromActivate()
    {
        // TODO: Draw a card to the player's hand by activating the deck
        // Assign the player as the owner + add to their owned cards
        
        // deck.Pop();
    }

    public void RemoveCard(Card card)
    {
        this.cards.Remove(card);
        UpdateCardBlob();
    }

    public void CardAcceptedBySocket()
    {
        // Get the card and add it to the deck
        // Send an event afterward that the game manager picks up on
        Card newCard = new Card();
        acceptSocket.socketActive = false;
        acceptSocket.socketActive = true;
    }

    public void DrawCardToPlayer(BaseCharacter target, bool autoAdd = false)
    {
        Card nextCard = Pop();
        if (nextCard == null)
        {
            return;
        }

        if (autoAdd)
        {
            target.TeleportNewCardToHand(nextCard);
        }
        else
        {
            // Animate card flying to target's hand
        }
    }

    public bool EvaluateSelection(Card candidateCard)
    {
        Card topCard = PeekTop();
        if (topCard == null) return true;
        return topCard.IsValidMatch(candidateCard);
    }

    public void MergeDeckAndShuffle(CardDeck deck)
    {
        
    }


}