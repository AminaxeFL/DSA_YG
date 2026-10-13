using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private float riseHeight = 1.2f;

    [SerializeField] private float riseTime = 0.6f;

    [Range(1f, 255f)] [SerializeField] private float alphaStep = 50f;

    [SerializeField] private AudioClip pickupClip;

    [Range(0f, 1f)] [SerializeField] private float pickupVolume = 0.7f;

    private static readonly int AlphaId = Shader.PropertyToID("_Alpha");

    private Rigidbody2D body;
    private SpriteRenderer renderer;
    private MaterialPropertyBlock block;

    private Transform carRoot;
    private Vector3 riseFrom;
    private float riseT;
    private float alpha = 1f;
    private bool collected;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();

        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D circle = gameObject.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            circle.radius = 0.18f;
        }

        WheelSpinner car = FindAnyObjectByType<WheelSpinner>();

        carRoot = car != null ? car.transform.root : null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || carRoot == null)
            return;

        Rigidbody2D rb = other.attachedRigidbody;

        if (rb == null || rb.transform.root != carRoot)
            return;

        collected = true;

        if (pickupClip != null)
            AudioSource.PlayClipAtPoint(pickupClip, transform.position, pickupVolume);

        if (Money.Instance != null)
            Money.Instance.AddMoney(1f);

        riseFrom = transform.position;

        FloatBob bob = GetComponent<FloatBob>();

        if (bob != null)
            bob.enabled = false;
    }

    private void Update()
    {
        if (!collected)
            return;

        riseT += Time.deltaTime / Mathf.Max(riseTime, 0.01f);

        float t = Mathf.Clamp01(riseT);

        float easeOut = 1f - (1f - t) * (1f - t);

        Vector3 pos = riseFrom;
        pos.y += Mathf.Lerp(0f, riseHeight, easeOut);

        if (body != null)
            body.position = pos;
        else
            transform.position = pos;

        int steps = Mathf.Max(1, Mathf.CeilToInt(255f / Mathf.Max(alphaStep, 1f)));

        float q = steps > 1
            ? Mathf.Clamp01(Mathf.Floor(t * steps) / (steps - 1f))
            : t;

        alpha = 1f - q;

        if (renderer != null)
        {
            block ??= new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetFloat(AlphaId, alpha);
            renderer.SetPropertyBlock(block);
        }

        if (alpha <= 0f)
            Destroy(gameObject);
    }
}