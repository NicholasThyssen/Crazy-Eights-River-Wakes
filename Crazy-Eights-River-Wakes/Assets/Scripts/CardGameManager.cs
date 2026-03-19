using System.Collections.Generic;
using UnityEngine;

public class CardGameManager : MonoBehaviour
{
    private int currentTurnIdx;
    private BaseCharacter currentPlayerTurn;


    void Start()
    {
        // initialze beginning suit, rank

        // start off at player 0's turn
        currentTurnIdx = 0;
        List<BaseCharacter> players = GetPlayers();

        if (players != null && players.Count > 0)
        {
            currentPlayerTurn = players[currentTurnIdx];
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void HandleCardDrawn(BaseCharacter playerDrawing, Suit cardSuit, Rank cardRank)
    {

    }

    private List<BaseCharacter> GetPlayers()
    {
        return GameManager.instance.characters;
    }

    // Call this from BaseCharacter when done drawing
    public void EndTurn(BaseCharacter player)
    {
        if (player != currentPlayerTurn)
        {
            throw new System.Exception("EndTurn was called by a player while it was not their turn");
        }

        // move onto next player
        currentTurnIdx = (currentTurnIdx + 1) % GetPlayers().Count;
        currentPlayerTurn = GetPlayers()[currentTurnIdx];

        // notify player that it is their turn
        currentPlayerTurn.BeginCardTurn();
    }
}

public enum Suit
{
    Clubs,
    Diamonds,
    Hearts,
    Spades
}

public enum Rank
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