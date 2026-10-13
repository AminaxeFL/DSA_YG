using UnityEngine;

[ExecuteAlways]
public class WheelDamage : MonoBehaviour
{
    public const float SliderMax = 500f;

    [Min(1f)] public float maxHp = 500f;

    [Range(0f, SliderMax)] public float hp = 500f;

    public SpriteRenderer[] wheels;

    [Range(0f, 1f)] public float rustAtZero = 0.8f;
    [Range(0f, 1f)] public float crackAtZero = 0.6f;

    [Range(0.25f, 4f)] public float damageBias = 1f;

    public float MaxHp => maxHp;
    public float Hp01 => Mathf.Clamp01(hp / Mathf.Max(maxHp, 1f));

    private void Awake()
    {
        Collect();
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

    public void Collect()
    {
        if (wheels != null && wheels.Length > 0)
            return;

        SpriteRenderer[] found = GetComponentsInChildren<SpriteRenderer>(true);

        int count = 0;

        for (int i = 0; i < found.Length; i++)
        {
            if (found[i] == null)
                continue;

            if (found[i].gameObject == gameObject)
                continue;

            count++;
        }

        if (count == 0)
            return;

        wheels = new SpriteRenderer[count];

        int index = 0;

        for (int i = 0; i < found.Length; i++)
        {
            if (found[i] == null)
                continue;

            if (found[i].gameObject == gameObject)
                continue;

            wheels[index] = found[i];
            index++;
        }
    }

    private void Apply()
    {
        if (wheels == null || wheels.Length == 0)
            return;

        float t = DamageLook.Curve(Hp01, damageBias);

        float rust = DamageLook.RustAt(t, rustAtZero);
        float crack = DamageLook.CrackAt(t, crackAtZero);

        for (int i = 0; i < wheels.Length; i++)
        {
            DamageLook.Apply(wheels[i], rust, crack);
        }
    }
}