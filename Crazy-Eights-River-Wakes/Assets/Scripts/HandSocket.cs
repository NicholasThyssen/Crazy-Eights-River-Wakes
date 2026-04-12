using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HandSocket : XRSocketInteractor
{
    public CardHand cardHand;
    public CardDeck cardDeck;
    public override bool CanHover(IXRHoverInteractable interactable)
    {
        if (!base.CanHover(interactable))
        {
            return false;
        }
        Card targetCard = interactable.transform.gameObject.GetComponent<Card>();
        if (targetCard == null)
        {
            return false;
        }
        if (cardHand != null)
        {
            if (targetCard == cardHand.socketIgnoreCard)
            {
                return false;
            }
            return !cardHand.HasCardInHand(targetCard);
        }
        else if (cardDeck != null)
        {
            CardGameManager cgm = CardGameManager.instance;
            if (cgm.IsPlayerTurn(targetCard.owner))
            {
                bool isValid = cgm.CanPlayCard(targetCard);
                return isValid;
            } 
        }
        return false;
    }

    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        if (!base.CanSelect(interactable))
        {
            return false;
        }
        Card targetCard = interactable.transform.gameObject.GetComponent<Card>();
        if (targetCard == null)
        {
            return false;
        }
        if (cardHand != null)
        {
            Debug.Log("Trying to socket with Card Hand");
            if (targetCard == cardHand.socketIgnoreCard)
            {
                return false;
            }
            return !cardHand.HasCardInHand(targetCard);
        }
        else if (cardDeck != null)
        {
            Debug.Log("Trying to socket with Card Deck");
            CardGameManager cgm = CardGameManager.instance;
            if (cgm.IsPlayerTurn(targetCard.owner))
            {
                bool isValid = cgm.CanPlayCard(targetCard);
                return isValid;
            }
        }
        return false;
    }
}
