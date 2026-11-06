using UnityEngine;

public class ElectricalLever : InteractBase
{
    public int value;
    public bool enabled;
    private ElectricityManager _manager;

    private void Awake()
    {
        enabled = false;
        if (!_manager) _manager = FindFirstObjectByType<ElectricityManager>();
    }
    public void ToggleLever()
    {
        enabled = !enabled;
        if (enabled) _manager.enabledLevers.Add(this);
        else _manager.enabledLevers.Remove(this);
        _manager.Evaluate();
        
        //ALPHA TEST WORK -- REMOVE ONCE LEVERS HAVE ANIMATIONS
        transform.localScale = enabled ? new Vector3(1,3,1) : new Vector3(1,2,1);
        Debug.Log(enabled?"Enabled":"Disabled");
    }
    public override void Click() => ToggleLever();
}
