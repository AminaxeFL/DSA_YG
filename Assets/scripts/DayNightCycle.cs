using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class DayNightCycle : MonoBehaviour
{
    private static readonly int SunAzElId = Shader.PropertyToID("_SkySunAzEl");

    [Range(0f, 24f)] public float hour = 6f;
    public bool advance = true;
    [Min(1f)] public float daySeconds = 180f;
    [Min(1f)] public float nightSeconds = 150f;
    [Range(1f, 23f)] public float dayHours = 12f;
    [Range(0.1f, 3.2f)] public float elevationSweep = Mathf.PI;
    [Range(-6.2831853f, 6.2831853f)] public float azimuthSweep;
    [Range(0f, 6.2831853f)] public float azimuthStart;
    [Range(0f, 24f)] public float sunriseHour = 6f;
    [Min(0f)] public float smoothTime = 0.4f;
    [Min(0f)] public float dayIntensity = 1f;
    [Min(0f)] public float nightIntensity = 0.18f;

    [SerializeField] private Light2D globalLight;
    [SerializeField] private Color dayColor = Color.white;
    [SerializeField] private Color nightColor = new Color(0.26f, 0.28f, 0.34f, 1f);
    [SerializeField] private float dayLightLow = -0.25f;
    [SerializeField] private float dayLightHigh = 0.35f;

    private Color appliedColor;
    private float appliedIntensity;
    private bool appliedValid;

    public float Azimuth { get; private set; }
    public float Elevation { get; private set; }

    public float Phase => Mathf.Repeat(hour - sunriseHour, 24f);

    public float NightHours => Mathf.Max(24f - dayHours, 0.001f);

    public bool IsDay => Phase <= dayHours;

    public float CycleSeconds => daySeconds + nightSeconds;

    public Vector3 SunDirection
    {
        get
        {
            float ca = Mathf.Cos(Elevation);
            return new Vector3(ca * Mathf.Cos(Azimuth), Mathf.Sin(Elevation), ca * Mathf.Sin(Azimuth));
        }
    }

    public string Clock
    {
        get
        {
            int h = Mathf.FloorToInt(Mathf.Repeat(hour, 24f));
            int m = Mathf.FloorToInt(Mathf.Repeat(hour, 1f) * 60f);
            return string.Format("{0:00}:{1:00}", h, m);
        }
    }

    private Vector2 current;
    private bool ready;

    private void OnEnable()
    {
        ready = false;
        appliedValid = false;
        ResolveGlobalLight();
    }

    private void Reset()
    {
        ResolveGlobalLight();
    }

    private void ResolveGlobalLight()
    {
        globalLight ??= FindAnyObjectByType<Light2D>();
    }

    private void OnValidate()
    {
        hour = Mathf.Repeat(hour, 24f);
        daySeconds = Mathf.Max(1f, daySeconds);
        nightSeconds = Mathf.Max(1f, nightSeconds);
        smoothTime = Mathf.Max(0f, smoothTime);
    }

    private void Update()
    {
        if (advance && Application.isPlaying)
        {
            float rate = IsDay
                ? dayHours / daySeconds
                : NightHours / nightSeconds;

            hour = Mathf.Repeat(hour + rate * Time.deltaTime, 24f);
        }

        Evaluate(hour, out float targetAz, out float targetEl);

        if (!ready)
        {
            current = new Vector2(targetAz, targetEl);
            ready = true;
        }
        else if (smoothTime <= 0.0001f)
        {
            current = new Vector2(targetAz, targetEl);
        }
        else
        {
            float t = 1f - Mathf.Exp(-Time.deltaTime / smoothTime);

            float deltaAz = Mathf.DeltaAngle(current.x * Mathf.Rad2Deg, targetAz * Mathf.Rad2Deg) * Mathf.Deg2Rad;
            float deltaEl = Mathf.DeltaAngle(current.y * Mathf.Rad2Deg, targetEl * Mathf.Rad2Deg) * Mathf.Deg2Rad;

            current.x += deltaAz * t;
            current.y += deltaEl * t;
        }

        Azimuth = current.x;
        Elevation = current.y;

        Shader.SetGlobalVector(SunAzElId, new Vector4(Azimuth, Elevation, Daylight, 1f));

        ApplyGlobalLight();
    }

    public float Daylight => Mathf.SmoothStep(dayLightLow, dayLightHigh, Mathf.Sin(Elevation));

    public float SunY => Mathf.Sin(Elevation);

    private void ApplyGlobalLight()
    {
        if (!globalLight)
            return;

        float daylight = Daylight;

        Color tint = Color.Lerp(nightColor, dayColor, daylight);
        float intensity = Mathf.Lerp(nightIntensity, dayIntensity, daylight);

        if (appliedValid && Mathf.Abs(intensity - appliedIntensity) < 0.001f
            && (tint - appliedColor).maxColorComponent < 0.001f)
            return;

        globalLight.color = tint;
        globalLight.intensity = intensity;

        appliedColor = tint;
        appliedIntensity = intensity;
        appliedValid = true;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.EditorUtility.SetDirty(globalLight);
#endif
    }

    public void Evaluate(float h, out float azimuth, out float elevation)
    {
        float phase = Mathf.Repeat(h - sunriseHour, 24f);

        float day = Mathf.Max(dayHours, 0.001f);
        float night = NightHours;

        if (phase <= day)
            elevation = elevationSweep * (phase / day);
        else
            elevation = elevationSweep * (1f + (phase - day) / night);

        azimuth = azimuthStart + azimuthSweep * (phase / 24f);
    }

    public void SetHour(float value)
    {
        hour = Mathf.Repeat(value, 24f);
    }

    public void AdvanceHours(float amount)
    {
        SetHour(hour + amount);
    }
}