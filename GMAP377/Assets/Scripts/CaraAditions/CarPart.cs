using System;
using UnityEngine;

public class CarPart : InventoryObject
{
    [SerializeField] private bool startsDisabled = false;
    private void Awake() => FindAnyObjectByType<testCarScript>().partsBroken.Add(gameObject.name);
    protected override void Start()
    {
        base.Start();
        if (startsDisabled) gameObject.SetActive(false);
    }
}
