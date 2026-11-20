using JetBrains.Annotations;
using UnityEngine;

public class AlertBroadcastManager : MonoBehaviour
{
    /*Basic handler of alert events.
     * When something makes a sound that may alert the monster or trigger some other sound-detection event,
     * call the Alert function on this object. In the object recieving the alert broadcast, multiply the
     * volume of the noise by the distance of the object to that noise, then check if it's under a
     * sensitivity threshold to determine if the detector hears the sound 
     */
    public delegate void AlertHandler(Vector3 pos, float volume);
    [CanBeNull] public event AlertHandler AlertEvent;
    
    public void Alert(Vector3 pos, float volume) => AlertEvent?.Invoke(pos, volume);
}
