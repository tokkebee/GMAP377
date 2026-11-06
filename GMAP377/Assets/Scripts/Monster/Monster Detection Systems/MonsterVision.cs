//using System.Collections.Generic;
using UnityEngine;

public class MonsterVision : MonoBehaviour {
    [SerializeField] private Transform player;
    [SerializeField] private float peripheralDetectionTime = 1.5f;

    public bool canSeePlayer { get; private set; }
    public bool shouldChase { get; private set; }
    public bool hasLastKnownPosition { get; private set; }
    public Vector3 lastKnownPosition { get; private set; }

    [SerializeField] private Transform eyePoint;
    [SerializeField] private LayerMask obstacleMask;

    private bool inCentralVision;
    private bool inPeripheralVision;

    private float peripheralTimer;

    public void EnterVision(VisionZone.VisionType type) {
        if (type == VisionZone.VisionType.Central) {
            inCentralVision = true;

            // shouldChase = true;
            // UpdateLastKnownPosition();
        } else {
            inPeripheralVision = true;
        }

        //UpdateVisibility();
    }

    private bool HasLineOfSight() {
        if (player == null || eyePoint == null) {
            return false;
        }

        Vector3 direction = player.transform.position - eyePoint.position;
        float distance = direction.magnitude;

        if (distance <= 0.01f) {
            return true;
        }

        // if (Physics.Raycast(eyePoint.position, direction.normalized, out RaycastHit hit, distance, obstacleMask, QueryTriggerInteraction.Ignore)) {
        //     return hit.transform == player.transform;
        // }

        bool blocked = Physics.Raycast(eyePoint.position, direction.normalized, out RaycastHit hit, distance, obstacleMask, QueryTriggerInteraction.Ignore);

        Debug.DrawRay(eyePoint.position, direction.normalized * distance, blocked ? Color.red : Color.green);

        if (blocked) {
            Debug.Log($"Line of sight blocked by: {hit.collider.name}");
            return false;
        }

        Debug.Log("Line of sight clear");
        return true;
    }

    public void ExitVision(VisionZone.VisionType type) {
        if (type == VisionZone.VisionType.Central) {
            inCentralVision = false;
        } else {
            inPeripheralVision = false;
            peripheralTimer = 0f;
        }

        // UpdateVisibility();
    }

    void Update() {
        bool hasLightOfSight = (inCentralVision || inPeripheralVision) && HasLineOfSight();

        canSeePlayer = hasLightOfSight;

        if (canSeePlayer) {
            lastKnownPosition = player.transform.position;
            hasLastKnownPosition = true;
            shouldChase = true;
        } else if (inPeripheralVision && !shouldChase) {
            peripheralTimer += Time.deltaTime;

            if (peripheralTimer >= peripheralDetectionTime) {
                shouldChase = true;
                peripheralTimer = 0f;
            }
        } else if (!inPeripheralVision) {
            peripheralTimer = 0f;
        } else {
            Debug.Log($"Player not visible. shouldChase={shouldChase}, " + $"HasLastKnownPosition={hasLastKnownPosition}");
        }

        // if (inCentralVision) {
        //     //UpdateLastKnownPosition();
        //     shouldChase = true;
        //     peripheralTimer = 0f;
        // } else if (inPeripheralVision) {
        //     UpdateLastKnownPosition();

        //     peripheralTimer += Time.deltaTime;

        //     if (peripheralTimer >= peripheralDetectionTime) {
        //         shouldChase = true;
        //     }
        // }
    }

    private void UpdateVisibility() {
        canSeePlayer = (inCentralVision || inPeripheralVision) && HasLineOfSight();
    }

    private void UpdateLastKnownPosition() {
        lastKnownPosition = player.position;
        hasLastKnownPosition = true;
    }

    public void ClearAlert() {
        shouldChase = false;
        hasLastKnownPosition = false;
        canSeePlayer = false;
        peripheralTimer = 0f;
    }
}