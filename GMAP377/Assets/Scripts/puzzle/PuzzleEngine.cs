using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(LineRenderer))]
public class PuzzleEngine : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int columns = 5;
    [SerializeField] private int rows = 5;
    [SerializeField] private float cellSize = 1.5f;
    [SerializeField] private Vector2Int exitPos = new Vector2Int(4, 4);
    [SerializeField] private float targetAnswer = 25f;

    [Header("Puzzle - one line per row, top row first, cells split by spaces, '.' = empty")]
    [SerializeField] private string[] puzzle =
    {
        "5 . * . 5",
        ". . . . .",
        ". . . . .",
        ". . . . .",
        ". . . . .",
    };

    [Header("Refs")]
    [SerializeField] private Camera puzzleCamera;
    [SerializeField] private TMP_Text hudText;

    private static readonly string[] Operations = { "+", "-", "*", "/" };

    private string[,] board;
    private readonly List<Vector2Int> path = new List<Vector2Int>();
    private LineRenderer line;
    private string equation = "";
    private bool isDragging, gameWon;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;                                  // cell positions are world space
        line.startWidth = line.endWidth = cellSize * 0.5f;         // scene value (1) is far wider than a cell
        if (line.sharedMaterial == null)
        {
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit");
            if (shader != null) line.material = new Material(shader);
        }

        BuildBoard();
        BuildTiles();
        UpdateHud();
    }

    
    private void BuildBoard()
    {
        board = new string[columns, rows];
        for (int c = 0; c < columns; c++)
            for (int r = 0; r < rows; r++)
                board[c, r] = " ";

        for (int r = 0; r < rows && r < puzzle.Length; r++)
        {
            string[] cells = puzzle[r].Split(' ');
            for (int c = 0; c < columns && c < cells.Length; c++)
                board[c, r] = cells[c] == "." ? " " : cells[c];
        }
    }

    private void BuildTiles()
    {
        for (int c = 0; c < columns; c++)
            for (int r = 0; r < rows; r++)
            {
                var tile = new GameObject($"Tile {c},{r}", typeof(RectTransform));
                tile.layer = gameObject.layer;
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = new Vector3(c * cellSize, 0f, (rows - 1 - r) * cellSize);
 
                tile.transform.rotation = puzzleCamera != null ? puzzleCamera.transform.rotation : Quaternion.Euler(90f, 0f, 0f);

                var text = tile.AddComponent<TextMeshPro>();
                text.rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = cellSize * 8f;

                tile.AddComponent<GridTileVisual>().SetupTile(board[c, r], new Vector2Int(c, r) == exitPos);
            }
    }



    private Vector3 CellToWorld(Vector2Int cell) =>
        transform.position + new Vector3(cell.x * cellSize, 0f, (rows - 1 - cell.y) * cellSize);

    private Vector2Int WorldToCell(Vector3 world) =>
        new Vector2Int(Mathf.RoundToInt((world.x - transform.position.x) / cellSize),
                       rows - 1 - Mathf.RoundToInt((world.z - transform.position.z) / cellSize));

    private bool InBounds(Vector2Int cell) => cell.x >= 0 && cell.x < columns && cell.y >= 0 && cell.y < rows;

    private bool PointerCell(out Vector2Int cell)
    {
        cell = default;
        Camera view = ViewCamera;
        if (view == null || Pointer.current == null) return false;

        Ray ray = view.ScreenPointToRay(Pointer.current.position.ReadValue());
        if (!new Plane(Vector3.up, transform.position).Raycast(ray, out float distance)) return false;

        cell = WorldToCell(ray.GetPoint(distance));
        return true;
    }

    
    private Camera ViewCamera => Camera.main != null ? Camera.main : puzzleCamera;

    
    public void HandleInput(InputAction.CallbackContext context)
    {
        if (gameWon) return;

        if (context.performed) BeginPath();
        else if (context.canceled) EndPath();
    }

    void Update()
    {
        if (!isDragging) return;
        if (PointerCell(out Vector2Int cell)) ExtendPath(cell);
    }

    private void BeginPath()
    {
        if (!PointerCell(out Vector2Int cell)) return;
        if (cell != Vector2Int.zero) return; 

        path.Clear();
        path.Add(cell);
        isDragging = true;
        RebuildEquation();
        DrawLine();
        UpdateHud();
    }

    private void ExtendPath(Vector2Int cell)
    {
        if (!InBounds(cell)) return;

        Vector2Int last = path[path.Count - 1];
        if (cell == last) return;

        int seen = path.IndexOf(cell);
        if (seen >= 0) // dragged back over the line: undo to that cell
        {
            path.RemoveRange(seen + 1, path.Count - seen - 1);
            RebuildEquation();
            DrawLine();
            UpdateHud();
            return;
        }

        if (Mathf.Abs(cell.x - last.x) + Mathf.Abs(cell.y - last.y) != 1) return; // orthogonal steps only

        string value = board[cell.x, cell.y];
        if (value != " " && IsOperator(value) && EndsWithOperator()) return; // no two operators in a row

        path.Add(cell);
        RebuildEquation();
        DrawLine();
        UpdateHud();
    }

    private void EndPath()
    {
        isDragging = false;
        if (path.Count > 0 && path[path.Count - 1] == exitPos)
        {
            VerifyWin();
            return;
        }

        ResetPath(); 
        UpdateHud();
    }



    private void VerifyWin()
    {
        float value = Evaluate(equation);
        string expression = equation;

        if (Mathf.Approximately(value, targetAnswer))
        {
            gameWon = true;
            Debug.Log($"PUZZLE SOLVED! {expression} = {value}");
            SetHud($"{expression} = {value}   SOLVED!");
            return;
        }

        Debug.Log($"{expression} = {value}, not {targetAnswer}");
        ResetPath();
        SetHud($"{expression} = {value} - needs {targetAnswer}, try again");
    }

    private void ResetPath()
    {
        path.Clear();
        RebuildEquation();
        DrawLine();
    }

    private void RebuildEquation()
    {
        var values = new List<string>();
        foreach (Vector2Int cell in path)
            if (board[cell.x, cell.y] != " ")
                values.Add(board[cell.x, cell.y]);

        equation = string.Join(" ", values);
    }

    private static bool IsOperator(string value) => System.Array.IndexOf(Operations, value) >= 0;

    private bool EndsWithOperator()
    {
        foreach (string op in Operations)
            if (equation.EndsWith(op)) return true;
        return false;
    }


    private static float Evaluate(string expression)
    {
        if (string.IsNullOrEmpty(expression)) return 0f;

        string[] parts = expression.Split(' ');
        float value = float.Parse(parts[0]);

        for (int i = 1; i + 1 < parts.Length; i += 2)
        {
            float operand = float.Parse(parts[i + 1]);
            switch (parts[i])
            {
                case "+": value += operand; break;
                case "-": value -= operand; break;
                case "*": value *= operand; break;
                case "/": value /= operand; break;
            }
        }
        return value;
    }



    private void DrawLine()
    {
        line.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
            line.SetPosition(i, CellToWorld(path[i]) - Vector3.up * 0.05f); 
    }

    private void SetHud(string message)
    {
        if (hudText != null) hudText.text = message;
    }

    private void UpdateHud() => SetHud(equation == ""
        ? $"Target {targetAnswer}   |   drag from the top-left cell"
        : $"Target {targetAnswer}   |   {equation}");
}
