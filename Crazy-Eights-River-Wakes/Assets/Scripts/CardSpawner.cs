using System.Collections.Generic;
using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    public GameObject cardPrefab;
    public CardDeck deck;

    void Start()
    {
        foreach (CardSuit suit in System.Enum.GetValues(typeof(CardSuit)))
        {
            foreach (CardRank rank in System.Enum.GetValues(typeof(CardRank)))
            {
                if (suit == CardSuit.None)
                {
                    if (!(rank == CardRank.Swap || rank == CardRank.Eight))
                        continue;
                }
                else if (rank == CardRank.Swap || rank == CardRank.Eight)
                {
                    continue;
                }

                GameObject newCardObj = Instantiate(cardPrefab);
                TestCard newCard = newCardObj.GetComponent<TestCard>();
                newCard.Initialize(suit, rank);

                deck.AddCard(newCard);
            }
        }

        deck.ShuffleDeck();
    }
}
