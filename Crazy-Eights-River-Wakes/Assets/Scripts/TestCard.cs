using TMPro;
using UnityEngine;
using System.Collections.Generic;
using Unity.XR.CoreUtils;

public class TestCard : Card
{
    private TextMeshPro text;
    private Renderer rend;
    private Material faceMaterial;

    private Vector3 throwVelocity;
    private Vector3 lastControllerPos;
    private Vector3 controllerVelocity;
    private bool isBeingHeld = false;

    protected override void Awake()
    {
        base.Awake();
        rend = GetComponent<Renderer>();
        text = GetComponentInChildren<TextMeshPro>();
        faceMaterial = transform.GetChild(0).GetComponent<MeshRenderer>().material;

        if (faceMaterial != null)
            SetFaceTexture(suit, rank);

        grab.selectEntered.AddListener(OnCardGrabStarted);
        grab.selectExited.AddListener(OnCardReleased);
    }

    private void OnCardGrabStarted(UnityEngine.XR.Interaction.Toolkit.SelectEnterEventArgs args)
    {
        isBeingHeld = true;
        currentlyHeld = true;
        controllerVelocity = Vector3.zero;
        lastControllerPos = args.interactorObject.transform.position;

        grab.trackPosition = false;
        grab.trackRotation = false;

        Transform controllerTransform = args.interactorObject.transform;
        transform.SetParent(controllerTransform, true);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(0, 180, 0);
    }

    private void OnCardReleased(UnityEngine.XR.Interaction.Toolkit.SelectExitEventArgs args)
    {
        isBeingHeld = false;
        currentlyHeld = false; // ? ADD

        grab.trackPosition = true;
        grab.trackRotation = true;

        transform.SetParent(null, true);
        EnablePhysics();
        rb.linearVelocity = controllerVelocity;
    }

    private void Update()
    {
        if (isBeingHeld && grab.interactorsSelecting.Count > 0)
        {
            Transform controllerTransform = grab.interactorsSelecting[0].transform;

            transform.position = controllerTransform.position;
            transform.rotation = controllerTransform.rotation * Quaternion.Euler(0, 180, 0);

            // No threshold — track all movement
            controllerVelocity = (controllerTransform.position - lastControllerPos) / Time.deltaTime;
            lastControllerPos = controllerTransform.position;
        }
    }

    public void UpdateSuitDisplay(CardSuit newSuit)
    {
        if (faceMaterial == null) return;
        // Use instance to avoid affecting all cards
        MeshRenderer mr = transform.GetChild(0).GetComponent<MeshRenderer>();
        Material instanceMat = mr.material; // .material creates an instance
        instanceMat.SetFloat("Suit", (int)newSuit);
        instanceMat.SetFloat("Rank", 7);
        faceMaterial = instanceMat;
    }

    public void Initialize(CardSuit suit, CardRank rank)
    {
        this.suit = suit;
        this.rank = rank;
        
        if (faceMaterial != null)
        {
            SetFaceTexture(suit, rank);
        }

        // Fetch components here in case Awake hasn't run yet
        if (rend == null) rend = GetComponent<Renderer>();
        if (text == null) text = GetComponentInChildren<TextMeshPro>();

        if (text != null)
            text.text = GetTextFromRank(rank);

        if (rend != null)
            rend.material.color = GetColorFromSuit(suit);
    }

    // Sets the face texture of the card.
    private void SetFaceTexture(CardSuit suit, CardRank rank)
    {
        if (rank == CardRank.Eight)
        {
            faceMaterial.SetFloat("Suit", 0);
            faceMaterial.SetFloat("Rank", 7);
        }
        else if (rank == CardRank.Swap)
        {
            faceMaterial.SetFloat("Suit", 1);
            faceMaterial.SetFloat("Rank", 7);
        }
        else
        {
            faceMaterial.SetFloat("Suit", (int)suit);
            faceMaterial.SetFloat("Rank", (int)rank);
        }
    }

    public void PlaySpecialEffect(string effectName)
    {
        
    }

    // Returns a non-zero value if the card can be played to the current pile.
    // I'd make this a boolean, but I don't know how wilds and swaps are planned to be handled.
    private int IsValidPlayable(CardSuit suit, CardRank rank)
    {
        if (this.rank == CardRank.Eight)
        {
            // Eights are wild
            return 2;
        }
        else if (this.rank == CardRank.Swap)
        {
            // Swap cards also don't care about suit
            return 3;
        }
        else if (this.rank == rank || this.suit == suit)
        {
            // Otherwise, return a non-zero value if ranks or suits match
            return 1;
        }
        return 0;
    }

    // Override in TestCard.cs
    public override void ReRegisterGrabListeners()
    {
        base.ReRegisterGrabListeners();
        grab.selectExited.RemoveAllListeners();
        grab.selectExited.AddListener(OnCardReleased); // ? always fresh on each return to hand
        grab.selectEntered.AddListener(OnCardGrabStarted);
    }

    private void EnableXRInteractions()
    {
        
    }

    private void DisableXRInteractions()
    {
        
    }

    private static Color GetColorFromSuit(CardSuit suit)
    {
        switch (suit)
        {
            case CardSuit.Hearts: return Color.red;
            case CardSuit.Diamonds: return Color.yellow;
            case CardSuit.Clubs: return Color.cyan;
            case CardSuit.Spades: return Color.blue;
            default: return Color.white;
        }
    }

    private static string GetTextFromRank(CardRank rank) => rank switch
    {
        CardRank.Ace => "A",
        CardRank.Two => "2",
        CardRank.Three => "3",
        CardRank.Four => "4",
        CardRank.Five => "5",
        CardRank.Six => "6",
        CardRank.Seven => "7",
        CardRank.Eight => "8",
        CardRank.Nine => "9",
        CardRank.Ten => "10",
        CardRank.Reverse => "R",
        CardRank.Swap => "Swap",
        CardRank.Skip => "Skip",
        CardRank.PlusOne => "+1",
        _ => "other",
    };
}