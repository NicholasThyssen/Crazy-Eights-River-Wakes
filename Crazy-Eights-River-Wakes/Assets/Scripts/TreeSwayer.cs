using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

public class TreeSwayer : MonoBehaviour
{
    private List<Transform> nearbyTreeLeaves = new List<Transform>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating(nameof(FindNearbyTrees), 0f, 5f);
    }

    // Update is called once per frame
    void Update()
    {
        SwayTrees();
    }

    private void FindNearbyTrees()
    {
        nearbyTreeLeaves.Clear();
        int treeLeavesMask = LayerMask.GetMask("TreeLeaves");
        var characters = GameManager.instance.characters;
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
