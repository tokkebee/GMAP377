using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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
    private bool isDragging;
    public bool gameWon {get; private set;}
    
    [SerializeField] private ScreenScript screen;
    [SerializeField] private Canvas screenCanvas;
    private List<GameObject> tiles = new List<GameObject>();

    public static Action puzzleCompleted;
 

    void Awake()
    {
        line = screenCanvas != null ? screenCanvas.GetComponent<LineRenderer>() : GetComponent<LineRenderer>();
        if (line == null) line = gameObject.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.startWidth = line.endWidth = cellSize * 0.005f;
        if (line.sharedMaterial == null)
        {
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit");
            if (shader != null) line.material = new Material(shader);
        }
        puzzle = new string[rows];
    
        GenerateRandomPuzzle();
        
        BuildBoard();
        BuildTiles();
        UpdateHud();
    }

    public void restart()
    {
        puzzle = new string[rows];
        UpdateHud();
        ResetPath(); 
        gameWon = false;
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
        foreach(GameObject tile in tiles)
        {
            Destroy(tile);
        }
        Vector2 centerOffSet = new Vector2();
        centerOffSet.x = 500 - cellSize * columns;
        centerOffSet.y = 500 - cellSize * rows;
        for (int c = 0; c < columns; c++)
            for (int r = 0; r < rows; r++)
            {
                var tile = new GameObject($"Tile {c},{r}", typeof(RectTransform));
                tiles.Add(tile);
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

    
    private Camera ViewCamera => puzzleCamera != null ? puzzleCamera : Camera.main;
    

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
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            restart();
        }
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
        if (seen >= 0)
        {
            path.RemoveRange(seen + 1, path.Count - seen - 1);
            RebuildEquation();
            DrawLine();
            UpdateHud();
            return;
        }

        if (Mathf.Abs(cell.x - last.x) + Mathf.Abs(cell.y - last.y) != 1) return;

        string value = board[cell.x, cell.y];
        if (value != " " && IsOperator(value) && EndsWithOperator()) return;

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
            puzzleCompleted?.Invoke();
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
        equation = "";
        foreach (Vector2Int cell in path)
        {
            string value = board[cell.x, cell.y];
            if (value == " ") continue;

            bool digitsTouch = equation.Length > 0
                && char.IsDigit(equation[equation.Length - 1])
                && value.Length == 1 && char.IsDigit(value[0]);

            if (equation.Length > 0 && !digitsTouch) equation += " ";
            equation += value;
        }
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

        float value = 0f;
        bool haveValue = false;
        string pendingOp = "";
        string number = "";

        void Apply()
        {
            if (number == "") return;

            float operand = float.Parse(number);
            number = "";

            if (!haveValue) { value = operand; haveValue = true; pendingOp = ""; return; }

            switch (pendingOp)
            {
                case "+": value += operand; break;
                case "-": value -= operand; break;
                case "*": value *= operand; break;
                case "/": value /= operand; break;
            }
            pendingOp = "";
        }

        foreach (string part in expression.Split(' '))
        {
            if (IsOperator(part)) { Apply(); pendingOp = part; continue; }
            if (!float.TryParse(part, out _)) continue;
            number += part;
        }
        Apply();

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
        int minX = Mathf.Min(columns - 1, Mathf.Max(1, columns - 3));
        int minY = Mathf.Min(rows - 1, Mathf.Max(1, rows - 3));
        exitPos = new Vector2Int(Random.Range(minX, columns), Random.Range(minY, rows));
        List<Vector2Int> path = DFSWalkBoard(exitPos);
        Debug.Log("Path: " + string.Join(", ", path.Select(p => p.ToString())));
        int i = 0;
        char[,] newBoard = new char[columns, rows];
        foreach(Vector2Int cell in path)
        {
            int empty = Random.Range(60, 100);
            if(empty >= 80)
            {
                if(i % 2 == 0)
                {
                    int number = Random.Range(1, 9);
                    newBoard[cell.x, cell.y] = (char) (number + '0');
                }
                else
                {
                    int symbol = Random.Range(6, 10);
                    if(symbol > 5)
                    {
                        switch(symbol)
                        {
                            case 6:
                                newBoard[cell.x, cell.y] = '+';
                                break;
                            case 7:
                                newBoard[cell.x, cell.y] = '-';
                                break;
                            case 8:
                                newBoard[cell.x, cell.y] = '*';
                                break;
                            case 9:
                                newBoard[cell.x, cell.y] = '/';
                                break;
                        }
                    }
                }
                i++;
            }
            else newBoard[cell.x, cell.y] = '.';
        }
        string EquationOf() => string.Join(" ", path
            .Select(c => newBoard[c.x, c.y])
            .Where(ch => ch != '.' && ch != '\0'));

        newBoard[path[path.Count-1].x, path[path.Count-1].y] = '.';
        path.RemoveAt(path.Count - 1);

        string puzzleEquation = EquationOf();
        while (puzzleEquation.Length > 0 && !char.IsDigit(puzzleEquation[puzzleEquation.Length - 1]))
        {
            newBoard[path[path.Count-1].x, path[path.Count-1].y] = '.';
            path.RemoveAt(path.Count - 1);
            puzzleEquation = EquationOf();
        }
        if (puzzleEquation.Length == 0 && path.Count > 0)
        {
            newBoard[path[0].x, path[0].y] = (char)('1' + Random.Range(0, 9));
            puzzleEquation = EquationOf();
        }
        targetAnswer = Evaluate(puzzleEquation);
        for(int r = 0; r < rows; r++)
        {  
            for(int c = 0; c < columns; c++)
            {
                int empty;
                
                if(newBoard[c, r] == '\0')
                {
                    empty = Random.Range(0, 100);
                    if(empty > 80)
                    {
                        int symbol = Random.Range(0, 10);
                        if(symbol > 5)
                        {
                            switch(symbol)
                            {
                                case 6:
                                    newBoard[c, r] = '+';
                                    break;
                                case 7:
                                    newBoard[c, r] = '-';
                                    break;
                                case 8:
                                    newBoard[c, r] = '*';
                                    break;
                                case 9:
                                    newBoard[c, r] = '/';
                                    break;
                            }
                        }
                        else
                        {
                            int number = Random.Range(1, 9);
                            newBoard[c, r] = (char) (number + '0');
                        }
                    }
                    else
                        newBoard[c, r] = '.';
                }
                
            }
        }
        
        puzzle = new string[rows];
        for(int r = 0; r < rows; r++)
            puzzle[r] = string.Join(" ", Enumerable.Range(0, columns)
                       .Select(colIndex => newBoard[colIndex, r])
                       .ToArray());
        
    }

    public List<Vector2Int> DFSWalkBoard(Vector2Int exitPos)
    {
        HashSet<Vector2Int> exploredPositions = new HashSet<Vector2Int>();
        Stack<Vector2Int> Path = new Stack<Vector2Int>();
        Vector2Int curPos = new Vector2Int(0, 0);
        Path.Push(curPos);
        exploredPositions.Add(curPos);

        int guard = 4 * rows * columns + 16;
        while(Path.Count > 0 && guard-- > 0)
        {
            List<Vector2Int> possibleMoves = new List<Vector2Int>();
            if((curPos + Vector2Int.up).y < rows && !exploredPositions.Contains(curPos + Vector2Int.up))
            {
                possibleMoves.Add(curPos + Vector2Int.up);
            }
            if((curPos + Vector2Int.down).y >= 0 && !exploredPositions.Contains(curPos + Vector2Int.down))
            {
                possibleMoves.Add(curPos + Vector2Int.down);
            }
            if((curPos + Vector2Int.left).x >= 0 && !exploredPositions.Contains(curPos + Vector2Int.left))
            {
                possibleMoves.Add(curPos + Vector2Int.left);
            }
            if((curPos + Vector2Int.right).x < columns && !exploredPositions.Contains(curPos + Vector2Int.right))
            {
                possibleMoves.Add(curPos + Vector2Int.right);
            }
            if(possibleMoves.Count == 0)
            {
                Path.Pop();
                if (Path.Count == 0) break;
                curPos = Path.Peek();
                continue;
            }
            curPos = possibleMoves[Random.Range(0, possibleMoves.Count)];
            exploredPositions.Add(curPos);
            Path.Push(curPos);
            if (curPos == exitPos) break;

            
        }
        List<Vector2Int> p = Path.ToList();
        p.Reverse();
        return p;
    }

    

}
