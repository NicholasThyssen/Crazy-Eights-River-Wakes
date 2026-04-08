using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameState gameState;
    [HideInInspector] public static GameManager instance;
    [HideInInspector] public List<BaseCharacter> characters;

    private void Awake()
    {
        instance = this;
        gameState = GameState.Default;
        characters = new List<BaseCharacter>();
        var charactersArray = FindObjectsByType<BaseCharacter>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    }

    public List<BaseCharacter> BuildCharactersArray()
    {
        var charactersArray = FindObjectsByType<BaseCharacter>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        return new List<BaseCharacter>(charactersArray);
    }
}

public enum GameState
{
    Default,
}