using UnityEngine;
using UnityEngine.InputSystem;

public class UITest : MonoBehaviour
{
    public SuitSelectionUI suitUI;
    public SwapSelectionUI swapUI;

    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            Debug.Log("Showing Suit UI");
            suitUI.Show(null); // or pass a dummy player
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            Debug.Log("Showing Swap UI");
            Debug.Log("Characters count: " + GameManager.instance.characters.Count);

            foreach (var p in GameManager.instance.characters)
            {
                Debug.Log("Player: " + p);
            }


            swapUI.Show(null, GameManager.instance.characters);
            
        }
    }
    void Start()
    {
        
    }
}