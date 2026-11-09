using System;
using UnityEngine;

public class InventoryObject : InteractBase
{
    private GameManagerScript gameManager;
    private InteractionPrepper interact;
    protected virtual void Start()
    {
        interact = FindAnyObjectByType<InteractionPrepper>();
        gameManager = FindAnyObjectByType<GameManagerScript>();
    }

    public override void Click()
    {
        if (!gameManager.inventory.Contains(interact.highlightedObject.name))
        {
            gameManager.inventory.Add(interact.highlightedObject.name);
            //Debug.Log($"{highlightedObject} picked up");
            Destroy(interact.highlightedObject);
        }
    }
}
