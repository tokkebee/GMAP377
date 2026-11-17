using System;
using UnityEngine;

public class InventoryObject : InteractBase
{
    private PlayerManager playerManager;
    private InteractionPrepper interact;
    protected virtual void Start()
    {
        interact = FindAnyObjectByType<InteractionPrepper>();
        playerManager = FindAnyObjectByType<PlayerManager>();
    }

    public override void Click()
    {
        if (!playerManager.inventory.Contains(interact.highlightedObject.name))
        {
            playerManager.inventory.Add(interact.highlightedObject.name);
            //Debug.Log($"{highlightedObject} picked up");
            Destroy(interact.highlightedObject);
        }
    }
}
