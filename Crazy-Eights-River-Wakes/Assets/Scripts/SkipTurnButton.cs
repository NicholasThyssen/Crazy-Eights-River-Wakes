using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SkipTurnButton : MonoBehaviour
{
    public bool ShouldSkip(HumanPlayer skipper)
    {
        CardGameManager cgm = CardGameManager.instance;
        if (cgm.IsPlayerTurn(skipper))
        {
            if (skipper.CanSkip())
            {
                return true;
            }
        }
        return false;
    }

    public void HandleSkipActivation(ActivateEventArgs action)
    {
        var interactor = action.interactorObject;
        HumanPlayer firingUser = interactor.transform.GetComponentInParent<HumanPlayer>();

        if (ShouldSkip(firingUser))
        {
            Debug.Log("Player pressed skip button.");
            firingUser.EndTurn();
        }
    }
}
