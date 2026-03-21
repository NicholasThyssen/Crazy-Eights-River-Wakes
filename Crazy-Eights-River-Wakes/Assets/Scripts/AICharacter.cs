using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AICharacter : BaseCharacter
{
    private List<Card> cards; 
    public override void BeginCardTurn()
    {
        StartCoroutine(HandleCardTurn());
    }

    IEnumerator HandleCardTurn()
    {
        yield return new WaitForSeconds(5f);

        // Attempt to draw a random card
        DrawRandomCard();

        // End the turn
        EndTurn();
    }

    void DrawRandomCard()
    {
        throw new System.NotImplementedException();
    }

    void Start()
    {

    }

    void Update()
    {

    }
}