using UnityEngine;
using UnityEngine.Rendering.Universal;

public class NightLights : LightSwitcher
{
    [SerializeField] private Light2D[] lights = new Light2D[0];

    protected override void ApplyToLights(bool on)
    {
        if (lights.Length == 0)
            return;

        foreach (Light2D light in lights)
            SetLight(light, on);
    }
}
