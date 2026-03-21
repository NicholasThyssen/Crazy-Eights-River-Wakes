using TMPro;
using UnityEngine;

public class TestCard : Card
{
    private TextMeshPro text;
    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        text = GetComponentInChildren<TextMeshPro>();
    }
    public void Initialize(CardSuit suite, CardRank rank)
    {
        // Set text
        if (text != null)
        {
            text.text = GetTextFromRank(rank);
        }

        // Set cube color
        if (rend != null)
        {
            rend.material.color = GetColorFromSuit(suite);
        }
    }

    private static Color GetColorFromSuit(CardSuit suit)
{
    switch (suit)
    {
        case CardSuit.Hearts:
            return Color.red;

        case CardSuit.Diamonds:
            return Color.yellow;

        case CardSuit.Clubs:
            return Color.cyan;

        case CardSuit.Spades:
            return Color.blue;

        default:
            return Color.white;
    }
}

    private static string GetTextFromRank(CardRank rank) => rank switch
    {
        CardRank.Ace => "A",
        CardRank.Two => "2",
        CardRank.Three => "3",
        CardRank.Four => "4",
        CardRank.Five => "5",
        CardRank.Six => "6",
        CardRank.Seven => "7",
        CardRank.Eight => "8",
        CardRank.Nine => "9",
        CardRank.Ten => "10",
        CardRank.Reverse => "R",
        CardRank.Swap => "Swap",
        CardRank.Skip => "Skip",
        CardRank.PlusOne => "+1",
        _ => "other",
    };
}