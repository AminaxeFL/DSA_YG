using UnityEngine;
using System.Collections.Generic;

public class ParticleSys : MonoBehaviour
{
    private const float MinRate = 0.01f;
    private const float MinLifetime = 0.01f;

    private const string shaderName = "Diesel/Smoke2D";

    private static readonly int PixelSizeId = Shader.PropertyToID("_PixelSize");

    public float partRate = 20f;
    public int partMax = 200;

    public float startSpeed = 10f;
    public float drag = 3f;
    public float lifetime = 3f;

    public float sizeX = 0.15f;
    public float sizeY = 0.15f;

    public Color startColor = Color.white;
    public Color endColor = new Color(1f, 1f, 1f, 0f);
    public int fadeSteps = 1;

    [Range(0f, 1f)] public float colorJitter = 0.25f;

    public float spawnRadius = 0f;

    public float spreadUp = 0.5f;

    public float spreadSide = 0.15f;

    public float spreadExp = 0.04f;

    [Range(1f, 8f)] public float spreadExpMax = 4f;

    [Range(1, 32)] public int pixelSize = 1;

    public Vector3 baseDirection = Vector3.forward;

    [Range(0f, 1f)] public float alphaAtRest = 1f;

    [Range(0f, 1f)] public float alphaAtMaxSpeed = 0.2f;

    [Min(0f)] public float speedForMinAlpha = 40f;

    private struct Particle
    {
        public Vector3 position;
        public Vector3 velocity;
        public float life;
        public bool active;
        public int slot;
        public GameObject go;
        public SpriteRenderer renderer;
        public Color from;
        public Color to;
    }

    private Particle[] particles;
    private int[] freeSlots;
    private int freeCount;
    private int pooledMax;
    private float partTimer;
    private float spreadScale = 1f;
    private float speedAlpha = 1f;
    private Vector3 prevEmitterPos;
    private Material squareMaterial;
    private Sprite squareSprite;
    private Texture2D squareTexture;
    private bool pixelSizeSupported;
    private CarSorting sorting;
    private int smokeOrder;
    private WheelSpinner car;

    void OnValidate()
    {
        if (partMax < 0) partMax = 0;

        speedForMinAlpha = Mathf.Max(0f, speedForMinAlpha);
    }

    void Start()
    {
        sorting = transform.parent != null ? transform.parent.GetComponent<CarSorting>() : null;
        car = transform.parent != null ? transform.parent.GetComponent<WheelSpinner>() : null;

        if (sorting != null)
            smokeOrder = sorting.SmokeSortingOrder;

        CreateSquareAssets();
        EnsurePool();

        prevEmitterPos = transform.position;
        speedAlpha = alphaAtRest;
    }

    void Update()
    {
        EnsurePool();

        if (particles == null)
            return;

        float life = Mathf.Max(lifetime, MinLifetime);

        float dt = Time.deltaTime;
        Vector3 emitterPos = transform.position;

        if (squareMaterial != null && pixelSizeSupported)
            squareMaterial.SetFloat(PixelSizeId, pixelSize);

        if ((emitterPos - prevEmitterPos).sqrMagnitude > 1e4f)
            prevEmitterPos = emitterPos;

        float step = Mathf.Max(dt, 1e-6f);

        float emitterSpeed = (emitterPos - prevEmitterPos).magnitude / step;

        spreadScale = Mathf.Clamp(
            Mathf.Exp(spreadExp * emitterSpeed),
            1f,
            spreadExpMax
        );

        speedAlpha = Mathf.Lerp(
            alphaAtRest,
            alphaAtMaxSpeed,
            Mathf.Clamp01(emitterSpeed / Mathf.Max(speedForMinAlpha, 0.01f))
        );

        if (car == null || car.Fuel01 > 0.001f)
        {
            float rate = Mathf.Max(partRate, MinRate);
            float interval = 1f / rate;

            float start = partTimer;
            partTimer += dt;

            float next = interval;

            while (partTimer >= next)
            {
                float frac = Mathf.Clamp01((next - start) / step);

                partOne(life, Vector3.Lerp(prevEmitterPos, emitterPos, frac));

                next += interval;
            }
        }
        else
        {
            partTimer = 0f;
        }

        UpdateParticles(life, dt);

        prevEmitterPos = emitterPos;
    }

    private void UpdateParticles(float life, float dt)
    {
        for (int i = 0; i < particles.Length; i++)
        {
            if (!particles[i].active)
                continue;

            UpdateParticle(ref particles[i], life, dt);
        }
    }

