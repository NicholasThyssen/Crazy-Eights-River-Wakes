using UnityEngine;

public class Card : MonoBehaviour
{
    public enum SuitType {
        blue = 0,
        green = 1,
        orange = 2,
        gray = 3
    };

    [Tooltip("The player who currently owns this card and can see its face value unrestricted. -1 means that nobody owns this card (visible to all players)")] public int PlayerId = 0;

    [Tooltip("The suit (color/symbol/type) for this card.")] public SuitType Suit = SuitType.blue;

    [Tooltip("The rank (face value) for this card.\n0-9 correspond to 1-10 (face value-1), 10 to jacks, 11 to queens, and 12 to kings."), Range(0, 12)]  public int Rank = 0;

    private Material face_material; // This material corresponds to the card's face. The displayed texture is manipulated by using the shader graph and the Suit / Rank uniforms.

    void Start() {
        face_material = transform.GetChild(0).GetComponent<MeshRenderer>().material;
        UpdateCardFace();
        print(face_material.GetFloat("Rank"));
    }

    private void UpdateCardFace() {
        face_material.SetFloat("Suit", (int)Suit);
        face_material.SetFloat("Rank", Rank);
    }

    public bool CanViewFaceValue(int viewer_id) {
        if (PlayerId == -1 || viewer_id == PlayerId) {
            return true;
        }
        return false;
    }

    public bool IsValidMatch(Card other) {
        // 8s are wild: they always match
        if (Rank == 7) {
            return true;
        }
        // Otherwise, match is valid if suit or rank matches the other card's
        else if (Suit == other.Suit || Rank == other.Rank) {
            return true;
        }
        return false;
    }

    // Function for changing the suit of an 8
    public void ChangeSuit(SuitType new_suit) {
        Suit = new_suit;
        // TODO: Insert a special effect coroutine here (spawn smoke clouds? play a sound effect? make the card glow?)
        UpdateCardFace();
    }

    public int GetCardScore() {
        // In Crazy 8s scoring (lower score = better), a held 8 incurs 50 points
        if (Rank == 7) {
            return 50;
        }
        // Jacks, queens, and kings are 10 points
        else if (Rank > 8) {
            return 10;
        }
        // All other cards are face value
        return Rank + 1;
    }
}
