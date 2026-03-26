using UnityEngine;

public class Card : MonoBehaviour
{
    public CardSuit suit;
    public CardRank rank;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}


public enum CardSuit
{
    Clubs,
    Diamonds,
    Hearts,
    Spades,
    None // used for 8 and Swap
}

public enum CardRank
{
    Ace,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,      // special
    Nine,
    Ten,
    Reverse,    // special
    Swap,       // special
    Skip,       // special
    PlusOne     // special
}


