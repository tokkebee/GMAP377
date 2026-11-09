using System;
using System.Collections;
using UnityEngine;

public class GlobalPuzzleManager : MonoBehaviour
{
    [SerializeField] private PuzzleEngine[] puzzles;
    [SerializeField] private Transform poolSurface;
    [SerializeField] private Collider poolCollider;
    [SerializeField] private float dropAmount, dropTime;

    void Start()
    {
        PuzzleEngine.puzzleCompleted += CheckedCompletedAllPuzzles;
    }
    void CheckedCompletedAllPuzzles()
    {
        StartCoroutine(LowerPlatform());
        foreach(PuzzleEngine puzzle in puzzles)
        {
            if(!puzzle.gameWon) return;
        }
        Debug.Log("All clear");
        poolCollider.enabled = false;
    }
    
    IEnumerator LowerPlatform()
    {
        float dropIncrement = (dropAmount / dropTime)/60;
        Debug.Log(dropIncrement);
        float timeElapsed = 0f;
        while(timeElapsed<dropTime)
        {
            poolSurface.position += Vector3.down*dropIncrement;
            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }
}
