using System.Collections.Generic;
using UnityEngine;

public class CarNoCollide : MonoBehaviour
{
    private static readonly Collider2D[] NoColliders = new Collider2D[0];

    [SerializeField] private bool rescanEveryFrame;

    private readonly List<WheelSpinner> cars = new List<WheelSpinner>();

    private void Start()
    {
        Rescan();
    }

    private void FixedUpdate()
    {
        if (rescanEveryFrame)
            Rescan();
        else
            ApplyIgnores();
    }

    private void Rescan()
    {
        cars.Clear();
        cars.AddRange(FindObjectsByType<WheelSpinner>());
        ApplyIgnores();
    }

    private void ApplyIgnores()
    {
        for (int i = 0; i < cars.Count; i++)
        {
            Collider2D[] left = CollidersOf(cars[i]);
            if (left.Length == 0)
                continue;

            for (int j = i + 1; j < cars.Count; j++)
            {
                Collider2D[] right = CollidersOf(cars[j]);
                if (right.Length == 0)
                    continue;

                foreach (Collider2D a in left)
                {
                    if (a == null)
                        continue;

                    foreach (Collider2D b in right)
                    {
                        if (b != null && b != a)
                            Physics2D.IgnoreCollision(a, b, true);
                    }
                }
            }
        }
    }

    private static Collider2D[] CollidersOf(WheelSpinner car)
    {
        if (car == null)
            return NoColliders;

        List<Collider2D> list = null;

        if (car.TryGetComponent(out Rigidbody2D chassis))
            list = new List<Collider2D>(chassis.GetComponents<Collider2D>());

        if (car.wheels != null)
        {
            foreach (Rigidbody2D wheel in car.wheels)
            {
                if (wheel == null)
                    continue;

                if (list == null)
                    list = new List<Collider2D>();

                list.AddRange(wheel.GetComponents<Collider2D>());
            }
        }

        return list != null ? list.ToArray() : NoColliders;
    }
}