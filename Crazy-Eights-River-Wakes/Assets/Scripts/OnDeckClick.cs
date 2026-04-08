using UnityEngine;

public class OnDeckClick : MonoBehaviour
{
    // When user clicks on deck, we want to trigger the human player to draw a card (if it's their turn)
    private void OnMouseDown()
    {
        // Only allow drawing if it's the human player's turn
        var humanPlayer = FindFirstObjectByType<HumanPlayer>();
        if (humanPlayer != null && CardGameManager.instance != null)
        {
            // Optionally, check if it's actually the human's turn
            if (CardGameManager.instance.CurrentPlayerIs(humanPlayer))
            {
                humanPlayer.OnDrawCard();
            }
        }
    }
}
