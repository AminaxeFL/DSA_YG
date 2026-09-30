using System.Collections.Generic;
using UnityEngine;

public class CarShade : MonoBehaviour
{
    [Range(0.01f, 0.5f)] public float shadeStep = 0.1f;

    [Range(0f, 1f)] public float shadeMax = 0.8f;

    [Range(0.1f, 6f)] public float transitionTime = 3f;

    [Range(0.001f, 0.1f)] public float restartEpsilon = 0.02f;

    [Range(0.1f, 5f)] public float rowTolerance = 1.5f;

    private struct Part
    {
        public SpriteRenderer renderer;
        public Color baseColor;
    }

    private readonly List<Part> parts = new List<Part>();
    private float shade;      
    private float fromShade;  
    private float toShade;    
    private float transitionT = 1f;  
    private WheelSpinner car;
    private SpriteRenderer bodyRenderer;   

    private void Awake()
    {
        car = GetComponent<WheelSpinner>();

        Transform chassis = FindChassis();
        bodyRenderer = chassis != null ? chassis.GetComponent<SpriteRenderer>() : null;

        CollectParts(chassis);
    }

    private Transform FindChassis()
    {
        if (car != null)
            return car.transform;

        for (Transform t = transform; t != null; t = t.parent)
        {
            if (t.GetComponent<WheelSpinner>() != null && t.GetComponent<Rigidbody2D>() != null)
                return t;
        }

        return null;
    }

    private void CollectParts(Transform chassis)
    {
        parts.Clear();
        if (chassis == null)
            return;

        foreach (SpriteRenderer r in chassis.GetComponentsInChildren<SpriteRenderer>(true))
            parts.Add(new Part { renderer = r, baseColor = r.color });

        if (car != null && car.wheels != null)
        {
            foreach (Rigidbody2D wheel in car.wheels)
            {
                if (wheel == null)
                    continue;

                foreach (SpriteRenderer r in wheel.GetComponentsInChildren<SpriteRenderer>(true))
                    parts.Add(new Part { renderer = r, baseColor = r.color });
            }
        }
    }

    private void LateUpdate()
    {
        float target = ComputeShade();

        if (Mathf.Abs(target - toShade) > restartEpsilon)
        {
            fromShade = shade;
            toShade = target;
            transitionT = 0f;
        }

        if (transitionT < 1f)
        {
            transitionT = Mathf.Min(1f, transitionT + Time.deltaTime / Mathf.Max(transitionTime, 0.01f));
            float eased = Mathf.SmoothStep(0f, 1f, transitionT);
            shade = Mathf.Lerp(fromShade, toShade, eased);
        }
        else
        {
            shade = toShade;
        }

        ApplyShade();
    }

    private float ComputeShade()
    {
        Bounds mine = WorldBounds();
        if (IsEmpty(mine))
            return 0f;

        int myFront = MyFrontness();
        float myY = mine.center.y;

        int ahead = 0;

        foreach (CarShade other in FindObjectsByType<CarShade>(FindObjectsInactive.Exclude))
        {
            if (other == this || other == null)
                continue;

            if (other.MyFrontness() <= myFront)
                continue;

            Bounds theirs = other.WorldBounds();
            if (IsEmpty(theirs))
                continue;

            if (Mathf.Abs(theirs.center.y - myY) > rowTolerance)
                continue;

            ahead++;
        }

        return Mathf.Min(ahead * shadeStep, shadeMax);
    }

    private void ApplyShade()
    {
        float k = 1f - shade;
        foreach (Part p in parts)
        {
            if (p.renderer == null)
                continue;

            Color c = p.baseColor;
            c.r *= k;
            c.g *= k;
            c.b *= k;
            p.renderer.color = c;
        }
    }

    private int MyFrontness()
    {
        return bodyRenderer != null ? bodyRenderer.sortingOrder : 0;
    }

    private Bounds WorldBounds()
    {
        bool any = false;
        Bounds b = default;

        foreach (Part p in parts)
        {
            if (p.renderer == null || !p.renderer.enabled)
                continue;

            if (!any)
            {
                b = p.renderer.bounds;
                any = true;
            }
            else
            {
                b.Encapsulate(p.renderer.bounds);
            }
        }

        return b;
    }

    private static bool IsEmpty(Bounds b)
    {
        return b.size.x <= 0f || b.size.y <= 0f;
    }
}