using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CarLights : LightSwitcher
{
    [SerializeField] private Light2D frontLight;
    [SerializeField] private Light2D insideLight;
    [SerializeField] private Light2D backLight;

    protected override void Resolve()
    {
        if (frontLight && insideLight && backLight)
            return;

        foreach (Light2D light in FindObjectsByType<Light2D>())
        {
            if (!frontLight && light.gameObject.name == "frontLight")
                frontLight = light;
            else if (!insideLight && light.gameObject.name == "insideLight")
                insideLight = light;
            else if (!backLight && light.gameObject.name == "backLight")
                backLight = light;
        }
    }

    protected override void ApplyToLights(bool on)
    {
        SetLight(frontLight, on);
        SetLight(insideLight, on);
        SetLight(backLight, on);
    }
}
