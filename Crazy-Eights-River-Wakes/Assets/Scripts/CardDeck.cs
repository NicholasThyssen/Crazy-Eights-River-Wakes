using System.Collections.Generic;
using UnityEngine;

public class CardDeck : MonoBehaviour
{
    private List<Card> cards;

    public void Awake()
    {
        cards = new List<Card>(GetComponentsInChildren<Card>());
    }

    // Shouldn't be needed; only used for testing purposes
    public void SetCards(List<Card> cardsList)
    {
        this.cards = cardsList;
        foreach(var card in cards)
        {
            card.gameObject.transform.SetParent(this.transform);
            // card.gameObject.transform.localPosition = Vector3.zero;
        }
    }

    // Returns the card at the top of the deck, but does not remove it from the deck
    public Card PeekTop()
    {
        return cards[cards.Count-1];
    }

    public Card DrawRandomCard()
    {
        if (cards == null || cards.Count == 0)
        {
            return null;
        }

        int randomIdx = Random.Range(0, cards.Count);
        Card drawnCard = this.cards[randomIdx];

        // remove from deck
        cards.RemoveAt(randomIdx);
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
}