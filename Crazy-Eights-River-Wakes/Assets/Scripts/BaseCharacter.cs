using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCharacter : MonoBehaviour
{
    protected bool isCrouched = false;
    protected Animator animator;
    protected List<Card> cards;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        cards = new List<Card>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddCard(Card card)
    {
        this.cards.Add(card);
    }

    // This should handle what happens when CardManager notifies this player that it is their turn
    public abstract void BeginCardTurn();

    public void EndTurn() {
        throw new System.NotImplementedException();
    }

    public void ToggleCrouch()
    {
        SetCrouch(!!isCrouched);
    }

    public void SetCrouch(bool shouldCrouch)
    {
        isCrouched = shouldCrouch;
        animator.SetBool("IsCrouching", isCrouched);
    }
}
