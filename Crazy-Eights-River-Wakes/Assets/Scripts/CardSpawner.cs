using System.Collections.Generic;
using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    public GameObject cardPrefab;
    public CardDeck deck;

    void Start()
    {

        List<Card> cards = new List<Card>();
        

        foreach (CardSuit suit in System.Enum.GetValues(typeof(CardSuit)))
        {
            foreach (CardRank rank in System.Enum.GetValues(typeof(CardRank)))
            {
                // only allow Suite None for swap and 8 cards
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
                cards.Add(newCard);
            }
        }

        // move their locations a little bit
        Vector3 offset = Vector3.zero;
        foreach(Card card in cards)
        {
            card.gameObject.transform.SetParent(deck.gameObject.transform);
            card.gameObject.transform.localPosition = offset;
            offset += Vector3.forward * 0.001f;
        }

        deck.SetCards(cards);
        deck.ShuffleDeck();
    }
}
