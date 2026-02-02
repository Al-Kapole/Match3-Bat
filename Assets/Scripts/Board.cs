using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour
{
    [SerializeField] private float swapDuration = 0.2f;
    [SerializeField] private float fallDuration = 0.15f;
    [SerializeField] private  float postMatchDelay = 0.1f;
    [SerializeField] private GameObject[] fruitPrefabs;
    [SerializeField] private GameObject[] tilemaps;

    public bool IsBusy { get; private set; }

    private int gridSize;
    private Tile[,] grid;
    private Vector3 gridOrigin;
    private float tileSpacing;
    private const int minSize = 6;
    private const int maxSize = 8;
    public void InitializeBoard(int size)
    {
        ClearBoard();
        if (size < minSize)
            size = minSize;
        else if (size > maxSize)
            size = maxSize;
        SelectBoardSize(size);

        gridSize = size;
        grid = new Tile[gridSize, gridSize];

        tileSpacing = fruitPrefabs[0].transform.localScale.x;//scale of tile object can define grid space also
        float gridWidth = (gridSize - 1) * tileSpacing;
        float gridHeight = (gridSize - 1) * tileSpacing;
        gridOrigin = new Vector3(-gridWidth / 2f, -gridHeight / 2f - 0.5f, 0f);


        for (int col = 0; col < gridSize; col++)
        {
            for (int row = 0; row < gridSize; row++)
                SpawnTile(col, row);
        }

        EnsureNoInitialMatches();
    }
    private void SelectBoardSize(int boardSize)
    {
        foreach (GameObject tilemap in tilemaps)
            tilemap.SetActive(false);
        tilemaps[boardSize - minSize].SetActive(true);
    }

    public void ClearBoard()
    {
        if (grid != null)
        {
            for (int col = 0; col < grid.GetLength(0); col++)
            {
                for (int row = 0; row < grid.GetLength(1); row++)
                {
                    if (grid[col, row] != null)
                        Destroy(grid[col, row].gameObject);
                }
            }
        }
        grid = null;
    }

    public Vector3 GridToWorld(int col, int row)
    {
        return gridOrigin + new Vector3(col * tileSpacing, row * tileSpacing, 0f);
    }
    public Tile GetTile(int col, int row)
    {
        if (col < 0 || col >= gridSize || row < 0 || row >= gridSize) 
            return null;
        return grid[col, row];
    }

    private Tile SpawnTile(int col, int row, int fruitType = -1)
    {
        if (fruitType < 0)
            fruitType = Random.Range(0, fruitPrefabs.Length);

        Vector3 pos = GridToWorld(col, row);
        GameObject fruit = Instantiate(fruitPrefabs[fruitType], pos, Quaternion.identity, transform);

        Tile tile = fruit.AddComponent<Tile>();
        tile.FruitType = fruitType;
        tile.Col = col;
        tile.Row = row;
        fruit.tag = Const.TAG_Tile;

        grid[col, row] = tile;
        return tile;
    }

    private void DestroyTile(int col, int row)
    {
        if (grid[col, row] != null)
        {
            Destroy(grid[col, row].gameObject);
            grid[col, row] = null;
        }
    }

    private void EnsureNoInitialMatches()
    {
        bool found = true;
        int maxIterations = 100;
        int iteration = 0;

        while (found && iteration < maxIterations)
        {
            found = false;
            iteration++;

            List<Vector2Int> matches = FindAllMatches();
            if (matches.Count > 0)
            {
                found = true;
                foreach (Vector2Int pos in matches)
                {
                    int col = pos.x;
                    int row = pos.y;
                    int oldType = grid[col, row].FruitType;
                    Destroy(grid[col, row].gameObject);

                    // Pick a different type
                    int newType = (oldType + Random.Range(1, fruitPrefabs.Length)) % fruitPrefabs.Length;
                    grid[col, row] = null;
                    SpawnTile(col, row, newType);
                }
            }
        }
    }

    public void TrySwap(int col1, int row1, int col2, int row2)
    {
        if (IsBusy) return;
        if (col2 < 0 || col2 >= gridSize || row2 < 0 || row2 >= gridSize) return;
        if (grid[col1, row1] == null || grid[col2, row2] == null) return;

        StartCoroutine(SwapCoroutine(col1, row1, col2, row2));
    }

    private IEnumerator SwapCoroutine(int c1, int r1, int c2, int r2)
    {
        IsBusy = true;

        Tile tileA = grid[c1, r1];
        Tile tileB = grid[c2, r2];

        Vector3 posA = GridToWorld(c1, r1);
        Vector3 posB = GridToWorld(c2, r2);

        // Animate swap
        Coroutine moveA = tileA.MoveTo(posB, swapDuration);
        Coroutine moveB = tileB.MoveTo(posA, swapDuration);
        yield return moveA;
        yield return moveB;

        // Swap in grid
        grid[c1, r1] = tileB;
        grid[c2, r2] = tileA;
        tileA.Col = c2; tileA.Row = r2;
        tileB.Col = c1; tileB.Row = r1;

        // Check for matches
        List<Vector2Int> matches = FindAllMatches();
        if (matches.Count == 0)
        {
            // No match — swap back
            moveA = tileA.MoveTo(posA, swapDuration);
            moveB = tileB.MoveTo(posB, swapDuration);
            yield return moveA;
            yield return moveB;

            grid[c1, r1] = tileA;
            grid[c2, r2] = tileB;
            tileA.Col = c1; tileA.Row = r1;
            tileB.Col = c2; tileB.Row = r2;

            IsBusy = false;
            yield break;
        }

        // Valid match — run cascade
        yield return StartCoroutine(CascadeLoop());
    }

    public List<Vector2Int> FindAllMatches()
    {
        HashSet<Vector2Int> matched = new HashSet<Vector2Int>();

        // Horizontal
        for (int row = 0; row < gridSize; row++)
        {
            int col = 0;
            while (col < gridSize)
            {
                if (grid[col, row] == null) { col++; continue; }

                int type = grid[col, row].FruitType;
                int runStart = col;
                while (col < gridSize && grid[col, row] != null && grid[col, row].FruitType == type)
                {
                    col++;
                }
                int runLength = col - runStart;
                if (runLength >= 3)
                {
                    for (int i = runStart; i < col; i++)
                        matched.Add(new Vector2Int(i, row));
                }
            }
        }

        // Vertical
        for (int col = 0; col < gridSize; col++)
        {
            int row = 0;
            while (row < gridSize)
            {
                if (grid[col, row] == null) { row++; continue; }

                int type = grid[col, row].FruitType;
                int runStart = row;
                while (row < gridSize && grid[col, row] != null && grid[col, row].FruitType == type)
                {
                    row++;
                }
                int runLength = row - runStart;
                if (runLength >= 3)
                {
                    for (int i = runStart; i < row; i++)
                        matched.Add(new Vector2Int(col, i));
                }
            }
        }

        return new List<Vector2Int>(matched);
    }

    private IEnumerator CascadeLoop()
    {
        int chainMultiplier = 1;

        while (true)
        {
            List<Vector2Int> matches = FindAllMatches();
            if (matches.Count == 0) break;

            // Score
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChainMultiplier = chainMultiplier;
                GameManager.Instance.AddScore(matches.Count);
            }
            chainMultiplier++;

            // Destroy matched tiles
            foreach (Vector2Int pos in matches)
                DestroyTile(pos.x, pos.y);
            

            yield return new WaitForSeconds(postMatchDelay);

            // Collapse columns and spawn new tiles
            float maxFallDuration = 0f;

            for (int col = 0; col < gridSize; col++)
            {
                int writeRow = 0;
                for (int readRow = 0; readRow < gridSize; readRow++)
                {
                    if (grid[col, readRow] != null)
                    {
                        if (readRow != writeRow)
                        {
                            Tile tile = grid[col, readRow];
                            grid[col, writeRow] = tile;
                            grid[col, readRow] = null;
                            tile.Row = writeRow;
                            float dist = (readRow - writeRow) * tileSpacing;
                            float duration = fallDuration + dist * 0.02f;
                            if (duration > maxFallDuration) maxFallDuration = duration;
                            tile.MoveTo(GridToWorld(col, writeRow), duration);
                        }
                        writeRow++;
                    }
                }

                // Spawn new tiles for empty slots
                int emptyCount = gridSize - writeRow;
                for (int i = 0; i < emptyCount; i++)
                {
                    int row = writeRow + i;
                    Tile t = SpawnTile(col, row);
                    // Start above the visible grid
                    Vector3 spawnPos = GridToWorld(col, gridSize + i);
                    t.transform.position = spawnPos;
                    float dist = (gridSize + i - row) * tileSpacing;
                    float duration = fallDuration + dist * 0.02f;
                    if (duration > maxFallDuration) maxFallDuration = duration;
                    t.MoveTo(GridToWorld(col, row), duration);
                }
            }

            yield return new WaitForSeconds(maxFallDuration + 0.05f);
        }

        IsBusy = false;
    }
}
