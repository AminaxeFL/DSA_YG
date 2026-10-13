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

        if (kopot == null)
            kopot = FindByName(transform, "kopot");

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
        if (root == null)
            return;

        foreach (SpriteRenderer r in root.GetComponentsInChildren<SpriteRenderer>(true))
            bodyRenderers.Remove(r);
    }

    private static Transform FindTransformByName(Transform root, string target)
    {
        if (string.IsNullOrEmpty(target))
            return null;

        foreach (Transform child in root)
            if (string.Equals(child.name, target, System.StringComparison.OrdinalIgnoreCase))
                return child;

        return null;
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

        if (kopot != null)
            kopot.sortingOrder = bodyOrder - kopotDepthOffset;

        ApplySmokeDepth(bodyOrder);
    }

    private void ApplySmokeDepth(int bodyOrder)
    {
        Transform smoke = FindTransformByName(transform, smokeObjectName);

        if (smoke == null)
            return;

        foreach (SpriteRenderer r in smoke.GetComponentsInChildren<SpriteRenderer>(true))
            if (r != null)
                r.sortingOrder = bodyOrder + smokeDepthOffset;
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
        if (spinner != null)
        {
            var wheelsList = new List<Rigidbody2D>();
            foreach (Rigidbody2D wheel in spinner.GetComponentsInChildren<Rigidbody2D>(true))
            {
                if (wheel != null && wheel.GetComponent<WheelJoint2D>() != null)
                    wheelsList.Add(wheel);
            }
            if (wheelsList.Count > 0)
                return wheelsList.ToArray();
        }

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