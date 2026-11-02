using System;
using UnityEngine;

public class testListenerScript : MonoBehaviour
{
    private void OnEnable()
    {
        BasicMovement.onPlayerCrouch += DetectCrouching;
        BasicMovement.onPlayerStand += DetectStanding;
    }

    private void OnDisable()
    {
        BasicMovement.onPlayerCrouch -= DetectCrouching;
        BasicMovement.onPlayerStand -= DetectStanding;
    }

    void DetectCrouching()
    {
        Debug.Log("The Player Is Crouching.");
    }

    void DetectStanding()
    {
        Debug.Log("The Player Is Standing.");
    }
}
