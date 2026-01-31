using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CableGridManager : MonoBehaviour
{
    public static CableGridManager Instance { get; private set; }

    [Header("Grid Settings")]
    [SerializeField] private float spacing = 1.1f;

    [Header("Door (existing logic)")]
    public MyDoorController door;

    [Header("Timing")]
    [SerializeField] private float correctDelaySeconds = 0.20f;
    [SerializeField] private float wrongFlashSeconds = 0.50f;

    [Header("Behavior")]
    [SerializeField] private bool lockOffPathTiles = true;

    private CableTile[,] grid;
    private int width, height;

    private CableTile sourceTile;
    private CableTile targetTile;

    private List<Vector2Int> path;
    private Dictionary<Vector2Int, int> requiredMask;

    private int currentStepIndex = 1;
    private bool busy;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        BuildGridFromChildren();
        if (grid == null) return;

        SetupPath_4x4();
        ApplyInitialVisuals();
    }


    private void BuildGridFromChildren()
    {
        var tiles = GetComponentsInChildren<CableTile>(includeInactive: false);
        if (tiles == null || tiles.Length == 0)
        {
            Debug.LogError("CableGridManager: No CableTile found as children.");
            return;
        }

        float minX = float.PositiveInfinity;
        float minY = float.PositiveInfinity;

        foreach (var t in tiles)
        {
            var p = t.transform.localPosition;
            if (p.x < minX) minX = p.x;
            if (p.y < minY) minY = p.y;
        }

        int maxGX = 0, maxGY = 0;
        var coords = new Dictionary<CableTile, (int gx, int gy)>();

        foreach (var t in tiles)
        {
            var p = t.transform.localPosition;
            float fx = (p.x - minX) / Mathf.Max(0.0001f, spacing);
            float fy = (p.y - minY) / Mathf.Max(0.0001f, spacing);

            int gx = Mathf.RoundToInt(fx);
            int gy = Mathf.RoundToInt(fy);

            coords[t] = (gx, gy);
            if (gx > maxGX) maxGX = gx;
            if (gy > maxGY) maxGY = gy;
        }

        width = maxGX + 1;
        height = maxGY + 1;
        grid = new CableTile[width, height];

        foreach (var kv in coords)
        {
            var tile = kv.Key;
            var (gx, gy) = kv.Value;

            if (gx < 0 || gx >= width || gy < 0 || gy >= height)
                continue;

            if (grid[gx, gy] != null)
            {
                Debug.LogError($"CableGridManager: Duplicate cell ({gx},{gy}) for {tile.name} and {grid[gx, gy].name}");
                continue;
            }

            tile.Init(gx, gy);
            grid[gx, gy] = tile;
        }

        FindEndpoints();
        Debug.Log($"CableGridManager: grid created {width}x{height}");
    }

    private void FindEndpoints()
    {
        sourceTile = null;
        targetTile = null;

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var t = grid[x, y];
                if (!t) continue;

                if (t.IsSource) sourceTile = t;
                if (t.IsTarget) targetTile = t;
            }

        if (!sourceTile) Debug.LogError("CableGridManager: Missing SOURCE tile (IsSource=true).");
        if (!targetTile) Debug.LogError("CableGridManager: Missing TARGET tile (IsTarget=true).");
    }


    private void SetupPath_4x4()
    {
        path = new List<Vector2Int>
        {
            new Vector2Int(0,0),
            new Vector2Int(1,0),
            new Vector2Int(2,0),
            new Vector2Int(2,1),
            new Vector2Int(2,2),
            new Vector2Int(1,2),
            new Vector2Int(0,2),
            new Vector2Int(0,3),
            new Vector2Int(1,3),
            new Vector2Int(2,3),
            new Vector2Int(3,3)
        };

        requiredMask = new Dictionary<Vector2Int, int>
        {
            [new Vector2Int(0, 0)] = 2,   // E
            [new Vector2Int(1, 0)] = 10,  // W+E
            [new Vector2Int(2, 0)] = 9,   // W+N
            [new Vector2Int(2, 1)] = 5,   // N+S
            [new Vector2Int(2, 2)] = 12,  // S+W
            [new Vector2Int(1, 2)] = 10,  // W+E
            [new Vector2Int(0, 2)] = 3,   // N+E  
            [new Vector2Int(0, 3)] = 6,   // S+E
            [new Vector2Int(1, 3)] = 10,  // W+E
            [new Vector2Int(2, 3)] = 10,  // W+E
            [new Vector2Int(3, 3)] = 8    // W
        };

        currentStepIndex = 1;
        busy = false;
    }


    private void ApplyInitialVisuals()
    {
        if (grid == null) return;
        if (!sourceTile || !targetTile) FindEndpoints();
        if (!sourceTile || !targetTile) return;

        busy = false;
        currentStepIndex = 1;

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var t = grid[x, y];
                if (!t) continue;

                t.ShowUnpowered();

                Vector2Int pos = new Vector2Int(x, y);
                bool onPath = requiredMask.ContainsKey(pos);

                if (lockOffPathTiles && !onPath)
                {
                    t.LockRotation = true;     
                    t.BaseMask = 0;           
                }
                else
                {
                    if (t.IsSource || t.IsTarget) t.LockRotation = true;
                    else t.LockRotation = false;
                }
            }

        sourceTile.ShowPowered();

        door?.Lock();
    }

    public void OnTileRotated(CableTile tile)
    {
        if (busy) return;
        if (!tile) return;
        if (path == null || requiredMask == null) return;
        if (currentStepIndex < 1 || currentStepIndex >= path.Count) return;

        Vector2Int expected = path[currentStepIndex];

        if (!IsInside(expected) || grid[expected.x, expected.y] == null) return;

        Vector2Int pos = new Vector2Int(tile.gx, tile.gy);

        if (pos != expected) return;

        int need = requiredMask[expected];
        int cur = tile.CurrentMask;

        if (cur == need) StartCoroutine(CoCorrect(tile));
        else StartCoroutine(CoWrong(tile));
    }

    private IEnumerator CoCorrect(CableTile tile)
    {
        busy = true;

        tile.ShowPowered();
        tile.LockRotation = true; 

        yield return new WaitForSeconds(correctDelaySeconds);

        currentStepIndex++;

        if (currentStepIndex == path.Count - 1)
        {
            Vector2Int tp = path[currentStepIndex];
            if (IsInside(tp) && grid[tp.x, tp.y] != null)
                grid[tp.x, tp.y].ShowPowered();

            door?.Unlock();
        }

        busy = false;
    }

    private IEnumerator CoWrong(CableTile tile)
    {
        busy = true;

        tile.ShowWrong();
        yield return new WaitForSeconds(wrongFlashSeconds);
        tile.ShowUnpowered();

        busy = false;
    }

    private bool IsInside(Vector2Int p)
    {
        return p.x >= 0 && p.x < width && p.y >= 0 && p.y < height;
    }

    public void RegisterInsertedTile(CableTile tile, int gx, int gy)
    {
        if (grid == null || !tile) return;
        if (gx < 0 || gx >= width || gy < 0 || gy >= height) return;

        tile.gx = gx;
        tile.gy = gy;

        grid[gx, gy] = tile;

        ApplyInitialVisuals();
    }

    public void RegisterRemovedTile(int gx, int gy)
    {
        if (grid == null) return;
        if (gx < 0 || gx >= width || gy < 0 || gy >= height) return;

        grid[gx, gy] = null;

        ApplyInitialVisuals();
    }
}
