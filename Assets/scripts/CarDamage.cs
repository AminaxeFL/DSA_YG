using UnityEngine;

[ExecuteAlways]
public class CarDamage : PartDamage
{
    public SpriteRenderer sprite;
    public SpriteRenderer kopot;

    [Range(0.01f, 1f)] public float kopotBrokenAt = 0.2f;

    private HingeJoint2D kopotHinge;
    private FixedJoint2D kopotFixed;

    private void Awake()
    {
        sprite ??= GetComponent<SpriteRenderer>();

        kopot ??= FindKopot();

        kopotHinge = kopot?.GetComponent<HingeJoint2D>();
        kopotFixed = kopot?.GetComponent<FixedJoint2D>();
    }

    private SpriteRenderer FindKopot()
    {
        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>(true))
            if (renderer.gameObject.name == "kopot")
                return renderer;

        return null;
    }

    protected override void Apply()
    {
        LookValues(out float rust, out float crack);

        DamageLook.Apply(sprite, rust, crack);
        DamageLook.Apply(kopot, rust, crack);

        ApplyKopotLimits();
    }

    private void ApplyKopotLimits()
    {
        if (!kopotHinge)
            return;

        if (Hp01 <= kopotBrokenAt)
        {
            if (kopotFixed)
                kopotFixed.enabled = false;

            kopotHinge.useLimits = true;
            kopotHinge.limits = new JointAngleLimits2D { min = 283f, max = 359f };
        }
        else
        {
            if (kopotFixed)
                kopotFixed.enabled = true;

            float angle = kopotHinge.jointAngle;

            kopotHinge.useLimits = true;
            kopotHinge.limits = new JointAngleLimits2D { min = angle, max = angle };
        }
    }
}
