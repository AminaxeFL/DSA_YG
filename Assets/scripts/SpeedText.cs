using TMPro;
using UnityEngine;

public class SpeedText : MonoBehaviour
{
    private const float KmhPerUnit = 3.6f;

    [SerializeField] private WheelSpinner car;
    [SerializeField] private TMP_Text label;
    [Min(0f)] [SerializeField] private float metersPerUnit = 1f;
    [Range(0, 2)] [SerializeField] private int decimals;
    [Min(0f)] [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private string suffix = " км/ч";

    private float shownSpeed;
    private float shownVelocity;
    private float lastPrinted;
    private bool printed;
    private float powerOfTen = 1f;
    private string numberFormat = "F0";

    public float Kmh => shownSpeed;

    private void Awake()
    {
        label ??= GetComponent<TMP_Text>();

        car ??= FindAnyObjectByType<WheelSpinner>();

        CacheFormat();

        if (!label || !car)
            enabled = false;
    }

    private void OnValidate()
    {
        decimals = Mathf.Clamp(decimals, 0, 2);
        metersPerUnit = Mathf.Max(0f, metersPerUnit);

        CacheFormat();
        printed = false;
    }

    private void Update()
    {
        float kmh = ToKmh(SpeedUnits());

        shownSpeed = smoothTime <= 0.0001f
            ? kmh
            : Mathf.SmoothDamp(shownSpeed, kmh, ref shownVelocity, smoothTime);

        Print(shownSpeed);
    }

    private float SpeedUnits()
    {
        Rigidbody2D chassis = car.Chassis;

        return chassis ? chassis.linearVelocity.magnitude : Mathf.Abs(car.Speed);
    }

    private float ToKmh(float unitsPerSecond)
    {
        return unitsPerSecond * metersPerUnit * KmhPerUnit;
    }

    private void Print(float kmh)
    {
        float rounded = decimals > 0
            ? Mathf.Round(kmh * powerOfTen) / powerOfTen
            : Mathf.Round(kmh);

        if (printed && Mathf.Abs(rounded - lastPrinted) < 0.0001f)
            return;

        lastPrinted = rounded;
        printed = true;

        label.text = rounded.ToString(numberFormat) + suffix;
    }

    private void CacheFormat()
    {
        numberFormat = "F" + decimals;
        powerOfTen = Mathf.Pow(10f, decimals);
    }
}
