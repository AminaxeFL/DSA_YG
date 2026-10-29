using UnityEngine;

public class EngineLoop : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private WheelSpinner car;
    [SerializeField] private bool persistAcrossScenes = true;

    public float volume = 1f;
    public float gainFromSpeed = 1.5f;
    public float speedForMaxGain = 40f;
    public float volumeSmoothTime = 0.3f;
    public float pitchGain = 0.25f;
    public float pitchSmoothTime = 0.25f;
    public float fuelFadeTime = 3f;

    private float volumeVelocity;
    private float pitchVelocity;
    private float fuelFadeTimer;
    private bool wasRunning;

    private void Awake()
    {
        if (!source)
            source = GetComponent<AudioSource>();

        if (!source)
            source = gameObject.AddComponent<AudioSource>();

        if (persistAcrossScenes)
            DontDestroyOnLoad(gameObject);

        car ??= FindAnyObjectByType<WheelSpinner>();

        SetupSource();
        Play();

        source.volume = 0f;

        if (!car)
            enabled = false;
    }

    private void SetupSource()
    {
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.pitch = 1f;
    }

    public void Play()
    {
        if (!source.clip)
            return;

        SetupSource();

        if (!source.isPlaying)
            source.Play();
    }

    private float CurrentSpeed()
    {
        Rigidbody2D chassis = car.Chassis;

        return chassis ? chassis.linearVelocity.magnitude : Mathf.Abs(car.Speed);
    }

    private void Update()
    {
        if (!source.clip)
            return;

        if (!source.isPlaying)
            source.Play();

        float t = Mathf.Clamp01(CurrentSpeed() / Mathf.Max(speedForMaxGain, 0.01f));

        float peak = Mathf.Clamp01(volume);
        float gain = Mathf.Max(gainFromSpeed, 0f);
        float idle = peak / (1f + gain);

        bool running = car.Fuel01 > 0.001f && car.canGo;

        if (running)
        {
            fuelFadeTimer = 0f;
            wasRunning = true;

            source.volume = Mathf.SmoothDamp(
                source.volume,
                Mathf.Lerp(idle, peak, t),
                ref volumeVelocity,
                Mathf.Max(volumeSmoothTime, 0.01f));

            source.pitch = Mathf.SmoothDamp(
                source.pitch,
                1f + Mathf.Max(pitchGain, 0f) * t,
                ref pitchVelocity,
                Mathf.Max(pitchSmoothTime, 0.01f));
        }
        else
        {
            if (wasRunning)
            {
                fuelFadeTimer = 0f;
                wasRunning = false;
            }

            fuelFadeTimer += Time.deltaTime;

            float fade = 1f - Mathf.Clamp01(fuelFadeTimer / Mathf.Max(fuelFadeTime, 0.01f));

            source.volume = Mathf.SmoothDamp(
                source.volume,
                Mathf.Lerp(0f, Mathf.Lerp(idle, peak, t), fade),
                ref volumeVelocity,
                Mathf.Max(volumeSmoothTime, 0.01f));

            source.pitch = Mathf.SmoothDamp(
                source.pitch,
                1f + Mathf.Max(pitchGain, 0f) * t * fade,
                ref pitchVelocity,
                Mathf.Max(pitchSmoothTime, 0.01f));

            if (fade <= 0f && source.volume < 0.01f)
                source.Pause();
        }
    }
}
