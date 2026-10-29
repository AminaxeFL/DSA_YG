using UnityEngine;

public class WheelSpinner : MonoBehaviour
{
    public bool canGo = true;
    [Min(0f)] public float maxSpeed = 10f;
    [Range(0.01f, 10f)] public float accelerationTime = 1f;
    [Range(0.01f, 30f)] public float airStopTime = 3f;
    public LayerMask groundMask = -1;
    public float groundCheckDistance = 0.2f;
    [Min(0f)] public float fuel = 100f;
    [Min(0f)] public float maxFuel = 100f;
    [Min(0.1f)] public float fuelTime = 20f;

    private Rigidbody2D body;
    private WheelJoint2D[] joints = new WheelJoint2D[0];
    private float wheelRadius = 0.35f;
    private float targetMotorSpeed;
    private float currentMotorSpeed;
    private float fuelDrain;
    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[4];

    private void Awake()
    {
        joints = FindObjectsByType<WheelJoint2D>();

        foreach (WheelJoint2D joint in joints)
        {
            SetMotor(joint, 0f, 0f);
        }

        MeasureWheelRadius();
        ResolveDriveBody();
        targetMotorSpeed = 0f;
        currentMotorSpeed = 0f;
    }

    private void MeasureWheelRadius()
    {
        float sum = 0f;
        int count = 0;

        foreach (WheelJoint2D joint in joints)
        {
            if (!joint)
                continue;

            Rigidbody2D wheel = joint.connectedBody;

            if (!wheel)
                continue;

            float radius = WheelRadius(wheel);

            if (radius <= 0f)
                continue;

            sum += radius;
            count++;
        }

        if (count > 0)
            wheelRadius = sum / count;
    }

    private void ResolveDriveBody()
    {
        foreach (WheelJoint2D joint in joints)
        {
            if (!joint)
                continue;

            body = joint.GetComponent<Rigidbody2D>();
            return;
        }

        body = GetComponent<Rigidbody2D>();
    }

    private static void SetMotor(WheelJoint2D joint, float motorSpeed, float maxMotorTorque)
    {
        if (!joint)
            return;

        joint.motor = new JointMotor2D
        {
            motorSpeed = motorSpeed,
            maxMotorTorque = maxMotorTorque
        };
    }

    private float MaxMotorSpeed()
    {
        return -maxSpeed / Mathf.Max(wheelRadius, 0.05f);
    }

    private int CountGrounded()
    {
        int count = 0;

        foreach (WheelJoint2D joint in joints)
        {
            if (!joint)
                continue;

            Rigidbody2D wheel = joint.connectedBody;

            if (!wheel)
                continue;

            float radius = WheelRadius(wheel);
            Vector2 origin = wheel.position + Vector2.down * radius * 0.8f;
            float distance = groundCheckDistance + radius * 0.2f;
            int hitCount = Physics2D.RaycastNonAlloc(origin, Vector2.down, groundHits, distance, groundMask);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hit = groundHits[i].collider;

                if (!hit)
                    continue;

                Rigidbody2D hitBody = hit.attachedRigidbody;

                if (hitBody == wheel)
                    continue;

                if (hitBody == body)
                    continue;

                if (hitBody && hitBody.transform.IsChildOf(transform))
                    continue;

                count++;
                break;
            }
        }

        return count;
    }

    private void FixedUpdate()
    {
        if (joints.Length == 0)
            return;

        int grounded = CountGrounded();
        bool airStop = grounded < 2;

        bool hasFuel = fuel > 0f;

        if (canGo && maxSpeed > 0f && !airStop && hasFuel)
        {
            targetMotorSpeed = MaxMotorSpeed();
        }
        else
        {
            targetMotorSpeed = 0f;
        }

        float accelTime = accelerationTime;

        if (Mathf.Approximately(targetMotorSpeed, 0f) && airStop && grounded < 2)
        {
            accelTime = accelerationTime * 3f;

            if (accelTime < 0.01f)
                accelTime = 0.01f;
        }

        float accel = accelTime > 0f ? Mathf.Abs(MaxMotorSpeed() - currentMotorSpeed) / accelTime : float.PositiveInfinity;

        if (Mathf.Approximately(targetMotorSpeed, 0f))
        {
            accel = accelTime > 0f ? Mathf.Abs(currentMotorSpeed) / accelTime : float.PositiveInfinity;
        }

        float delta = accel * Time.fixedDeltaTime;
        currentMotorSpeed = Mathf.MoveTowards(currentMotorSpeed, targetMotorSpeed, delta);

        foreach (WheelJoint2D joint in joints)
        {
            SetMotor(joint, currentMotorSpeed, float.PositiveInfinity);
        }

        if (hasFuel && canGo && !Mathf.Approximately(targetMotorSpeed, 0f) && maxSpeed > 0f)
        {
            float speedFactor = Mathf.Clamp01(Mathf.Abs(currentMotorSpeed) / Mathf.Abs(MaxMotorSpeed()) * 1.5f);

            if (speedFactor > 1.5f)
                speedFactor = 1.5f;

            if (fuelTime <= 0f)
                fuelDrain = 0f;
            else
                fuelDrain = 1f / fuelTime * speedFactor;

            fuel -= fuelDrain * Time.fixedDeltaTime;

            if (fuel < 0f)
                fuel = 0f;
        }
    }

    public float Speed
    {
        get
        {
            float sum = 0f;
            int count = 0;

            foreach (WheelJoint2D joint in joints)
            {
                if (!joint)
                    continue;

                Rigidbody2D wheel = joint.connectedBody;

                if (!wheel)
                    continue;

                sum += wheel.angularVelocity;
                count++;
            }

            return count == 0 ? 0f : -sum / count * wheelRadius;
        }
    }

    public float Fuel01 => maxFuel <= 0f ? 0f : Mathf.Clamp01(fuel / maxFuel);

    public Rigidbody2D Chassis => body;

    private static float WheelRadius(Rigidbody2D wheel)
    {
        if (!wheel.TryGetComponent(out CircleCollider2D circle))
            return 0f;

        return circle.radius * Mathf.Abs(wheel.transform.lossyScale.x);
    }
}
