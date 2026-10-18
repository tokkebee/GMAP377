using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Mathematics;
using Unity.Multiplayer.Center.Common;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;


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
    
    [SerializeField] private ScreenScript screen;
    [SerializeField] private Canvas screenCanvas;
 

    void Awake()
    {
        line = screenCanvas.GetComponent<LineRenderer>();
        line.useWorldSpace = false;                                  // cell positions are world space
        line.startWidth = line.endWidth = cellSize * 0.005f;         // scene value (1) is far wider than a cell
        if (line.sharedMaterial == null)
        {
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit");
            if (shader != null) line.material = new Material(shader);
        }
        GenerateRandomPuzzle();
        
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
        Vector2 centerOffSet = new Vector2();
        centerOffSet.x = 500 - cellSize * columns;
        centerOffSet.y = 500 - cellSize * rows;
        for (int c = 0; c < columns; c++)
            for (int r = 0; r < rows; r++)
            {
                var tile = new GameObject($"Tile {c},{r}", typeof(RectTransform));
                tile.layer = gameObject.layer;
                tile.transform.SetParent(screenCanvas.transform, false);
                tile.transform.localPosition = new Vector3(c * cellSize + centerOffSet.x, (rows - 1 - r) * cellSize + centerOffSet.y, 0f);
 
                tile.transform.rotation = puzzleCamera != null ? puzzleCamera.transform.rotation : Quaternion.Euler(90f, 0f, 0f);

                var text = tile.AddComponent<TextMeshProUGUI>();
                text.rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = cellSize;

                tile.AddComponent<GridTileVisual>().SetupTile(board[c, r], new Vector2Int(c, r) == exitPos);
            }
    }



    private Vector3 CellToWorld(Vector2Int cell) =>
        GameObject.Find($"Tile {cell.x},{cell.y}").GetComponent<RectTransform>().anchoredPosition;
        //transform.position + new Vector3(cell.x * cellSize, 0f, (rows - 1 - cell.y) * cellSize);
        
        

    private Vector2Int WorldToCell(Vector3 world) =>
        new Vector2Int(Mathf.RoundToInt((world.x - transform.position.x) / cellSize),
                       rows - 1 - Mathf.RoundToInt((world.z - transform.position.z) / cellSize));

    private bool InBounds(Vector2Int cell) => cell.x >= 0 && cell.x < columns && cell.y >= 0 && cell.y < rows;

    private bool PointerCell(out Vector2Int cell)
    {
        cell = default;
        Camera view = ViewCamera;
        if (view == null || Pointer.current == null) return false;
        // Ray ray = view.ScreenPointToRay(screen.mouseFollow.position);
        // if (!new Plane(Vector3.up, transform.position).Raycast(ray, out float distance)) return false;
        // cell = WorldToCell(ray.GetPoint(distance));
        // return true;

        if (!screen.mouseOver) return false;
        string name = screen.mouseOver.name;
        if (!name.Contains(',')) return false;
        int vecIndex = name.IndexOf(',');
        int x = name[vecIndex - 1];
        int y = name[vecIndex + 1];
        cell = new Vector2Int(x-48, y-48);
        Debug.Log(cell);
        return true;
    }

    
    //private Camera ViewCamera => Camera.main != null ? Camera.main : puzzleCamera;
    private Camera ViewCamera => puzzleCamera != null ? puzzleCamera : Camera.main;
    
    
    // public void HandleInput(InputAction.CallbackContext context)
    // {
    //     if (gameWon) return;
    //
    //     if (context.performed) BeginPath();
    //     else if (context.canceled) EndPath();
    // }

    public void Down()
    {
        if (!gameWon) BeginPath();
    }
    public void Up()
    {
        if (!gameWon) EndPath();
    }

    void Update()
    {
        if (!isDragging) return;
        if (PointerCell(out Vector2Int cell)) ExtendPath(cell);
    }

    private void BeginPath()
    {
        if (!PointerCell(out Vector2Int cell)) return;
        //if (cell != Vector2Int.zero) return; 

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

    public void GenerateRandomPuzzle()
    {
        exitPos = new Vector2Int(Random.Range(rows-3, rows+1), Random.Range(columns-3, columns+1));
        List<Vector2Int> path = randomWalkBoard(exitPos);
        Debug.Log("Path: " + string.Join(", ", path.Select(p => p.ToString())));

        string[] newBoard = new string[rows];
        string newRow = "";
        for(int r = 0; r < rows; r++)
        {  
           for(int c = 0; c < columns; c++)
            {
                int empty = Random.Range(0, 100);
                if(empty > 80)
                {
                    int symbol = Random.Range(0, 10);
                    if(symbol > 5)
                    {
                        switch(symbol)
                        {
                            case 6:
                                newRow+="+";
                                break;
                            case 7:
                                newRow+="-";
                                break;
                            case 8:
                                newRow+="*";
                                break;
                            case 9:
                                newRow+="/";
                                break;
                        }
                    }
                    else
                    {
                        int number = Random.Range(0, 9);
                        newRow += number.ToString();
                    }
                    
                }
                else
                {
                    newRow += ".";
                }
                newRow += " ";
            }
            newRow.TrimEnd();
            newBoard[r] = newRow;
            newRow = "";
        }
        puzzle = newBoard;
    }

    public List<Vector2Int> randomWalkBoard(Vector2Int exitPos)
    {
        List<Vector2Int> exploredPositions = new List<Vector2Int>();
        List<Vector2Int> Path = new List<Vector2Int>();
        Vector2Int curPos = new Vector2Int(0, 0);
        Vector2Int direction;
        float episilon = 0.1f;
        while(!curPos.Equals(exitPos))
        {
            exploredPositions.Add(curPos);
            float[] dirWeights = calcStepsWeight(curPos, exitPos, episilon);
            float dir = Random.value;
            if(dir < dirWeights[0])
            {
                direction = Vector2Int.up;
            }
            else if(dir < dirWeights[1])
            {
                direction = Vector2Int.down;
            }
            else if(dir < dirWeights[2])
            {
                direction = Vector2Int.left;
            }
            else
            {
                direction = Vector2Int.right;
            }
            if(exploredPositions.Contains(curPos + direction))
            {
                continue;
            }
            curPos += direction;
        }

        return exploredPositions;
    }

    private float[] calcStepsWeight(Vector2Int curPos, Vector2Int exitPos, float bias)
    {
        float[] dirWeights = {0, 0, 0, 0};
        float up;
        float down;
        float left;
        float right;
        Vector2 Direction = Vector2.Normalize(exitPos - curPos);
        up = 1 + bias * Vector2.Dot(new Vector2(0, 1), Direction);
        down = 1 + bias * Vector2.Dot(new Vector2(0, -1), Direction);
        left = 1 + bias * Vector2.Dot(new Vector2(0, -1), Direction);
        right = 1 + bias * Vector2.Dot(new Vector2(0, 1), Direction);
        dirWeights[0] = up / (up + down + left + right);
        dirWeights[1] = down / (up + down + left + right);
        dirWeights[2] = left / (up + down + left + right);
        dirWeights[3] = right / (up + down + left + right);
        
        return dirWeights;
    }
    // void findBoardAnswer()
    // {
    //     List<string> numbers = new List<string>();
    //     List<string> symbols = new List<string>();
    //     foreach(string rows in puzzle)
    //     {
    //         MatchCollection Nmatches = Regex.Matches(rows, @"\d+");
    //         string[] Rownumbers = Nmatches.Select(m => m.Value).ToArray();
    //         MatchCollection Smatches = Regex.Matches(rows, @"\s*[+\-*/%^]\s*");
    //         string[] RowSymbols =  Smatches.Select(m => m.Value).ToArray();
    //         foreach(string number in Rownumbers)
    //         {
    //             numbers.Add(number);
    //         }
    //         foreach(string symbol in RowSymbols)
    //         {
    //             symbols.Add(symbol);
    //         }
    //     }
        
    //     Debug.Log(string.Join(", ", numbers));
    //     Debug.Log(string.Join(", ", symbols));
    //     int minLength = Random.Range(2, numbers.Count+1);
    //     if(minLength > symbols.Count+1)
    //     {
    //         minLength = symbols.Count+1;
    //     }
    //     string equation = "";
    //     List<string> num = new List<string>();
    //     List<string> sym = new List<string>();
    //     num = numbers.OrderBy(n => Random.value).Take(minLength).ToList();
    //     sym = symbols.OrderBy(n => Random.value).Take(minLength-1).ToList();
    //     int nI = 0;
    //     int sI = 0;
    //     for(int i = 0; i < minLength*2-1; i++)
    //     {
    //         if(i % 2 == 0)
    //         {
    //             equation += num[nI];
    //             nI++;
    //         }
    //         else
    //         {
    //             equation += sym[sI];
    //             sI++;
    //         }
    //     }
    //     Debug.Log(equation);
    //     targetAnswer = Evaluate(equation);
    // }


}
