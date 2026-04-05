using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

public class HandSocket : XRSocketInteractor
{
    public CardHand cardHand;
    public override bool CanHover(IXRHoverInteractable interactable)
    {
        Card targetCard = interactable.transform.gameObject.GetComponent<Card>();
        if (targetCard == cardHand.socketIgnoreCard)
        {
            return false;
        }
        return !cardHand.HasCardInHand(targetCard);
    }

    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        Card targetCard = interactable.transform.gameObject.GetComponent<Card>();
        if (targetCard == null)
        {
            return false;
        }
        if (targetCard == cardHand.socketIgnoreCard)
        {
            return false;
        }
        return !cardHand.HasCardInHand(targetCard);
    }
}
