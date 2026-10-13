using UnityEngine;

public class FloatBob : MonoBehaviour
{
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private float bobSpeed = 1.5f;

    [SerializeField] private float sideOffset = 0.15f;
    [Range(0.5f, 10f)] [SerializeField] private float sideChangeTime = 3f;

    [SerializeField] private float maxTilt = 15f;
    [Range(0.5f, 10f)] [SerializeField] private float rotateChangeTime = 4f;

    private Vector3 startPos;
    private float phase;

    private float sideFrom;
    private float sideTarget;
    private float sideT = 1f;

    private float tiltFrom;
    private float tiltTarget;
    private float tiltT = 1f;

    private void Awake()
    {
        startPos = transform.localPosition;

        sideTarget = RandomTarget(sideOffset);
        tiltTarget = RandomTarget(maxTilt);
    }

    private void Update()
    {
        phase += Time.deltaTime * bobSpeed;

        float bob = Mathf.Sin(phase * Mathf.PI * 2f) * bobHeight;

        sideT += Time.deltaTime / Mathf.Max(sideChangeTime, 0.01f);

        if (sideT >= 1f)
        {
            sideT = 0f;
            sideFrom = sideTarget;
            sideTarget = RandomTarget(sideOffset);
        }

        tiltT += Time.deltaTime / Mathf.Max(rotateChangeTime, 0.01f);

        if (tiltT >= 1f)
        {
            tiltT = 0f;
            tiltFrom = tiltTarget;
            tiltTarget = RandomTarget(maxTilt);
        }

        float easedSide = Mathf.SmoothStep(sideFrom, sideTarget, Mathf.Clamp01(sideT));
        float easedTilt = Mathf.SmoothStep(tiltFrom, tiltTarget, Mathf.Clamp01(tiltT));

        transform.localPosition = startPos + Vector3.up * bob + Vector3.right * easedSide;
        transform.localRotation = Quaternion.Euler(0f, 0f, easedTilt);
    }

    private float RandomTarget(float range)
    {
        return (Random.value * 2f - 1f) * range;
    }
}