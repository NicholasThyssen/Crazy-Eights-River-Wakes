using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DeckSocket : XRSocketInteractor
{
    public CardDeck cardDeck;
    public override bool CanHover(IXRHoverInteractable interactable)
    {
        CardGameManager cgm = CardGameManager.instance;
        Card targetCard = interactable.transform.gameObject.GetComponent<Card>();
        if (targetCard == null)
        {
            return false;
        }
        if (!cgm.IsPlayerTurn(targetCard.owner))
        {
            return false;
        }
        return cgm.CanPlayCard(targetCard);
    }

    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        CardGameManager cgm = CardGameManager.instance;
        Card targetCard = interactable.transform.gameObject.GetComponent<Card>();
        if (targetCard == null)
        {
            return false;
        }
        if (!cgm.IsPlayerTurn(targetCard.owner))
        {
            return false;
        }
        return cgm.CanPlayCard(targetCard);
    }
}
