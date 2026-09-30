using UnityEngine;

public class WheelSpinner : MonoBehaviour
{
    public bool canGo = true;

    public Rigidbody2D[] wheels;

    public float maxWheelDistance = 1.5f;

    [Range(0f, 4f)] public float tireFriction = 2f;

    public float maxBrakeTorque = 15f;

    [Range(0.01f, 1f)] public float restSnapSpeed = 0.15f;

    public float[] gearRatios = { 1f, 0.82f, 0.68f, 0.57f, 0.48f, 0.42f };

    public float[] shiftUpSpeed = { 7f, 12f, 18f, 24f, 30f };

    public float[] shiftDownSpeed = { 4f, 7f, 11f, 14f, 18f };

    [Range(0f, 5f)] public float rampTime = 2f;

    [Range(0.05f, 1f)] public float shiftLockTime = 0.4f;
    public float[] gearSpeeds = { 8f, 14f, 20f, 26f, 32f, 38f };

    public float motorTorque = 16f;

    public bool useCustomCenterOfMass = true;

    public Vector2 centerOfMassOffset = new Vector2(0f, -0.15f);

    [Range(0f, 1f)] public float airWheelFollow = 1f;

    [Range(0.2f, 10f)] public float airStopTime = 2f;

    [Range(0, 4)] public int airLevelWheelThreshold = 2;

    [Range(0f, 12f)] public float airLevelStiffness = 3f;

    [Range(0.1f, 3f)] public float airLevelDamping = 0.9f;

    public float airLevelMaxTorque = 60f;

    public LayerMask groundMask = -1;

    public float groundCheckSkin = 0.08f;

    private Rigidbody2D body;
    private PhysicsMaterial2D tireMaterial;
    private WheelJoint2D[] joints;
    private WheelJoint2D[] allJoints;
    private System.Collections.Generic.HashSet<WheelJoint2D> claimed;
    private float wheelRadius = 0.35f;
    private int gear;
    private float shiftLock;
    private float rampTimer;
    private bool stopDetected = true;

    private void Awake()
    {
        if (wheels == null || wheels.Length == 0)
            wheels = FindWheels();

        allJoints = FindObjectsByType<WheelJoint2D>();
        claimed = new System.Collections.Generic.HashSet<WheelJoint2D>();

        joints = new WheelJoint2D[wheels.Length];

        for (int i = 0; i < wheels.Length; i++)
        {
            Rigidbody2D wheel = wheels[i];

            if (wheel == null)
                continue;

            joints[i] = wheel.GetComponent<WheelJoint2D>() ?? FindJointFor(wheel);

            if (joints[i] != null)
                claimed.Add(joints[i]);

            SetMotor(joints[i], 0f, 0f);

            if (wheel.TryGetComponent(out CircleCollider2D circle))
                circle.sharedMaterial = CreateTireMaterial();
        }

        ClaimUnconnectedJoints();

        MeasureWheelRadius();
        ResolveDriveBody();
        ApplyCenterOfMass();
    }

