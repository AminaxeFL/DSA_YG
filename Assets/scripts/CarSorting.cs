using System;
using System.Collections.Generic;
using UnityEngine;

public class CarSorting : MonoBehaviour
{
    public int depthOrder = 0;
    public int wheelDepthOffset = 1;
    public SpriteRenderer helm;
    public SpriteRenderer neck;
    public int helmDepthOffset = 1;
    public int neckDepthOffset = 2;
    public SpriteRenderer springRight;
    public SpriteRenderer springLeft;
    public int springDepthOffset = 3;
    public SpriteRenderer kopot;
    public int kopotDepthOffset = 1;
    public string smokeObjectName = "smokeParticles";
    public int smokeDepthOffset = -1;
    [SerializeField] private float maxWheelDistance = 3f;

    public int SmokeSortingOrder => depthOrder + smokeDepthOffset;

    private readonly List<SpriteRenderer> bodyRenderers = new List<SpriteRenderer>();
    private readonly List<SpriteRenderer> wheelRenderers = new List<SpriteRenderer>();

    private void Awake()
    {
        Rebuild();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
            Rebuild();
    }

    public void Rebuild()
    {
        CollectRenderers();
        ApplyDepth();
    }

    public void SetDepth(int order)
    {
        depthOrder = order;
        ApplyDepth();
    }

    private void CollectRenderers()
    {
        bodyRenderers.Clear();
        wheelRenderers.Clear();

        helm ??= FindByName(transform, "helm");
        neck ??= FindByName(transform, "neck");
        springRight ??= FindByName(transform, "springRight");
        springLeft ??= FindByName(transform, "springLeft");
        kopot ??= FindByName(transform, "kopot");

        bodyRenderers.AddRange(GetComponentsInChildren<SpriteRenderer>(true));

        foreach (Rigidbody2D wheel in FindWheels())
            wheelRenderers.AddRange(wheel.GetComponentsInChildren<SpriteRenderer>(true));

        bodyRenderers.RemoveAll(r => wheelRenderers.Contains(r));

        RemoveWithChildren(helm);
        RemoveWithChildren(neck);
        RemoveWithChildren(springRight);
        RemoveWithChildren(springLeft);
        RemoveWithChildren(kopot);
        RemoveTransformWithChildren(FindTransformByName(transform, smokeObjectName));
    }

    private void RemoveTransformWithChildren(Transform root)
    {
        if (!root)
            return;

        foreach (SpriteRenderer r in root.GetComponentsInChildren<SpriteRenderer>(true))
            bodyRenderers.Remove(r);
    }

    private static Transform FindTransformByName(Transform root, string target)
    {
        if (string.IsNullOrEmpty(target))
            return null;

        foreach (Transform child in root)
            if (string.Equals(child.name, target, StringComparison.OrdinalIgnoreCase))
                return child;

        return null;
    }

    private static SpriteRenderer FindByName(Transform root, string target)
    {
        foreach (SpriteRenderer renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (renderer && string.Equals(renderer.gameObject.name, target, StringComparison.OrdinalIgnoreCase))
                return renderer;
        }

        return null;
    }

    private void RemoveWithChildren(SpriteRenderer root)
    {
        if (!root)
            return;

        foreach (SpriteRenderer renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
            bodyRenderers.Remove(renderer);
    }

    private void ApplyDepth()
    {
        int bodyOrder = depthOrder;
        int wheelOrder = depthOrder - wheelDepthOffset;

        foreach (SpriteRenderer r in bodyRenderers)
            SetOrder(r, bodyOrder);

        foreach (SpriteRenderer r in wheelRenderers)
            SetOrder(r, wheelOrder);

        SetOrder(helm, bodyOrder - helmDepthOffset);
        SetOrder(neck, bodyOrder - neckDepthOffset);
        SetOrder(springRight, bodyOrder - springDepthOffset);
        SetOrder(springLeft, bodyOrder - springDepthOffset);
        SetOrder(kopot, bodyOrder - kopotDepthOffset);

        ApplySmokeDepth(bodyOrder);
    }

    private static void SetOrder(SpriteRenderer renderer, int order)
    {
        if (renderer)
            renderer.sortingOrder = order;
    }

    private void ApplySmokeDepth(int bodyOrder)
    {
        Transform smoke = FindTransformByName(transform, smokeObjectName);

        if (!smoke)
            return;

        foreach (SpriteRenderer r in smoke.GetComponentsInChildren<SpriteRenderer>(true))
            SetOrder(r, bodyOrder + smokeDepthOffset);
    }

    private Rigidbody2D[] FindWheels()
    {
        WheelSpinner spinner = GetComponent<WheelSpinner>();

        if (spinner)
        {
            var wheelsList = new List<Rigidbody2D>();

            foreach (Rigidbody2D wheel in spinner.GetComponentsInChildren<Rigidbody2D>(true))
            {
                if (wheel && wheel.GetComponent<WheelJoint2D>())
                    wheelsList.Add(wheel);
            }

            if (wheelsList.Count > 0)
                return wheelsList.ToArray();
        }

        Rigidbody2D body = GetComponent<Rigidbody2D>();

        if (!body)
            return Array.Empty<Rigidbody2D>();

        var found = new List<Rigidbody2D>();

        foreach (CircleCollider2D circle in FindObjectsByType<CircleCollider2D>())
        {
            Rigidbody2D candidate = circle.attachedRigidbody;

            if (!candidate || candidate == body)
                continue;

            if (Vector2.Distance(candidate.position, body.position) > maxWheelDistance)
                continue;

            if (!found.Contains(candidate))
                found.Add(candidate);
        }

        return found.ToArray();
    }
}
