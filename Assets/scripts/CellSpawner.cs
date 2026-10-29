using System.Collections.Generic;
using UnityEngine;

public abstract class CellSpawner : MonoBehaviour
{
    [SerializeField] protected TerrainGenerator terrain;
    [SerializeField] protected float cellSize = 2.5f;
    [SerializeField] protected float density = 0.55f;
    [SerializeField] protected float viewRadius = 22f;
    [SerializeField] protected int seed = 12345;

    protected readonly Dictionary<int, List<GameObject>> cells = new Dictionary<int, List<GameObject>>();

    private readonly List<int> staleCells = new List<int>();
    private Transform follow;

    protected abstract GameObject Prefab { get; }
    protected abstract void Spawn(int cell, float step);

    protected virtual void Awake()
    {
        terrain ??= FindAnyObjectByType<TerrainGenerator>();

        follow = FindAnyObjectByType<WheelSpinner>()?.transform;

        if (!terrain || !Prefab)
            enabled = false;
    }

    protected virtual void Update()
    {
        float center = follow ? follow.position.x : transform.position.x;
        float step = Mathf.Max(cellSize, 0.1f);

        int first = Mathf.FloorToInt((center - viewRadius) / step);
        int last = Mathf.FloorToInt((center + viewRadius) / step);

        for (int i = first; i <= last; i++)
        {
            if (!cells.ContainsKey(i))
                Spawn(i, step);
        }

        DropStale(first, last);
    }

    protected float Hash(int cell, int salt)
    {
        unchecked
        {
            int n = cell * 374761393 + (seed + salt * 6151) * 668265263;

            n = (n ^ (n >> 13)) * 1274126177;

            return (n & 0x7fffffff) / (float)0x7fffffff;
        }
    }

    protected float Lerp(Vector2 range, int cell, int salt)
    {
        return Mathf.Lerp(range.x, range.y, Hash(cell, salt));
    }

    private void DropStale(int first, int last)
    {
        staleCells.Clear();

        foreach (KeyValuePair<int, List<GameObject>> pair in cells)
        {
            if (pair.Key < first - 1 || pair.Key > last + 1)
                staleCells.Add(pair.Key);
        }

        for (int i = 0; i < staleCells.Count; i++)
        {
            int cell = staleCells[i];

            if (!cells.TryGetValue(cell, out List<GameObject> spawned))
                continue;

            for (int j = 0; j < spawned.Count; j++)
            {
                if (spawned[j])
                    Destroy(spawned[j]);
            }

            cells.Remove(cell);
        }
    }
}
