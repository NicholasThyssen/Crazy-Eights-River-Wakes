using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class GameSetupMenu : MonoBehaviour
{
    public Button minusCardsButton;
    public Button plusCardsButton;
    public TextMeshProUGUI cardsLabel;

    public Button botRadioButton1;
    public Button botRadioButton2;
    public Button botRadioButton3;
    public void DecrementStartingCards()
    {
        int nextValue = SettingsData.GetStartingCards() - 1;
        SettingsData.SetStartingCards(Math.Max(nextValue, 1));
        if (nextValue <= 1)
        {
            minusCardsButton.enabled = false;
        }
        plusCardsButton.enabled = true;
        cardsLabel.text = nextValue.ToString();
    }

    public void IncrementStartingCards()
    {
        int nextValue = SettingsData.GetStartingCards() + 1;
        SettingsData.SetStartingCards(Math.Min(nextValue, 20));
        if (nextValue >= 20)
        {
            plusCardsButton.enabled = false;
        }
        minusCardsButton.enabled = true;
        cardsLabel.text = nextValue.ToString();
    }

    public void SetBotCount(int count)
    {
        SettingsData.SetBotCount(count);
        // Change settings graphic
        AdjustRadioButtonVisuals();
    }

    public void AdjustRadioButtonVisuals()
    {
        int count = SettingsData.GetBotCount();
        switch (count)
        {
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
            default:
                break;
        }        
    }
}
