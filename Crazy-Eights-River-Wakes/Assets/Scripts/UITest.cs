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
            if (suitUI.gameObject.activeSelf)
                suitUI.Hide();
            else
                suitUI.Show(null);
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            if (swapUI.gameObject.activeSelf)
                swapUI.Hide();
            else
                swapUI.Show(null, GameManager.instance.characters);
        }
    }

}