using System.Collections.Generic;
using UnityEngine;

public class MonsterVision : MonoBehaviour {
    [SerializeField] private Transform player;
    [SerializeField] private float peripheralDetectionTime = 2f;

    public bool canSeePlayer { get; private set; }
    public bool shouldChase { get; private set; }
    public bool hasLastKnownPosition { get; private set; }
    public Vector3 lastKnownPosition { get; private set; }

    private bool inCentralVision;
    private bool inPeripheralVision;

    private float peripheralTimer;

    public void EnterVision(VisionZone.VisionType type) {
        if (type == VisionZone.VisionType.Central) {
            inCentralVision = true;

            shouldChase = true;
            UpdateLastKnownPosition();
        } else {
            inPeripheralVision = true;
        }

        UpdateVisibility();
    }

    public void ExitVision(VisionZone.VisionType type) {
        if (type == VisionZone.VisionType.Central) {
            inCentralVision = false;
        } else {
            inPeripheralVision = false;
            peripheralTimer = 0f;
        }

        UpdateVisibility();
    }

    void Update() {
        if (player == null) {
            return;
        }

        if (inCentralVision) {
            UpdateLastKnownPosition();
            shouldChase = true;
            peripheralTimer = 0f;
        } else if (inPeripheralVision) {
            UpdateLastKnownPosition();

            peripheralTimer += Time.deltaTime;

            if (peripheralTimer >= peripheralDetectionTime) {
                shouldChase = true;
            }
        }
    }

    private void UpdateVisibility() {
        canSeePlayer = inCentralVision || inPeripheralVision;
    }

    private void UpdateLastKnownPosition() {
        lastKnownPosition = player.position;
        hasLastKnownPosition = true;
    }

    public void ClearAlert() {
        shouldChase = false;
        hasLastKnownPosition = false;
        peripheralTimer = 0f;
    }
}