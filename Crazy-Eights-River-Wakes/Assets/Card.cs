using UnityEngine;

public class Card : MonoBehaviour
{
    public enum Suit {
        blue = 0,
        green = 1,
        orange = 2,
        gray = 3
    };

    [Tooltip("The suit (color/symbol/type) for this card.")] public Suit suit = Suit.blue;

    [Tooltip("The rank (face value) for this card.\n0-9 correspond to 1-10 (face value-1), 10 to jacks, 11 to queens, and 12 to kings."), Range(0, 12)]  public int rank = 0;

    private Material face_material; // This material corresponds to the card's face. The displayed texture is manipulated by using the shader graph and the Suit / Rank uniforms.

    void Start() {
        face_material = transform.GetChild(0).GetComponent<MeshRenderer>().material;
        UpdateCardFace();
    }

    private void UpdateCardFace() {
        face_material.SetFloat("Suit", (int)suit);
        face_material.SetFloat("Rank", rank);
    }

    public bool IsValidMatch(Card other) {
        // 8s are wild: they always match
        if (rank == 8) {
            return true;
        }
        // Otherwise, match is valid if suit or rank matches the other card's
        else if (suit == other.suit || rank == other.rank) {
            return true;
        }
        return false;
    }

    // Function for changing the suit of an 8
    public bool ChangeSuit(Suit new_suit) {
        suit = new_suit;
        // TODO: Insert a special effect coroutine here (spawn smoke clouds? play a sound effect? make the card glow?)
        UpdateCardFace();
    }
}
