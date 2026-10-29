using System.Collections.Generic;
using UnityEngine;

public class CarNoCollide : MonoBehaviour
{
    private static readonly Collider2D[] NoColliders = new Collider2D[0];

    [SerializeField] private bool rescanEveryFrame;
    [Range(0.05f, 5f)] [SerializeField] private float rescanInterval = 0.25f;

    private readonly List<WheelSpinner> cars = new List<WheelSpinner>();
    private readonly List<Collider2D[]> carColliders = new List<Collider2D[]>();
    private float rescanTimer;

    private void Start()
    {
        Rescan();
    }

    private void FixedUpdate()
    {
        if (rescanEveryFrame)
        {
            Rescan();
            return;
        }

        rescanTimer -= Time.fixedDeltaTime;

        if (rescanTimer <= 0f)
            Rescan();
    }

    private void Rescan()
    {
        rescanTimer = Mathf.Max(rescanInterval, 0.05f);

        cars.Clear();
        carColliders.Clear();

        WheelSpinner[] found = FindObjectsByType<WheelSpinner>();

        for (int i = 0; i < found.Length; i++)
        {
            if (!found[i])
                continue;

            cars.Add(found[i]);
            carColliders.Add(CollidersOf(found[i]));
        }

        for (int i = 0; i < cars.Count; i++)
        {
            Collider2D[] left = carColliders[i];

            if (left.Length == 0)
                continue;

            for (int j = i + 1; j < cars.Count; j++)
            {
                Collider2D[] right = carColliders[j];

                if (right.Length == 0)
                    continue;

                IgnorePair(left, right);
            }
        }
    }

    private static void IgnorePair(Collider2D[] left, Collider2D[] right)
    {
        for (int a = 0; a < left.Length; a++)
        {
            Collider2D leftCollider = left[a];

            if (!leftCollider)
                continue;

            for (int b = 0; b < right.Length; b++)
            {
                Collider2D rightCollider = right[b];

                if (rightCollider && rightCollider != leftCollider)
                    Physics2D.IgnoreCollision(leftCollider, rightCollider, true);
            }
        }
    }

    private static Collider2D[] CollidersOf(WheelSpinner car)
    {
        List<Collider2D> list = null;

        if (car.TryGetComponent(out Rigidbody2D chassis))
            list = new List<Collider2D>(chassis.GetComponents<Collider2D>());

        foreach (Rigidbody2D wheel in car.GetComponentsInChildren<Rigidbody2D>(true))
        {
            if (!wheel)
                continue;

            if (!wheel.GetComponent<WheelJoint2D>())
                continue;

            list ??= new List<Collider2D>();

            list.AddRange(wheel.GetComponents<Collider2D>());
        }

        return list?.ToArray() ?? NoColliders;
    }
}
