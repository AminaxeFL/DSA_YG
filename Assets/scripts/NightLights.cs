using UnityEngine;
using UnityEngine.Rendering.Universal;

public class NightLights : MonoBehaviour
{
    private static readonly int SkySunAzElId = Shader.PropertyToID("_SkySunAzEl");

    [Range(0f, 1f)] public float lightsOnBelow = 0.25f;

    [SerializeField] private float lightsHysteresis = 0.03f;

    [SerializeField] private Light2D[] lights;

    private bool lightsOn;
    private bool lightsValid;

    private void Update()
    {
        Apply(Shader.GetGlobalVector(SkySunAzElId).z);
    }

    private void Apply(float daylight)
    {
        if (lights == null || lights.Length == 0)
            return;

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

        foreach (Light2D light in lights)
            SetLight(light, lightsOn);
    }

    private void SetLight(Light2D light, bool on)
    {
        if (light == null || light.enabled == on)
            return;

        light.enabled = on;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.EditorUtility.SetDirty(light);
#endif
    }
}