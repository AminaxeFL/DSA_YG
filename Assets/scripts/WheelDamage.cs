using UnityEngine;

public class WheelDamage : PartDamage
{
    public SpriteRenderer[] wheels = new SpriteRenderer[0];

    private void Awake()
    {
        Collect();
    }

    public void Collect()
    {
        if (wheels.Length > 0)
            return;

        SpriteRenderer[] found = GetComponentsInChildren<SpriteRenderer>(true);

        int count = 0;

        for (int i = 0; i < found.Length; i++)
        {
            if (!found[i])
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
            if (!found[i])
                continue;

            if (found[i].gameObject == gameObject)
                continue;

            wheels[index] = found[i];
            index++;
        }
    }

    protected override void Apply()
    {
        if (wheels.Length == 0)
            return;

        LookValues(out float rust, out float crack);

        for (int i = 0; i < wheels.Length; i++)
            DamageLook.Apply(wheels[i], rust, crack);
    }
}
