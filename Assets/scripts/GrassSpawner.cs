using System.Collections.Generic;
using UnityEngine;

public class GrassSpawner : CellSpawner
{
    [SerializeField] private GameObject grassPrefab;
    [SerializeField] private float yOffset;
    [SerializeField] private int sortingOrder = -50;
    [SerializeField] private Vector2 scaleRange = new Vector2(0.7f, 1.4f);
    [SerializeField] private Vector2 heightRange = new Vector2(0.7f, 1f);
    [SerializeField] private Vector2 widthRange = new Vector2(0.2f, 0.45f);
    [SerializeField] private Vector2 bendRange = new Vector2(0.1f, 0.5f);
    [SerializeField] private Vector2 windRange = new Vector2(0.06f, 0.18f);

    protected override GameObject Prefab => grassPrefab;

    protected override void Spawn(int cell, float step)
    {
        var bushes = new List<GameObject>(1);

        if (Hash(cell, 0) < density)
        {
            float x = (cell + Hash(cell, 1)) * step;
            float y = terrain.HeightAt(x);

            GameObject bush = Instantiate(
                grassPrefab,
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

            block.SetFloat("_BladeHeight", Lerp(heightRange, cell, 4));
            block.SetFloat("_BladeWidth", Lerp(widthRange, cell, 5));
            block.SetFloat("_BladeBend", Lerp(bendRange, cell, 6));
            block.SetFloat("_WindStrength", Lerp(windRange, cell, 7));

            block.SetFloat("_Seed", Mathf.Floor(Hash(cell, 8) * 9973f));

            renderer.SetPropertyBlock(block);

            bushes.Add(bush);
        }

        cells[cell] = bushes;
    }
}
