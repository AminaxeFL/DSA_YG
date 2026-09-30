using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    [SerializeField] private Vector2 offset = new Vector2(0f, 0.7f);

    [Range(0.5f, 8f)] [SerializeField] private float frequency = 2.2f;

    [Range(0.05f, 0.99f)] [SerializeField] private float dampingRatio = 0.65f;

    [SerializeField] private float maxSpeed = 40f;

    [SerializeField] private float lookAheadDistance = 2.5f;

    [SerializeField] private float lookAheadSpeed = 12f;

    [Header("Зум камеры ок")]
    [Range(0f, 30f)] [SerializeField] private float baseSize = 0f;

    [Range(0f, 30f)] [SerializeField] private float zoomOut = 3f;

    [Range(1f, 80f)] [SerializeField] private float zoomSpeed = 25f;

    [Range(0.02f, 2f)] [SerializeField] private float zoomSmoothTime = 0.35f;

    private Camera cam;
    private Rigidbody2D targetBody;
    private Vector2 velocity;
    private float sizeVelocity;

    private void Start()
    {
        cam = GetComponent<Camera>();

        if (cam != null && baseSize <= 0f)
            baseSize = cam.orthographicSize;

        targetBody = target != null ? FindBody(target) : null;

        if (target != null)
            transform.position = DesiredPosition();
    }

    private void LateUpdate()
    {
        ApplyZoom();

        if (target == null)
            return;

        Vector2 goal = DesiredPosition();
        Vector2 deviation = (Vector2)transform.position - goal;

        float omega = frequency * 2f * Mathf.PI;
        float decay = dampingRatio * omega;
        float w0Sq = omega * omega;
        float wd = Mathf.Sqrt(w0Sq - decay * decay);

        Vector2 v0 = velocity;
        float decayFactor = Mathf.Exp(-decay * Time.deltaTime);
        float cos = Mathf.Cos(wd * Time.deltaTime);
        float sin = Mathf.Sin(wd * Time.deltaTime);

        Vector2 b = (v0 + decay * deviation) / wd;
        velocity = decayFactor * (v0 * cos - (w0Sq * deviation + decay * v0) / wd * sin);
        Vector2 offsetNow = decayFactor * (deviation * cos + b * sin);

        if (velocity.sqrMagnitude > maxSpeed * maxSpeed)
            velocity = velocity.normalized * maxSpeed;

        transform.position = goal + offsetNow;
    }

    private void ApplyZoom()
    {
        if (cam == null || zoomOut <= 0f)
            return;

        float speed = targetBody != null ? targetBody.linearVelocity.magnitude : 0f;
        float t = Mathf.Clamp01(speed / Mathf.Max(zoomSpeed, 0.01f));

        float wanted = baseSize + zoomOut * t;

        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, wanted, ref sizeVelocity, zoomSmoothTime);
    }

    private static Rigidbody2D FindBody(Transform root)
    {
        Rigidbody2D own = root.GetComponent<Rigidbody2D>();

        if (own != null)
            return own;

        foreach (Rigidbody2D candidate in root.GetComponentsInChildren<Rigidbody2D>())
        {
            if (candidate == null || candidate.GetComponent<Collider2D>() == null)
                continue;

            if (candidate.GetComponent<CircleCollider2D>() != null)
                continue;

            return candidate;
        }

        return null;
    }

    private Vector2 DesiredPosition()
    {
        return (Vector2)target.position + offset + LookAheadOffset();
    }

    private Vector2 LookAheadOffset()
    {
        if (targetBody == null || lookAheadDistance <= 0f)
            return Vector2.zero;

        Vector2 v = targetBody.linearVelocity;
        float speed = v.magnitude;

        if (speed < 0.01f)
            return Vector2.zero;

        float t = Mathf.Clamp01(speed / Mathf.Max(lookAheadSpeed, 0.01f));
        return v / speed * lookAheadDistance * t;
    }
}