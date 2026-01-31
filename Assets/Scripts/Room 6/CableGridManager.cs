using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CableGridManager : MonoBehaviour
{
    public static CableGridManager Instance { get; private set; }

    [SerializeField] private float spacing = 1.1f;
    public MyDoorController door;

    [SerializeField] private bool lockOffPathTiles = true;
    [SerializeField] private float blinkDuration = 0.2f;
    [SerializeField] private int blinkCount = 2;

    private CableTile[,] grid;
    private int width, height;

    private CableTile sourceTile;
    private CableTile targetTile;

    private Dictionary<Vector2Int, int> requiredMask;

    private bool solved;
    private bool busy;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        BuildGridFromChildren();
        SetupRequiredMask_4x4();
        ApplyInitialVisuals();
        CheckSolvedAndMaybeBlink();
    }

    void BuildGridFromChildren()
    {
        var tiles = GetComponentsInChildren<CableTile>(false);

        float minX = float.PositiveInfinity;
        float minY = float.PositiveInfinity;

        foreach (var t in tiles)
        {
            var p = t.transform.localPosition;
            minX = Mathf.Min(minX, p.x);
            minY = Mathf.Min(minY, p.y);
        }

        int maxGX = 0, maxGY = 0;
        var coords = new Dictionary<CableTile, Vector2Int>();

        foreach (var t in tiles)
        {
            var p = t.transform.localPosition;
            int gx = Mathf.RoundToInt((p.x - minX) / Mathf.Max(0.0001f, spacing));
            int gy = Mathf.RoundToInt((p.y - minY) / Mathf.Max(0.0001f, spacing));
            coords[t] = new Vector2Int(gx, gy);
            maxGX = Mathf.Max(maxGX, gx);
            maxGY = Mathf.Max(maxGY, gy);
        }

        width = maxGX + 1;
        height = maxGY + 1;
        grid = new CableTile[width, height];

        foreach (var kv in coords)
        {
            var t = kv.Key;
            var p = kv.Value;
            t.Init(p.x, p.y);
            grid[p.x, p.y] = t;
        }

        FindEndpoints();
    }

    void FindEndpoints()
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
    }

    void SetupRequiredMask_4x4()
    {
        requiredMask = new Dictionary<Vector2Int, int>
        {
            [new Vector2Int(0, 0)] = 2,
            [new Vector2Int(1, 0)] = 10,
            [new Vector2Int(2, 0)] = 9,
            [new Vector2Int(2, 1)] = 5,
            [new Vector2Int(2, 2)] = 12,
            [new Vector2Int(1, 2)] = 10,
            [new Vector2Int(0, 2)] = 3,
            [new Vector2Int(0, 3)] = 6,
            [new Vector2Int(1, 3)] = 10,
            [new Vector2Int(2, 3)] = 10,
            [new Vector2Int(3, 3)] = 8
        };

        solved = false;
        busy = false;
    }

    void ApplyInitialVisuals()
    {
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var t = grid[x, y];
                if (!t) continue;

                t.ShowBase();
                t.RefreshHintFromCurrentMask();

                bool onPath = requiredMask.ContainsKey(new Vector2Int(x, y));

                if (lockOffPathTiles && !onPath)
                {
                    t.LockRotation = true;
                    t.BaseMask = 0;
                    t.RefreshHintFromCurrentMask();
                }
                else
                {
                    t.LockRotation = t.IsSource || t.IsTarget;
                }
            }

        RandomizeAllTiles();
        door?.Lock();
    }

    void RandomizeAllTiles()
    {
        foreach (var kv in requiredMask)
        {
            var p = kv.Key;
            var t = grid[p.x, p.y];
            if (!t || t.IsSource || t.IsTarget) continue;

            int r = Random.Range(0, 4);
            t.ForceSetRotationSteps(r);
        }
    }

    public void OnTileRotated(CableTile tile)
    {
        if (busy || solved) return;
        CheckSolvedAndMaybeBlink();
    }

    void CheckSolvedAndMaybeBlink()
    {
        foreach (var kv in requiredMask)
        {
            var p = kv.Key;
            var t = grid[p.x, p.y];
            if (!t || t.CurrentMask != kv.Value) return;
        }

        solved = true;
        door?.Unlock();
        StartCoroutine(CoBlinkAllGreen());
    }

    IEnumerator CoBlinkAllGreen()
    {
        busy = true;

        for (int i = 0; i < blinkCount; i++)
        {
            foreach (var t in grid)
                if (t) t.BlinkGreen();

            yield return new WaitForSeconds(blinkDuration);

            foreach (var t in grid)
                if (t) t.ShowBase();

            yield return new WaitForSeconds(blinkDuration);
        }

        busy = false;
    }

    public void RegisterInsertedTile(CableTile tile, int gx, int gy)
    {
        if (grid == null || tile == null) return;
        if (gx < 0 || gx >= width || gy < 0 || gy >= height) return;

        tile.gx = gx;
        tile.gy = gy;
        grid[gx, gy] = tile;

        ApplyInitialVisuals();
        solved = false;
        CheckSolvedAndMaybeBlink();
    }

    public void RegisterRemovedTile(int gx, int gy)
    {
        if (grid == null) return;
        if (gx < 0 || gx >= width || gy < 0 || gy >= height) return;

        grid[gx, gy] = null;

        ApplyInitialVisuals();
        solved = false;
    }
}
