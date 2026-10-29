using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{

    [SerializeField] private float heightOffset = 0f;
    [SerializeField] private int seed = 12345;
    [SerializeField] private float chunkSize = 8f;
    [Range(1, 12)] [SerializeField] private int viewDistance = 3;
    [SerializeField] private float flatStartRadius = 6f;
    [SerializeField] private float sampleStep = 0.025f;

    [SerializeField] private float hillHeight = 0.55f;
    [SerializeField] private float hillLength = 9f;
    [Range(1, 8)] [SerializeField] private int octaves = 3;
    [Range(0f, 1f)] [SerializeField] private float octaveGain = 0.5f;
    [Min(0f)] [SerializeField] private float flatHeight = 0.07f;
    [Min(1f)] [SerializeField] private float reliefLength = 60f;
    [Range(0.1f, 4f)] [SerializeField] private float reliefContrast = 1.2f;

    [SerializeField] private Color surfaceColor = new Color(0.36f, 0.42f, 0.22f);
    [SerializeField] private Color soilColor = new Color(0.33f, 0.24f, 0.15f);
    [SerializeField] private Color deepColor = new Color(0.13f, 0.10f, 0.07f);
    [SerializeField] private float surfaceThickness = 0.18f;
    [SerializeField] private float depthBelow = 10f;
    [SerializeField] private int sortingOrder = -100;
    [SerializeField] private string groundLayer = "Ground";
    [SerializeField] private Texture2D groundTexture;
    [SerializeField] private float textureTileSize = 1.5f;
    [SerializeField] private Material terrainMaterial;

    private readonly Dictionary<int, Chunk> chunks = new Dictionary<int, Chunk>();

    private Material sharedMaterial;

    private Transform follow;

    private class Chunk
    {
        public GameObject root;
        public Mesh mesh;
    }

    private void Awake()
    {
        sharedMaterial = BuildMaterial();

        WheelSpinner car = FindAnyObjectByType<WheelSpinner>();

        if (car)
            follow = car.transform;
    }

    private void Update()
    {
        float center = follow ? follow.position.x : transform.position.x;

        int needed = Mathf.Max(1, Mathf.CeilToInt(chunkSize / Mathf.Max(sampleStep, 0.001f)));

        int index = Mathf.FloorToInt(center / chunkSize);

        for (int i = index - viewDistance; i <= index + viewDistance; i++)
        {
            if (!chunks.ContainsKey(i))
                BuildChunk(i, needed);
        }

        if (chunks.Count > viewDistance * 2 + 3)
        {
            List<int> stale = null;

            foreach (KeyValuePair<int, Chunk> pair in chunks)
            {
                if (Mathf.Abs(pair.Key - index) > viewDistance)
                {
                    stale ??= new List<int>();

                    stale.Add(pair.Key);
                }
            }

            if (stale?.Count > 0)
            {
                for (int i = 0; i < stale.Count; i++)
                    DestroyChunk(stale[i]);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (KeyValuePair<int, Chunk> pair in chunks)
        {
            if (pair.Value.mesh)
                Destroy(pair.Value.mesh);
        }

        chunks.Clear();

        if (sharedMaterial)
            Destroy(sharedMaterial);
    }

    public float HeightAt(float x)
    {
        float h = 0f;
        float amplitude = 1f;
        float total = 0f;

        float frequency = 1f / Mathf.Max(hillLength, 0.001f);

        for (int i = 0; i < octaves; i++)
        {
            h += (ValueNoise(x * frequency) - 0.5f) * 2f * amplitude;

            total += amplitude;

            amplitude *= octaveGain;
            frequency *= 2f;
        }

        if (total > 0f)
            h /= total;

        h *= LocalAmplitude(x);

        float flat = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(Mathf.Abs(x) / Mathf.Max(flatStartRadius, 0.001f)));

        return h * flat + heightOffset;
    }

    private float LocalAmplitude(float x)
    {
        float t = Mathf.Clamp01(ValueNoise(x / Mathf.Max(reliefLength, 1f) + 137f));

        t = Mathf.Pow(t, reliefContrast);

        return Mathf.Lerp(flatHeight, hillHeight, t);
    }

    private float ValueNoise(float x)
    {
        int cell = Mathf.FloorToInt(x);

        float t = x - cell;

        float u = t * t * t * (t * (t * 6f - 15f) + 10f);

        return Mathf.Lerp(Hash(cell), Hash(cell + 1), u);
    }

    private float Hash(int cell)
    {
        unchecked
        {
            int n = cell * 374761393 + seed * 668265263;

            n = (n ^ (n >> 13)) * 1274126177;

            return (n & 0x7fffffff) / (float)0x7fffffff;
        }
    }

    private void BuildChunk(int index, int segments)
    {
        float originX = index * chunkSize;

        GameObject root = new GameObject("Terrain_" + index);

        root.transform.SetParent(transform, false);
        root.transform.position = new Vector3(originX, 0f, 0f);

        int layer = LayerMask.NameToLayer(groundLayer);

        if (layer >= 0)
            root.layer = layer;

        Vector2[] points = new Vector2[segments + 1];

        float lowest = float.MaxValue;

        for (int i = 0; i <= segments; i++)
        {
            float localX = i * chunkSize / segments;

            float worldX = originX + localX;

            float y = HeightAt(worldX);

            points[i] = new Vector2(localX, y);

            if (y < lowest)
                lowest = y;
        }

        EdgeCollider2D edge = root.AddComponent<EdgeCollider2D>();

        edge.points = points;

        Mesh mesh = BuildMesh(points, lowest);

        GameObject visual = new GameObject("Visual");

        visual.transform.SetParent(root.transform, false);

        MeshFilter filter = visual.AddComponent<MeshFilter>();

        filter.sharedMesh = mesh;

        MeshRenderer renderer = visual.AddComponent<MeshRenderer>();

        renderer.sharedMaterial = sharedMaterial;
        renderer.sortingOrder = sortingOrder;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        chunks[index] = new Chunk { root = root, mesh = mesh };
    }

    private Mesh BuildMesh(Vector2[] points, float lowest)
    {
        int count = points.Length;

        var vertices = new Vector3[count * 5];
        var colors = new Color32[count * 5];
        var uvs = new Vector2[count * 5];

        const float overhang = 0.4f;

        var depths = new Vector2[count * 5];

        Color32 surface = surfaceColor;
        Color32 soil = soilColor;
        Color32 deep = deepColor;

        float bottom = lowest - depthBelow;

        bool hasTexture = groundTexture;

        for (int i = 0; i < count; i++)
        {
            Vector2 p = points[i];

            float u = hasTexture ? p.x / textureTileSize : 0f;

            float lineY = p.y - surfaceThickness;

            vertices[i * 5] = new Vector3(p.x, p.y + overhang, 0f);
            colors[i * 5] = surface;
            uvs[i * 5] = new Vector2(u, 1f + overhang / Mathf.Max(textureTileSize, 0.001f));
            depths[i * 5] = new Vector2(-overhang, 0f);

            vertices[i * 5 + 1] = new Vector3(p.x, p.y, 0f);
            colors[i * 5 + 1] = surface;
            uvs[i * 5 + 1] = new Vector2(u, 1f);
            depths[i * 5 + 1] = new Vector2(0f, 0f);

            vertices[i * 5 + 2] = new Vector3(p.x, lineY, 0f);
            colors[i * 5 + 2] = surface;
            uvs[i * 5 + 2] = new Vector2(u, 0.85f);

            depths[i * 5 + 2] = new Vector2(surfaceThickness, 0f);

            vertices[i * 5 + 3] = new Vector3(p.x, lineY, 0f);
            colors[i * 5 + 3] = soil;
            uvs[i * 5 + 3] = new Vector2(u, 0.7f);
            depths[i * 5 + 3] = new Vector2(surfaceThickness, 0f);

            vertices[i * 5 + 4] = new Vector3(p.x, bottom, 0f);
            colors[i * 5 + 4] = deep;
            uvs[i * 5 + 4] = new Vector2(u, 0f);
            depths[i * 5 + 4] = new Vector2(p.y - bottom, 0f);
        }

        var triangles = new int[(count - 1) * 24];

        int t = 0;

        for (int i = 0; i < count - 1; i++)
        {
            int a = i * 5;
            int b = (i + 1) * 5;

            for (int row = 0; row < 4; row++)
            {
                int a0 = a + row;
                int a1 = a + row + 1;
                int b0 = b + row;
                int b1 = b + row + 1;

                triangles[t++] = a0;
                triangles[t++] = b0;
                triangles[t++] = b1;

                triangles[t++] = a0;
                triangles[t++] = b1;
                triangles[t++] = a1;
            }
        }

        Mesh mesh = new Mesh { name = "TerrainMesh" };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors32 = colors;
        mesh.uv = uvs;

        mesh.uv2 = depths;

        mesh.RecalculateBounds();

        return mesh;
    }

    private Material BuildMaterial()
    {
        Material material;

        if (terrainMaterial)
        {
            material = new Material(terrainMaterial);
        }
        else
        {
            Shader shader = Shader.Find("Sprites/Default");

            if (!shader)
                shader = Shader.Find("Universal Render Pipeline/Unlit");

            material = new Material(shader);
        }

        material.name = "TerrainMaterial";

        if (groundTexture)
        {
            material.mainTexture = groundTexture;
            material.mainTextureScale = new Vector2(1f / Mathf.Max(textureTileSize, 0.001f), 1f / Mathf.Max(textureTileSize, 0.001f));

            if (material.HasProperty("_UseMainTex"))
                material.SetFloat("_UseMainTex", 1f);
        }

        if (material.HasProperty("_GrassThickness"))
            material.SetFloat("_GrassThickness", surfaceThickness);

        return material;
    }

    private void DestroyChunk(int index)
    {
        if (!chunks.TryGetValue(index, out Chunk chunk))
            return;

        if (chunk.root)
            Destroy(chunk.root);

        if (chunk.mesh)
            Destroy(chunk.mesh);

        chunks.Remove(index);
    }
}