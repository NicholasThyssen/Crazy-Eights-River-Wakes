using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        var centerPos = GetCenter(charactersArray.Select(c=>c.transform).ToList());
        // attempt to make order of array match the circular order by sorting by angle from center
        List<BaseCharacter> charactersList = charactersArray.OrderBy(c =>
        {
            Vector3 centerToCharacter = c.transform.position - centerPos;
            float angle = Mathf.Atan2(centerToCharacter.z, centerToCharacter.x);
            return angle;
        }).ToList();
        return charactersList;
    }

    // Get center of all the characters
    private static Vector3 GetCenter(List<Transform> transforms)
    {
        if (transforms.Count == 0)
            return Vector3.zero;

        Vector3 sum = Vector3.zero;

        foreach (Transform t in transforms)
        {
            sum += t.position;
        }

        return sum / transforms.Count;
    }
}

public enum GameState
{
    Default,
}