using UnityEngine;

public class PuzzleButtonScript : InteractBase
{
    [SerializeField] private Vector2Int direction;
    [SerializeField] private PuzzleEngine puzzle;

    public override void Click()
    {
        if (direction == Vector2Int.zero) puzzle.EndPath();
        puzzle.ButtonControlPath(direction);
    }
}
