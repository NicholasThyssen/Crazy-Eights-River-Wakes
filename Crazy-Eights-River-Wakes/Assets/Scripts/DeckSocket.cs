using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DeckSocket : XRSocketInteractor
{
    public CardDeck cardDeck;
    public override bool CanHover(IXRHoverInteractable interactable)
    {
        CardGameManager cgm = CardGameManager.instance;
        Card targetCard = interactable.transform.gameObject.GetComponent<Card>();
        Debug.Log("SOMETHING?");
        if (targetCard == null)
        {
            return false;
        }
        Debug.Log("SOMETHING??");
        if (!cgm.IsPlayerTurn(targetCard.owner))
        {
            return false;
        }
        Debug.Log("SOMETHING???");
        bool isValid = cgm.CanPlayCard(targetCard);
        return isValid;
    }

    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        CardGameManager cgm = CardGameManager.instance;
        Card targetCard = interactable.transform.gameObject.GetComponent<Card>();
        Debug.Log("SOMETHING?");
        if (targetCard == null)
        {
            return false;
        }
        Debug.Log("SOMETHING??");
        if (!cgm.IsPlayerTurn(targetCard.owner))
        {
            return false;
        }
        Debug.Log("SOMETHING???");
        bool isValid = cgm.CanPlayCard(targetCard);
        return isValid;
    }
}
