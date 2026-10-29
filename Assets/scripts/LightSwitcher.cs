using UnityEngine;
using UnityEngine.Rendering.Universal;

public abstract class LightSwitcher : MonoBehaviour
{
    private static readonly int SkySunAzElId = Shader.PropertyToID("_SkySunAzEl");

    [Range(0f, 1f)] public float lightsOnBelow = 0.25f;
    [SerializeField] protected float lightsHysteresis = 0.03f;

    private bool lightsOn;
    private bool lightsValid;

    protected abstract void ApplyToLights(bool on);

    protected virtual void OnEnable()
    {
        lightsValid = false;
        Resolve();
    }

    protected virtual void Reset()
    {
        Resolve();
    }

    protected virtual void Resolve()
    {
    }

    protected virtual void Update()
    {
        ApplyToLights(Switch(Shader.GetGlobalVector(SkySunAzElId).z));
    }

    private bool Switch(float daylight)
    {
        if (!lightsValid)
        {
            lightsOn = daylight < lightsOnBelow;
            lightsValid = true;
        }
        else if (lightsOn)
        {
            lightsOn = daylight < lightsOnBelow + lightsHysteresis;
        }
        else
        {
            lightsOn = daylight < lightsOnBelow - lightsHysteresis;
        }

        return lightsOn;
    }

    protected static void SetLight(Light2D light, bool on)
    {
        if (!light || light.enabled == on)
            return;

        light.enabled = on;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.EditorUtility.SetDirty(light);
#endif
    }
}
