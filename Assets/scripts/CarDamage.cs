using UnityEngine;

[ExecuteAlways]
public class CarDamage : MonoBehaviour
{
    public const float SliderMax = 1000f;

    [Min(1f)] public float maxHp = 1000f;

    [Range(0f, SliderMax)] public float hp = 1000f;

    public SpriteRenderer sprite;

    public SpriteRenderer kopot;

    [Range(0.01f, 1f)] public float kopotBrokenAt = 0.2f;

    [Range(0f, 1f)] public float rustAtZero = 0.9f;
    [Range(0f, 1f)] public float crackAtZero = 0.7f;

    [Range(0.25f, 4f)] public float damageBias = 1f;

    public float MaxHp => maxHp;
    public float Hp01 => Mathf.Clamp01(hp / Mathf.Max(maxHp, 1f));

    private void Awake()
    {
        if (sprite == null)
            sprite = GetComponent<SpriteRenderer>();

        if (kopot == null)
            kopot = FindKopot();

        kopotHinge = kopot != null ? kopot.GetComponent<HingeJoint2D>() : null;
        kopotFixed = kopot != null ? kopot.GetComponent<FixedJoint2D>() : null;
    }

    private HingeJoint2D kopotHinge;

    private FixedJoint2D kopotFixed;

    private SpriteRenderer FindKopot()
    {
        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>(true))
            if (renderer.gameObject.name == "kopot")
                return renderer;

        return null;
    }

    private void OnEnable()
    {
        Apply();
    }

    private void OnValidate()
    {
        maxHp = Mathf.Max(1f, maxHp);
        hp = Mathf.Clamp(hp, 0f, maxHp);
        Apply();
    }

    private void Update()
    {
        Apply();
    }

    public void SetHp(float value)
    {
        hp = Mathf.Clamp(value, 0f, maxHp);
        Apply();
    }

    public void Damage(float amount)
    {
        SetHp(hp - amount);
    }

    public void Repair(float amount)
    {
        SetHp(hp + amount);
    }

    public void RepairFull()
    {
        SetHp(maxHp);
    }

    private void Apply()
    {
        if (sprite == null)
        {
            if (!Application.isPlaying)
                sprite = GetComponent<SpriteRenderer>();

            if (sprite == null)
                return;
        }

        float t = DamageLook.Curve(Hp01, damageBias);

        DamageLook.Apply(sprite, DamageLook.RustAt(t, rustAtZero), DamageLook.CrackAt(t, crackAtZero));
        DamageLook.Apply(kopot, DamageLook.RustAt(t, rustAtZero), DamageLook.CrackAt(t, crackAtZero));

        ApplyKopotLimits();
    }

    private void ApplyKopotLimits()
    {
        if (kopotHinge == null)
            return;

        if (Hp01 <= kopotBrokenAt)
        {
            if (kopotFixed != null)
                kopotFixed.enabled = false;

            kopotHinge.useLimits = true;
            kopotHinge.limits = new JointAngleLimits2D { min = 283f, max = 359f };
        }
        else
        {
            if (kopotFixed != null)
                kopotFixed.enabled = true;

            float angle = kopotHinge.jointAngle;

            kopotHinge.useLimits = true;
            kopotHinge.limits = new JointAngleLimits2D { min = angle, max = angle };
        }
    }
}