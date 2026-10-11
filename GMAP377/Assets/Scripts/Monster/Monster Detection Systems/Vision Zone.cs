using UnityEngine;

// This script is a crude implementation of the monster's visual detection system

public class VisionZone : MonoBehaviour
{
    public enum VisionType {
        Central,
        Peripheral
    }

    [SerializeField] private VisionType visionType;
    [SerializeField] private MonsterVision monsterVision;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) {
            return;
        }
        
        monsterVision.EnterVision(visionType);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) {
            return;
        }
        
        monsterVision.ExitVision(visionType);
    }
}