using UnityEngine;

public class FallTeleporter : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private BoxCollider collider;
    void Awake()
    {
        collider = GetComponent<BoxCollider>();
    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     Debug.Log(other);
    //     CardDeck deck = other.gameObject.GetComponent<CardDeck>();
    //     Debug.Log(deck);
    //     if (deck != null)
    //     {
    //         deck.Warpback();
    //     }
    //     Card card = other.gameObject.GetComponent<Card>();
    //     if (card != null)
    //     {
    //         card.Warpback();
    //     }
    // }
}
