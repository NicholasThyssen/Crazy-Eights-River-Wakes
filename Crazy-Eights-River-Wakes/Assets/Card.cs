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

    private Material face_material;

    void Start() {
        face_material = transform.GetChild(0).GetComponent<MeshRenderer>().material;
        print(face_material.GetFloat("Suit"));
        UpdateCardFace();
    }

    private void UpdateCardFace() {
        face_material.SetFloat("Suit", (int)suit);
        face_material.SetFloat("Rank", rank);
    }

    public bool IsValidMatch() {
        return false;
    }
}
