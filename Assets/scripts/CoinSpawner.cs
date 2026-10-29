using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : CellSpawner
{
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int sortingOrder;
    [SerializeField] private float heightAbove = 1f;
    [SerializeField] private Vector2Int groupCountRange = new Vector2Int(3, 8);
    [SerializeField] private float groupWidth = 1.5f;
    [SerializeField] private float arcHeight = 0.8f;

    protected override GameObject Prefab => coinPrefab;

    protected override void Spawn(int cell, float step)
    {
        var coins = new List<GameObject>();

        if (Hash(cell, 0) < density)
        {
            float cx = (cell + Hash(cell, 1)) * step;

            int count = Mathf.Max(1, Mathf.RoundToInt(Mathf.Lerp(groupCountRange.x, groupCountRange.y, Hash(cell, 2))));
            float peak = heightAbove + Mathf.Lerp(-arcHeight, arcHeight, Hash(cell, 3));

            for (int i = 0; i < count; i++)
            {
                float t = count <= 1 ? 0.5f : (float)i / (count - 1);
                float x = cx + (t - 0.5f) * groupWidth;
                float y = terrain.HeightAt(x) + peak + Mathf.Sin(t * Mathf.PI) * arcHeight;

                GameObject coin = Instantiate(
                    coinPrefab,
                    new Vector3(x, y, 0f),
                    Quaternion.identity,
                    transform
                );

                coin.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder + Mathf.RoundToInt(Hash(cell, 9 + i) * 2f);

                coins.Add(coin);
            }
        }

        cells[cell] = coins;
    }
}
