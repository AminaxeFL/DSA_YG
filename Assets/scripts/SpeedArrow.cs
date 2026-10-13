using UnityEngine;

public class SpeedArrow : MonoBehaviour
{
    [SerializeField] private WheelSpinner car;

    [SerializeField] private float minX = -909.34f;

    [SerializeField] private float maxX = -408.8f;

    [SerializeField] private float smoothTime = 0.15f;

    private float position01;
    private float position01Velocity;
    private Vector3 startPosition;

    private void Awake()
    {
        startPosition = transform.localPosition;
        position01 = Mathf.Clamp01(Mathf.InverseLerp(minX, maxX, startPosition.x));
    }

    private void Update()
    {
        if (car == null)
        {
            car = FindAnyObjectByType<WheelSpinner>();
            if (car == null)
                return;
        }

        float fuel = car.Fuel01;
        float target = Mathf.Clamp01(fuel);

        position01 = Mathf.SmoothDamp(position01, target, ref position01Velocity, smoothTime);
        position01 = Mathf.Clamp01(position01);

        Vector3 next = startPosition;
        next.x = Mathf.Lerp(minX, maxX, position01);
        transform.localPosition = next;
    }
}