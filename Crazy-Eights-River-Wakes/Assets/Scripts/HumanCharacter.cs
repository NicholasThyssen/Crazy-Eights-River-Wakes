using UnityEngine;

public class HumanPlayer : BaseCharacter
{

    public override void BeginPlayerTurn(BaseCharacter player)
    {
        if (player == this)
        {
            Debug.Log("Player begin turn event received!");
        }
        
    }
}