    private void MeasureWheelRadius()
    {
        float sum = 0f;
        int count = 0;

        foreach (Rigidbody2D wheel in wheels)
        {
            if (wheel == null)
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

    private void ClaimUnconnectedJoints()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            Rigidbody2D wheel = wheels[i];

            if (wheel == null || joints[i] != null)
                continue;

            WheelJoint2D best = null;
            float bestDistance = float.MaxValue;

            foreach (WheelJoint2D joint in allJoints)
            {
                if (claimed.Contains(joint) || !IsOwnJoint(joint))
                    continue;

                float distance = Vector2.Distance(joint.transform.position, wheel.position);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = joint;
                }
            }

            if (best == null)
                continue;

            joints[i] = best;
            claimed.Add(best);
            SetMotor(best, 0f, 0f);
        }
    }

    private bool IsOwnJoint(WheelJoint2D joint)
    {
        Transform jointTransform = joint.transform;

        return jointTransform == transform
            || jointTransform.IsChildOf(transform)
            || transform.IsChildOf(jointTransform);
    }

    private void ResolveDriveBody()
    {
        foreach (WheelJoint2D joint in joints)
        {
            if (joint == null)
                continue;

            Rigidbody2D jointBody = joint.GetComponent<Rigidbody2D>();

            body = jointBody;
            return;
        }

        body = GetComponent<Rigidbody2D>();
    }

    private WheelJoint2D FindJointFor(Rigidbody2D wheel)
    {
        foreach (WheelJoint2D joint in allJoints)
        {
            if (joint.connectedBody != wheel || claimed.Contains(joint))
                continue;

            return joint;
        }

        return null;
    }

    private static void SetMotor(WheelJoint2D joint, float motorSpeed, float maxMotorTorque)
    {
        if (joint == null)
            return;

        joint.motor = new JointMotor2D
        {
            motorSpeed = motorSpeed,
            maxMotorTorque = maxMotorTorque
        };
    }

    private float SpeedForGear()
    {
        if (gearSpeeds == null || gearSpeeds.Length == 0)
            return 10f;

        return gearSpeeds[Mathf.Clamp(gear, 0, gearSpeeds.Length - 1)];
    }

    private float MotorSpeedForGear()
    {
        return -SpeedForGear() / Mathf.Max(wheelRadius, 0.05f);
    }

    private Rigidbody2D[] FindWheels()
    {
        var found = new System.Collections.Generic.List<Rigidbody2D>();

        foreach (CircleCollider2D circle in FindObjectsByType<CircleCollider2D>())
        {
            Rigidbody2D candidate = circle.attachedRigidbody;

            if (!IsOwnWheel(candidate))
                continue;

            if (!found.Contains(candidate))
                found.Add(candidate);
        }

        if (found.Count == 0)
        {
            foreach (CircleCollider2D circle in FindObjectsByType<CircleCollider2D>())
            {
                Rigidbody2D candidate = circle.attachedRigidbody;

                if (!IsOwnWheel(candidate))
                    continue;

                if (Vector2.Distance(candidate.position, transform.position) > maxWheelDistance)
                    continue;

                if (!found.Contains(candidate))
                    found.Add(candidate);
            }
        }

        return found.ToArray();
    }

    private bool IsOwnWheel(Rigidbody2D candidate)
    {
        if (candidate == null)
            return false;

        if (!candidate.transform.IsChildOf(transform))
            return false;

        foreach (WheelJoint2D joint in FindObjectsByType<WheelJoint2D>())
        {
            if (joint.GetComponent<Rigidbody2D>() == candidate)
                return false;
        }

        return true;
    }

    private PhysicsMaterial2D CreateTireMaterial()
    {
        if (tireMaterial == null)
        {
            tireMaterial = new PhysicsMaterial2D("Tire")
            {
                friction = tireFriction,
                bounciness = 0f,
                frictionCombine = PhysicsMaterialCombine2D.Maximum
            };
        }

        return tireMaterial;
    }

    private void FixedUpdate()
    {
        UpdateRamp();

        if (shiftLock > 0f)
            shiftLock -= Time.fixedDeltaTime;

        UpdateGearbox();

        ApplySelfRighting();

        float throttle = ThrottleFactor();

        for (int i = 0; i < wheels.Length; i++)
        {
            Rigidbody2D wheel = wheels[i];
            WheelJoint2D joint = joints != null && i < joints.Length ? joints[i] : null;

            if (wheel == null)
                continue;

            if (!IsWheelGrounded(wheel))
            {
                SetMotor(joint, 0f, 0f);

                ApplyAirDrag(wheel);

                wheel.angularVelocity += body.angularVelocity * airWheelFollow;
                continue;
            }

            if (!canGo)
            {
                if (Mathf.Abs(wheel.angularVelocity) < restSnapSpeed)
                {
                    SetMotor(joint, 0f, 0f);
                    wheel.angularVelocity = 0f;
                }
                else
                {
                    SetMotor(joint, 0f, maxBrakeTorque);
                }
            }
            else
            {
                SetMotor(joint, MotorSpeedForGear(), motorTorque * CurrentRatio() * throttle);
            }
        }
    }

    private void ApplyCenterOfMass()
    {
        if (!useCustomCenterOfMass || body == null)
            return;

        body.centerOfMass = DefaultCenterOfMass() + centerOfMassOffset;
    }

    private Vector2 DefaultCenterOfMass()
    {
        foreach (Collider2D collider in body.GetComponents<Collider2D>())
        {
            if (collider is CapsuleCollider2D capsule)
                return capsule.offset;
        }

        return body.centerOfMass;
    }

    private int CountAirborneWheels()
    {
        int airborne = 0;

        foreach (Rigidbody2D wheel in wheels)
        {
            if (wheel != null && !IsWheelGrounded(wheel))
                airborne++;
        }

        return airborne;
    }

    private void ApplySelfRighting()
    {
        if (body == null || airLevelStiffness <= 0f)
            return;

        if (CountAirborneWheels() < airLevelWheelThreshold)
            return;

        float inertia = body.inertia > 0f ? body.inertia : Mathf.Max(body.mass, 0.01f);

        float angle = Mathf.DeltaAngle(0f, body.rotation * Mathf.Rad2Deg) * Mathf.Deg2Rad;
        float frequency = airLevelStiffness;

        float desiredAcceleration =
            -angle * frequency * frequency
            -body.angularVelocity * 2f * airLevelDamping * frequency;

        body.AddTorque(Mathf.Clamp(inertia * desiredAcceleration, -airLevelMaxTorque, airLevelMaxTorque));
    }

    private void ApplyAirDrag(Rigidbody2D wheel)
    {
        float inertia = wheel.inertia > 0f ? wheel.inertia : Mathf.Max(wheel.mass, 0.01f);
        float response = inertia / Mathf.Max(airStopTime, 0.05f);

        wheel.AddTorque(-wheel.angularVelocity * response);
    }

    private float CurrentRatio()
    {
        if (gearRatios == null || gearRatios.Length == 0)
            return 1f;

        return gearRatios[Mathf.Clamp(gear, 0, gearRatios.Length - 1)];
    }

    private int GearCount => gearRatios != null ? gearRatios.Length : 0;

    private void UpdateGearbox()
    {
        int count = GearCount;
        if (count == 0)
            return;

        if (shiftLock > 0f)
            return;

        float forwardSpeed = ForwardSpeed();

        if (canGo && gear < count - 1 && forwardSpeed > ThresholdAt(shiftUpSpeed, gear))
            ShiftTo(gear + 1);

        if (gear > 0 && forwardSpeed < ThresholdAt(shiftDownSpeed, gear - 1))
            ShiftTo(gear - 1);
    }

    private void ShiftTo(int newGear)
    {
        gear = newGear;
        shiftLock = shiftLockTime;

        rampTimer = 0f;
    }

    private void UpdateRamp()
    {
        if (rampTime <= 0f)
            return;

        if (!canGo)
        {
            stopDetected = true;
            return;
        }

        if (stopDetected)
        {
            rampTimer = 0f;
            stopDetected = false;
        }

        rampTimer = Mathf.Min(rampTimer + Time.fixedDeltaTime, rampTime);
    }

    private float ThrottleFactor()
    {
        if (rampTime <= 0f)
            return 1f;

        return Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(rampTimer / rampTime));
    }

    private float ForwardSpeed()
    {
        float sum = 0f;
        int count = 0;

        foreach (Rigidbody2D wheel in wheels)
        {
            if (wheel == null)
                continue;

            sum += wheel.angularVelocity;
            count++;
        }

        return count == 0 ? 0f : -sum / count * wheelRadius;
    }

    public bool IsGrounded()
    {
        foreach (Rigidbody2D wheel in wheels)
        {
            if (wheel != null && IsWheelGrounded(wheel))
                return true;
        }

        return false;
    }

    private bool IsWheelGrounded(Rigidbody2D wheel)
    {
        float radius = WheelRadius(wheel);

        if (radius <= 0f)
            return false;

        Vector2 origin = wheel.position + Vector2.down * (radius * 0.5f);
        float distance = radius * 0.5f + groundCheckSkin;

        return Physics2D.Raycast(origin, Vector2.down, distance, groundMask).collider != null;
    }

    private static float WheelRadius(Rigidbody2D wheel)
    {
        if (!wheel.TryGetComponent(out CircleCollider2D circle))
            return 0f;

        return circle.radius * Mathf.Abs(wheel.transform.lossyScale.x);
    }

    private static float ThresholdAt(float[] thresholds, int index)
    {
        if (thresholds == null || index < 0 || index >= thresholds.Length)
            return float.MaxValue;

        return thresholds[index];
    }
}