using System;
using System.Collections;
using UnityEngine;

public class GlobalPuzzleManager : MonoBehaviour
{
    [SerializeField] private PuzzleEngine[] puzzles;
    [SerializeField] private Transform platform;

    void Start()
    {
        PuzzleEngine.puzzleCompleted += CheckedCompletedAllPuzzles;
    }
    void CheckedCompletedAllPuzzles()
    {
        foreach(PuzzleEngine puzzle in puzzles)
        {
            if(!puzzle.gameWon) return;
        }
        Debug.Log("All clear");
        StartCoroutine(RaisePlatform());
        
    }

    IEnumerator RaisePlatform()
    {
        
        while(platform.position.y < 58)
        {
            platform.position += new Vector3(0, 0.01f);
            yield return new WaitForSeconds(0.002f);
        }
        yield return null;
        
    }
}
