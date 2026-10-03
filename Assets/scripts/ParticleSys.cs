using UnityEngine;
using System.Collections.Generic;

public class ParticleSys : MonoBehaviour
{
    public float partRate = 20f;
    public int partMax = 200;

    public float startSpeed = 10f;
    public float drag = 3f;
    public float lifetime = 3f;

    public float sizeX = 0.15f;
    public float sizeY = 0.15f;

    public Vector3 baseDirection = Vector3.forward;

    private struct Particle
    {
        public Vector3 position;
        public Vector3 velocity;
        public float life;
        public bool active;
        public GameObject go;
        public SpriteRenderer renderer;
    }

    private Particle[] particles;
    private float partTimer;
    private Material squareMaterial;
    private Sprite squareSprite;

    void Start()
    {
        CreateSquareAssets();
        InitPool();
    }

    void Update()
    {
        partTimer += Time.deltaTime;
        float interval = 1f / partRate;
        while (partTimer >= interval)
        {
            partOne();
            partTimer -= interval;
        }

        for (int i = 0; i < particles.Length; i++)
        {
            if (!particles[i].active) continue;
            UpdateParticle(ref particles[i]);
        }
    }

    private void partOne()
    {
        int freeIndex = -1;
        for (int i = 0; i < particles.Length; i++)
        {
            if (!particles[i].active) { freeIndex = i; break; }
        }
        if (freeIndex == -1) return;

        float angleDeg = Random.Range(4f, 5.5f);
        float rotAround = Random.Range(0f, 360f);

        float aRad = angleDeg * Mathf.Deg2Rad;
        float rRad = rotAround * Mathf.Deg2Rad;
        Vector3 localOffset = new Vector3(
            Mathf.Sin(aRad) * Mathf.Cos(rRad),
            Mathf.Sin(aRad) * Mathf.Sin(rRad),
            Mathf.Cos(aRad)
        );

        Quaternion rot = Quaternion.FromToRotation(Vector3.forward, baseDirection.normalized);
        Vector3 worldDir = (rot * localOffset).normalized;

        ref Particle p = ref particles[freeIndex];
        p.active = true;
        p.position = transform.position;
        p.velocity = worldDir * startSpeed;
        p.life = lifetime;

        p.go.SetActive(true);
        p.go.transform.position = p.position;
        p.go.transform.localScale = new Vector3(sizeX, sizeY, 1f);
        p.renderer.color = new Color(1f, 1f, 1f, 1f);
    }

    private void UpdateParticle(ref Particle p)
    {
        p.life -= Time.deltaTime;
        if (p.life <= 0f)
        {
            p.active = false;
            p.go.SetActive(false);
            return;
        }

        p.velocity *= Mathf.Max(0f, 1f - drag * Time.deltaTime);

        p.position += p.velocity * Time.deltaTime;
        p.go.transform.position = p.position;

        float progress = 1f - (p.life / lifetime);
        int step = Mathf.Min(5, Mathf.FloorToInt(progress * 5f));
        float alpha = 1f - step * 0.2f;
        p.renderer.color = new Color(1f, 1f, 1f, alpha);
    }

    private void InitPool()
    {
        particles = new Particle[partMax];
        for (int i = 0; i < partMax; i++)
        {
            GameObject go = new GameObject($"Particle_{i}");
            go.transform.SetParent(transform);
            go.hideFlags = HideFlags.HideInHierarchy;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = squareSprite;
            sr.material = squareMaterial;
            sr.color = new Color(1, 1, 1, 0);

            go.SetActive(false);

            particles[i] = new Particle
            {
                active = false,
                go = go,
                renderer = sr
            };
        }
    }

    private void CreateSquareAssets()
    {
        Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] px = new Color[16];
        for (int i = 0; i < px.Length; i++) px[i] = Color.white;
        tex.SetPixels(px);
        tex.Apply();

        squareSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);

        squareMaterial = new Material(Shader.Find("Sprites/Default"));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, baseDirection.normalized * 2f);
    }
}