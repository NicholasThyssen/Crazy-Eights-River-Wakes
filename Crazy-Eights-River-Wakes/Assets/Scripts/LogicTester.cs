using UnityEngine;
using UnityEngine.InputSystem;

public class LogicTester : MonoBehaviour
{
    public BaseCharacter testPlayer;
    public Card eightCard;
    public Card swapCard;

    void Update()
    {
        if (CardGameManager.instance == null)
            Debug.LogError("CardGameManager.instance is NULL");

        if (testPlayer == null)
            Debug.LogError("testPlayer is NULL");

        if (eightCard == null)
            Debug.LogError("eightCard is NULL");

        if (swapCard == null)
            Debug.LogError("swapCard is NULL");

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            Debug.Log("Testing Eight card...");
            CardGameManager.instance.EndTurn(testPlayer, eightCard);
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            Debug.Log("Testing Swap card...");
            CardGameManager.instance.EndTurn(testPlayer, swapCard);
        }
    }
}