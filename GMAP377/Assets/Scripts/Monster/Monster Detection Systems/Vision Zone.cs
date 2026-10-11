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
        Debug.Log($"{visionType}: Player entered vision zone");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) {
            return;
        }
        
        monsterVision.ExitVision(visionType);
        Debug.Log($"{visionType}: Player exited vision zone");
    }

    private void OnDisable()
    {
        if (monsterVision != null)
            monsterVision.ExitVision(visionType);
    }
}