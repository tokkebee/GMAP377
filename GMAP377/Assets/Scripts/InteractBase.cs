using UnityEngine;

public abstract class InteractBase : MonoBehaviour
{
    //For any script that has the player interacting with it, inherit from this class,
    //and add Click() and Release() as public override void functions.
    public virtual void Click(){} //Start Interact (Left Mouse Button Down)
    public virtual void Release(){} //Start 
}
