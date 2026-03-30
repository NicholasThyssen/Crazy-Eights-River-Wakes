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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public List<BaseCharacter> BuildCharactersArray()
    {
        var charactersArray = FindObjectsByType<BaseCharacter>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        return new List<BaseCharacter>(charactersArray);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Start()
    {
    }

    void Update()
    {
    }
}

public enum GameState
{
    Default,
}