using UnityEngine;

public abstract class BaseCharacter : MonoBehaviour
{
    protected bool isCrouched = false;
    protected Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
