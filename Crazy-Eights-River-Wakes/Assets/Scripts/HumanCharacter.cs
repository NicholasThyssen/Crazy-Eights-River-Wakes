using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

public class HandSocketFilter : MonoBehaviour, IXRHoverFilter
{
    public bool canProcess => isActiveAndEnabled;

    private BaseCharacter owner;

    public void Awake()
    {
        owner = null;
    }

    public void UpdateOwner(BaseCharacter owner)
    {
        this.owner = owner;
    }

    public bool Process(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
    {
        BaseCharacter target = interactable.transform.gameObject.GetComponent<BaseCharacter>();
        if (owner == null)
        {
            return false;
        }
        return target == owner;
    }
}

public class HumanPlayer : BaseCharacter
{
    private bool canAct = false;

    public void AssignListeners()
    {
        CardGameManager cgm = CardGameManager.instance;
        // Assign listeners to our own signal
        playerPlayedCard.AddListener(cgm.PlayerPlayedCard);
        playerTurnEnded.AddListener(cgm.PlayerTurnEnded);
        // Listen to the manager's signals
        cgm.beginPlayerTurn.AddListener(BeginPlayerTurn);
        // Connect to the discard pile
        CardDeck pile = cgm.discardPile;
        pile.cardPlayedToDeck.AddListener(PlayedToDiscardPile);
    }


    public override void BeginPlayerTurn(BaseCharacter player)
    {
        if (player == this)
        {
            Debug.Log("Player begin turn event received!");
            canAct = true;
        }
    }

    public override void FinishPlayerTurn(BaseCharacter player)
    {
        if (player == this)
        {
            canAct = false;
            EndTurn();
        }
    }

    public void PlayedToDiscardPile(BaseCharacter player, Card card)
    {
        if (player == this)
        {
            playerPlayedCard.Invoke(this, card);
        }
    }
}

