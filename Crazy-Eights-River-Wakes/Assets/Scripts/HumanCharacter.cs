using UnityEngine;

public class HumanPlayer : BaseCharacter
{
    public override void BeginCardTurn()
    {
        Debug.Log("Human player's turn started: " + name);

        // TEST
    }

    public override void BeginPlayerTurn(BaseCharacter player)
    {
        if (player == this)
        {
            Debug.Log("Player begin turn event received!");
        }
        
    }
}

