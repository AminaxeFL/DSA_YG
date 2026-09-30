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
        ReportMissingDriverParts();
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

        if (helm == null)
            helm = FindByName(transform, "helm");

        if (neck == null)
            neck = FindByName(transform, "neck");

        if (springRight == null)
            springRight = FindByName(transform, "springRight");

        if (springLeft == null)
            springLeft = FindByName(transform, "springLeft");

        bodyRenderers.AddRange(GetComponentsInChildren<SpriteRenderer>(true));

        foreach (Rigidbody2D wheel in FindWheels())
            wheelRenderers.AddRange(wheel.GetComponentsInChildren<SpriteRenderer>(true));

        bodyRenderers.RemoveAll(r => wheelRenderers.Contains(r));

        RemoveWithChildren(helm);
        RemoveWithChildren(neck);
        RemoveWithChildren(springRight);
        RemoveWithChildren(springLeft);
    }

    private static SpriteRenderer FindByName(Transform root, string target)
    {
        foreach (SpriteRenderer renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (renderer != null && string.Equals(renderer.gameObject.name, target, System.StringComparison.OrdinalIgnoreCase))
                return renderer;
        }

        return null;
    }

    private void RemoveWithChildren(SpriteRenderer root)
    {
        if (root == null)
            return;

        foreach (SpriteRenderer renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
            bodyRenderers.Remove(renderer);
    }

    private void ApplyDepth()
    {
        int bodyOrder = depthOrder;
        int wheelOrder = depthOrder - wheelDepthOffset;

        foreach (SpriteRenderer r in bodyRenderers)
            if (r != null)
                r.sortingOrder = bodyOrder;

        foreach (SpriteRenderer r in wheelRenderers)
            if (r != null)
                r.sortingOrder = wheelOrder;

        if (helm != null)
            helm.sortingOrder = bodyOrder - helmDepthOffset;

        if (neck != null)
            neck.sortingOrder = bodyOrder - neckDepthOffset;

        if (springRight != null)
            springRight.sortingOrder = bodyOrder - springDepthOffset;

        if (springLeft != null)
            springLeft.sortingOrder = bodyOrder - springDepthOffset;
    }

    private void ReportMissingDriverParts()
    {
        if (helm == null)
            Debug.LogWarning("головы нетчё то", this);

        if (neck == null)
            Debug.LogWarning("шеи нет", this);

        if (springRight == null)
            Debug.LogWarning("пружины нет (правая)", this);

        if (springLeft == null)
            Debug.LogWarning("левой пружины нет", this);
    }

    private Rigidbody2D[] FindWheels()
    {
        WheelSpinner spinner = GetComponent<WheelSpinner>();
        if (spinner != null && spinner.wheels != null && spinner.wheels.Length > 0)
            return spinner.wheels;

        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (body == null)
            return System.Array.Empty<Rigidbody2D>();

        var found = new List<Rigidbody2D>();

        foreach (CircleCollider2D circle in FindObjectsByType<CircleCollider2D>())
        {
            Rigidbody2D candidate = circle.attachedRigidbody;

            if (candidate == null || candidate == body)
                continue;

            if (Vector2.Distance(candidate.position, body.position) > maxWheelDistance)
                continue;

            if (!found.Contains(candidate))
                found.Add(candidate);
        }

        return found.ToArray();
    }

    private const float maxWheelDistance = 3f;
}