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
    private List<Transform> nearbyTreeLeaves = new List<Transform>();

    private void Awake()
    {
        instance = this;
        gameState = GameState.Default;
        characters = BuildCharactersArray();
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

    private void Start()
    {
        InvokeRepeating(nameof(FindNearbyTrees), 0f, 5f);
    }

    private void Update()
    {
        SwayTrees();
    }

    private void FindNearbyTrees()
    {
        nearbyTreeLeaves.Clear();
        int treeLeavesMask = LayerMask.GetMask("TreeLeaves");
        if (characters.Count < 1)
        {
            return;
        }
        Vector3 startPosition = characters[UnityEngine.Random.Range(0, characters.Count)].transform.position;

        Collider[] hits = Physics.OverlapSphere(startPosition, 80, treeLeavesMask);

        foreach (Collider hit in hits)
        {
            Transform t = hit.transform;
            nearbyTreeLeaves.Add(t);
        }
    }

    private void SwayTrees()
    {
        Debug.Log("swaying Trees is length " + this.nearbyTreeLeaves.Count);
        foreach (Transform treeTop in this.nearbyTreeLeaves)
        {

            float swaySpeed = 0.65f;
            float swayAmount = 2f;

            float x = Mathf.Sin(Time.time * swaySpeed + treeTop.position.x) * swayAmount;
            float y = Mathf.Sin(Time.time * swaySpeed + treeTop.position.y) * swayAmount;
            float z = Mathf.Sin(Time.time * swaySpeed + treeTop.position.z) * swayAmount;

            treeTop.localRotation = Quaternion.Euler(x, 0f, z);
        }
    }
}

public enum GameState
{
    Default,
}