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
        this.gameState = GameState.Default;
        this.characters = new List<BaseCharacter>();
        var charactersArray = FindObjectsByType<BaseCharacter>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        this.characters = new List<BaseCharacter>(charactersArray);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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