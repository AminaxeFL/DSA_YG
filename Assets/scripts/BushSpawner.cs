using System.Collections.Generic;
using UnityEngine;

public class BushSpawner : CellSpawner
{
    [SerializeField] private GameObject bushPrefab;
    [SerializeField] private float yOffset;
    [SerializeField] private int sortingOrder;
    [SerializeField] private Vector2 scaleRange = new Vector2(0.7f, 1.5f);
    [SerializeField] private Vector2 heightRange = new Vector2(0.75f, 1f);
    [SerializeField] private Vector2 radiusRange = new Vector2(0.8f, 1.15f);
    [SerializeField] private Vector2 lumpsRange = new Vector2(0.1f, 0.45f);
    [SerializeField] private Vector2 lumpFreqRange = new Vector2(1.5f, 4f);

    protected override GameObject Prefab => bushPrefab;

    protected override void Spawn(int cell, float step)
    {
        var bushes = new List<GameObject>(1);

        if (Hash(cell, 0) < density)
        {
            float x = (cell + Hash(cell, 1)) * step;
            float y = terrain.HeightAt(x);

            GameObject bush = Instantiate(
                bushPrefab,
                new Vector3(x, y + yOffset, 0f),
                Quaternion.identity,
                transform
            );

            float scale = Mathf.Lerp(scaleRange.x, scaleRange.y, Hash(cell, 2));

            bush.transform.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer renderer = bush.GetComponent<SpriteRenderer>();

            if (Hash(cell, 3) < 0.5f)
                renderer.flipX = !renderer.flipX;

            renderer.sortingOrder = sortingOrder + Mathf.RoundToInt(Hash(cell, 9) * 2f);

            var block = new MaterialPropertyBlock();

            block.SetFloat("_BushHeight", Lerp(heightRange, cell, 4));
            block.SetFloat("_BushRadius", Lerp(radiusRange, cell, 5));
            block.SetFloat("_BushLumps", Lerp(lumpsRange, cell, 6));
            block.SetFloat("_BushLumpFreq", Lerp(lumpFreqRange, cell, 7));

            block.SetFloat("_Seed", Mathf.Floor(Hash(cell, 8) * 9973f));

            renderer.SetPropertyBlock(block);

            bushes.Add(bush);
        }

        cells[cell] = bushes;
    }
}
