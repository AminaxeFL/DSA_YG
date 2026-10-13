using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CarLights : MonoBehaviour
{
    private static readonly int SkySunAzElId = Shader.PropertyToID("_SkySunAzEl");

    [Range(0f, 1f)] public float lightsOnBelow = 0.25f;

    [SerializeField] private float lightsHysteresis = 0.03f;

    [SerializeField] private Light2D frontLight;
    [SerializeField] private Light2D insideLight;
    [SerializeField] private Light2D backLight;

    private bool lightsOn;
    private bool lightsValid;

    private void OnEnable()
    {
        lightsValid = false;
        Resolve();
    }

    private void Reset()
    {
        Resolve();
    }

    private void Resolve()
    {
        if (frontLight != null && insideLight != null && backLight != null)
            return;

        foreach (Light2D light in FindObjectsByType<Light2D>())
        {
            if (frontLight == null && light.gameObject.name == "frontLight")
                frontLight = light;
            else if (insideLight == null && light.gameObject.name == "insideLight")
                insideLight = light;
            else if (backLight == null && light.gameObject.name == "backLight")
                backLight = light;
        }
    }

    private void Update()
    {
        Apply(Shader.GetGlobalVector(SkySunAzElId).z);
    }

    private void Apply(float daylight)
    {
        if (frontLight == null && insideLight == null && backLight == null)
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

        SetLight(frontLight, lightsOn);
        SetLight(insideLight, lightsOn);
        SetLight(backLight, lightsOn);
    }

    private void SetLight(Light2D light, bool on)
    {
        if (light == null)
            return;

        if (light.enabled == on)
            return;

        light.enabled = on;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.EditorUtility.SetDirty(light);
#endif
    }
}