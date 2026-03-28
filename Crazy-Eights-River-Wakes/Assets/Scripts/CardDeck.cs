using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CardDeck : MonoBehaviour
{
    private List<Card> cards;

    private Transform respawnAnchor;
    private Transform cardBlob;

    public bool faceDownDeck = false;
    public bool spawnCardsOnAwake = false;
    public GameObject cardPrefab;

    public UnityEvent<string> deckShuffled;
    public UnityEvent<bool> deckRespawned;

    public void Awake()
    {
        if (transform.childCount > 0)
        {
            cardBlob = transform.GetChild(0);
            //Destroy(cardBlob.gameObject);
        }
        // Spawn cards if necessary
        if (spawnCardsOnAwake)
        {
            SpawnCards();
        }
        // Otherwise, initialize card list
        else {
        cards = new List<Card>(GetComponentsInChildren<Card>());
        }
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
            card.gameObject.transform.SetParent(this.transform);
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

    // Returns the card at the top of the deck AND removes it from the deck
    public Card Pop()
    {
        Card cardAtTop = this.PeekTop();
        if (cardAtTop != null)
        {
            RemoveCard(cardAtTop);
        }
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

        // ShuffleDeck();
    }

    public void UpdateCardBlob()
    {
        Vector3 localForward = transform.worldToLocalMatrix.MultiplyVector(transform.forward);
        Vector3 topOffset = 0.001f * cards.Count * localForward;
        // If <3 cards, hide completely
        if (cards.Count < 3)
        {
            if (cardBlob != null)
            {
                cardBlob.gameObject.SetActive(false);
            }

            Card topCard = PeekTop();
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
            Card topCard = PeekTop();
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
        card.transform.SetParent(this.transform);
        card.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        if (faceDownDeck)
        {
            card.transform.SetLocalPositionAndRotation(new Vector3(180, 0, 0), Quaternion.identity);
        }

        // move card to be physically in front of the others (so they aren't all at the same location)
        Vector3 localForward = transform.worldToLocalMatrix.MultiplyVector(transform.forward);
        Vector3 offset = 0.001f * cards.Count * localForward;
        card.gameObject.transform.localPosition += offset;
        
        UpdateCardBlob();
    }

    public void MergeDeckAndShuffle(CardDeck deck)
    {
        
    }

    public void RemoveCard(Card card)
    {
        this.cards.Remove(card);
        UpdateCardBlob();
    }
}