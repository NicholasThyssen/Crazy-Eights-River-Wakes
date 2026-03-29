using UnityEngine;
using System.Collections.Generic;

public class CardHand : MonoBehaviour
{
    public int playerId = 0;
    public int maxHandSize = 25;

    private List<GameObject> cardSockets;

    void Awake()
    {
        
    }

    private void InitializeHand()
    {
        for (int i = 0; i < maxHandSize; i++)
        {
            // Make a new card socket
        }
    }

    public void AttachCardToHand()
    {
        
    }

    public void PopCardFromHand()
    {
        
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
