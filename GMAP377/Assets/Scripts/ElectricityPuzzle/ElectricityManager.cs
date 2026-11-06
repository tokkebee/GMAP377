using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ElectricityManager : MonoBehaviour
{
    public int targetNum;
    private int[] _forbiddenNumbers = {1,2,4,8,16,32,64,128};
    public List<ElectricalLever> enabledLevers;
    [SerializeField] private int leverSum;
    
    //ALPHA MATERIAL - DELETE ONCE MORE POLISHED
    [SerializeField] private TextMeshPro mapText;
    [SerializeField] private Renderer safeRenderer;

    void Start()
    {
        targetNum = ChooseNum();
        mapText.text = $"={targetNum}";
        safeRenderer.material.color = Color.red;
    }
    
    private int ChooseNum()
    {
        int numChoice = Random.Range(1, 256);
        if (_forbiddenNumbers.Contains(numChoice)) return ChooseNum();
        return numChoice;
    }

    public void Evaluate()
    {
        leverSum = 0;
        foreach (var lever in enabledLevers)
            if (lever.enabled)
                leverSum += lever.value;
        if (leverSum == targetNum) Unlock();
    }

    private void Unlock()
    {
        Debug.Log("Lever Puzzle Solved");
        safeRenderer.material.color = Color.green;
    }
}