    private void partOne(float life, Vector3 spawnPos)
    {
        if (freeCount <= 0) return;

        int freeIndex = freeSlots[--freeCount];

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

        Particle p = particles[freeIndex];
        p.active = true;
        p.velocity = worldDir * startSpeed;
        p.life = life;

        p.position = spawnPos + Perpendicular(worldDir) * Random.Range(-spawnRadius, spawnRadius);
        p.from = Jitter(startColor);
        p.to = Jitter(endColor);

        p.go.SetActive(true);
        p.go.transform.position = p.position;
        p.go.transform.localScale = new Vector3(sizeX, sizeY, 1f);
        p.renderer.color = WithSpeedAlpha(p.from);
        particles[freeIndex] = p;
    }

    private Color WithSpeedAlpha(Color c)
    {
        c.a *= speedAlpha;
        return c;
    }

    private void UpdateParticle(ref Particle p, float life, float dt)
    {
        p.life -= dt;
        if (p.life <= 0f)
        {
            p.active = false;
            p.go.SetActive(false);
            freeSlots[freeCount++] = p.slot;
            return;
        }

        Vector3 flat = new Vector3(p.velocity.x, p.velocity.y, 0f);

        if (flat.sqrMagnitude > 1e-6f)
        {
            Vector3 along = flat.normalized;
            Vector3 side = Perpendicular(along);

            p.velocity += side * Random.Range(-spreadUp, spreadUp) * spreadScale * dt;
            p.velocity += along * Random.Range(-spreadSide, spreadSide) * spreadScale * dt;
        }

        p.velocity *= Mathf.Max(0f, 1f - drag * dt);

        p.position += p.velocity * dt;
        p.go.transform.position = p.position;

        float progress = Mathf.Clamp01(1f - (p.life / life));

        int steps = Mathf.Max(fadeSteps, 1);
        float t = steps > 1
            ? Mathf.Clamp01(Mathf.Floor(progress * steps) / (steps - 1f))
            : progress;

        p.renderer.color = WithSpeedAlpha(Color.LerpUnclamped(p.from, p.to, t));
    }

    private static Vector3 Perpendicular(Vector3 dir)
    {
        Vector3 side = Vector3.Cross(dir, Vector3.up);

        if (side.sqrMagnitude < 1e-6f)
            side = Vector3.Cross(dir, Vector3.right);

        side.Normalize();

        return Vector3.Cross(side, dir).normalized;
    }

    private Color Jitter(Color c)
    {
        if (colorJitter <= 0f)
            return c;

        float k = 1f + Random.Range(-colorJitter, colorJitter);

        return new Color(
            Mathf.Clamp01(c.r * k),
            Mathf.Clamp01(c.g * k),
            Mathf.Clamp01(c.b * k),
            c.a
        );
    }

    private void EnsurePool()
    {
        int wanted = Mathf.Max(partMax, 0);
        if (particles != null && pooledMax == wanted)
            return;

        DestroyPool();

        if (wanted == 0)
        {
            pooledMax = 0;
            return;
        }

        particles = new Particle[wanted];
        freeSlots = new int[wanted];
        freeCount = wanted;

        for (int i = 0; i < wanted; i++)
        {
            GameObject go = new GameObject($"Particle_{i}");
            go.transform.SetParent(transform);
            go.hideFlags = HideFlags.HideInHierarchy;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = squareSprite;
            sr.material = squareMaterial;
            sr.color = Color.clear;
            sr.sortingOrder = smokeOrder;

            go.SetActive(false);

            particles[i] = new Particle
            {
                active = false,
                slot = i,
                go = go,
                renderer = sr
            };

            freeSlots[i] = wanted - 1 - i;
        }

        pooledMax = wanted;
        partTimer = 0f;
    }

    private void DestroyPool()
    {
        if (particles != null)
        {
            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i].go == null) continue;

                if (Application.isPlaying)
                    Destroy(particles[i].go);
                else
                    DestroyImmediate(particles[i].go);
            }
        }

        particles = null;
        freeSlots = null;
        freeCount = 0;
        pooledMax = 0;
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
        squareTexture = tex;

        Shader shader = Shader.Find(shaderName);

        if (shader == null)
        {
            if (Application.isPlaying)
                Debug.LogWarning($"{name}: шейдер {shaderName} не найден, дым будет квадратами", this);

            shader = Shader.Find("Sprites/Default");
        }

        squareMaterial = new Material(shader);

        pixelSizeSupported = squareMaterial.HasProperty(PixelSizeId);
    }

    void OnDestroy()
    {
        DestroyPool();

        if (squareSprite != null) ReleaseObject(squareSprite);
        if (squareTexture != null) ReleaseObject(squareTexture);
        if (squareMaterial != null) ReleaseObject(squareMaterial);

        squareSprite = null;
        squareTexture = null;
        squareMaterial = null;
    }

    private static void ReleaseObject(Object obj)
    {
        if (Application.isPlaying)
            Destroy(obj);
        else
            DestroyImmediate(obj);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, baseDirection.normalized * 2f);
    }
}