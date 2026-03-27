using TMPro;
using UnityEngine;

public class TestCard : Card
{
    private TextMeshPro text;
    private Renderer rend;
    private Material faceMaterial;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        text = GetComponentInChildren<TextMeshPro>();
        faceMaterial = transform.GetChild(0).GetComponent<MeshRenderer>().material; // "Face" should always be the first child in the GameObject hierarchy
            }

    public void Initialize(CardSuit suit, CardRank rank)
    {
        this.suit = suit;
        this.rank = rank;
        
        if (faceMaterial != null)
        {
            SetFaceTexture(suit, rank);
        }

        // Set text
        if (text != null)
        {
            text.text = GetTextFromRank(rank);
        }

        // Set cube color
        if (rend != null)
        {
            rend.material.color = GetColorFromSuit(suit);
        }
    }

    // Sets the face texture of the card.
    private void SetFaceTexture(CardSuit suit, CardRank rank)
    {
        if (rank != CardRank.Eight && rank != CardRank.Swap) {
            faceMaterial.SetFloat("Suit", (int)suit);
        }
        else if (rank == CardRank.Eight)
        {
            // Set the card to a blue 8 for now
            faceMaterial.SetFloat("Suit", 0);
            faceMaterial.SetFloat("Rank", 7);
        }
        else if (rank == CardRank.Swap)
        {
            // Set the card to a green 8 for now
            faceMaterial.SetFloat("Suit", 1);
            faceMaterial.SetFloat("Rank", 7);
        }
        faceMaterial.SetFloat("Rank", (int)rank);
    }

    // Returns a non-zero value if the card can be played to the current pile.
    // I'd make this a boolean, but I don't know how wilds and swaps are planned to be handled.
    private int IsValidPlayable(CardSuit suit, CardRank rank)
    {
        if (this.rank == CardRank.Eight)
        {
            // Eights are wild
            return 2;
        }
        else if (this.rank == CardRank.Swap)
        {
            // Swap cards also don't care about suit
            return 3;
        }
        else if (this.rank == rank || this.suit == suit)
        {
            // Otherwise, return a non-zero value if ranks or suits match
            return 1;
        }
        return 0;
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