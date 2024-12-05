using UnityEngine;

public class PoliceCar : MonoBehaviour
{
    [Header("Police Car Settings")]
    public float maxMotorTorque = 2400f;         // Aggressive acceleration
    public float maxSteerAngle = 50f;           // Sharper steering
    public float topSpeed = 400f;               // High-speed pursuit
    public float brakeForce = 8000f;            // Disk brake for quick stops
    public float reverseTorque = 1800f;         // Powerful reverse torque
    public float pursuitAggression = 25f;       // Aggression for tighter adjustments
    public float turnSensitivity = 2.0f;        // Quick turn response
    public float rotationDamping = 4f;          // Damping for smoother rotation

    [Header("Wheel Colliders")]
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;

    [Header("Wheel Transforms")]
    public Transform wheelFLTransform;
    public Transform wheelFRTransform;
    public Transform wheelRLTransform;
    public Transform wheelRRTransform;

    private GameObject playerCar;               // Reference to the player's car
    private Rigidbody rb;                       // Rigidbody for physics
    private float currentSpeed;                 // Current speed of the police car
    private bool isReversing = false;           // Whether the car is reversing
    private bool isBraking = false;             // Whether the car is braking

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0); // Lower center of mass for stability
    }

    private void FixedUpdate()
    {
        if (playerCar != null)
        {
            PursuePlayer();
            ApplyAntiRollBar();
            UpdateWheelPoses();
        }
    }

    public void Initialize(GameObject target)
    {
        playerCar = target;
    }

    private void PursuePlayer()
    {
        Vector3 targetDirection = (playerCar.transform.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, playerCar.transform.position);
        float dotProduct = Vector3.Dot(transform.forward, targetDirection);

        // Calculate motor torque based on distance
        float distanceFactor = Mathf.Clamp01(distanceToPlayer / 200f); // Scale between 0 and 1
        float adjustedTorque = Mathf.Lerp(maxMotorTorque / 2, maxMotorTorque, distanceFactor);

        // Logic for pursuing or reversing
        if (dotProduct > 0.2f || distanceToPlayer > 20f)
        {
            isReversing = false;

            // Accelerate forward
            if (currentSpeed < topSpeed)
            {
                ApplyMotorTorque(adjustedTorque);
            }
            else
            {
                ApplyMotorTorque(0f); // Stop accelerating beyond top speed
            }

            ApplyBraking(0f); // No brakes while pursuing
        }
        else
        {
            // Reverse if overshot the player
            isReversing = true;
            ApplyMotorTorque(-reverseTorque);

            ApplyBraking(0f);
        }

        // Braking logic if player dodges the police
        if (!isReversing && distanceToPlayer < 10f && dotProduct < 0f)
        {
            ApplyBraking(brakeForce);
            ApplyMotorTorque(0f);
        }

        // Adjust steering for sharper and quicker turns
        Vector3 localTarget = transform.InverseTransformPoint(playerCar.transform.position);
        float steerAngle = Mathf.Clamp((localTarget.x / localTarget.magnitude) * maxSteerAngle * turnSensitivity, -maxSteerAngle, maxSteerAngle);
        wheelFL.steerAngle = steerAngle;
        wheelFR.steerAngle = steerAngle;

        // Update speed
        currentSpeed = rb.velocity.magnitude * 3.6f;

        // Handle sudden turns or reversing scenarios
        if (isReversing || Mathf.Abs(steerAngle) > maxSteerAngle * 0.7f)
        {
            PerformQuickTurn();
        }
    }

    private void PerformQuickTurn()
    {
        // Reduce speed for a quick turn
        rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * rotationDamping);

        // Apply a sharp rotation towards the player's position
        Vector3 targetDirection = (playerCar.transform.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * turnSensitivity));
    }

    private void ApplyMotorTorque(float torque)
    {
        wheelRL.motorTorque = torque;
        wheelRR.motorTorque = torque;
    }

    private void ApplyBraking(float brakeForce)
    {
        wheelFL.brakeTorque = brakeForce;
        wheelFR.brakeTorque = brakeForce;
        wheelRL.brakeTorque = brakeForce;
        wheelRR.brakeTorque = brakeForce;

        // Simulate braking shock absorbers
        if (brakeForce > 0f && !isBraking)
        {
            rb.AddForce(-transform.forward * brakeForce * 0.05f, ForceMode.Impulse);
            isBraking = true;
        }
        else if (brakeForce == 0f)
        {
            isBraking = false;
        }
    }

    private void ApplyAntiRollBar()
    {
        ApplyAntiRoll(wheelFL, wheelFR);
        ApplyAntiRoll(wheelRL, wheelRR);
    }

    private void ApplyAntiRoll(WheelCollider wheelL, WheelCollider wheelR)
    {
        WheelHit hit;
        float travelL = 1.0f, travelR = 1.0f;

        // Check if left wheel is grounded
        bool groundedL = wheelL.GetGroundHit(out hit);
        if (groundedL)
        {
            travelL = Mathf.Clamp01((-wheelL.transform.InverseTransformPoint(hit.point).y - wheelL.radius) /
                                     Mathf.Max(wheelL.suspensionDistance, 0.001f));
        }

        // Check if right wheel is grounded
        bool groundedR = wheelR.GetGroundHit(out hit);
        if (groundedR)
        {
            travelR = Mathf.Clamp01((-wheelR.transform.InverseTransformPoint(hit.point).y - wheelR.radius) /
                                     Mathf.Max(wheelR.suspensionDistance, 0.001f));
        }

        // Calculate anti-roll force
        float antiRoll = (travelL - travelR) * 8000f;

        if (groundedL)
        {
            rb.AddForceAtPosition(wheelL.transform.up * -antiRoll, wheelL.transform.position);
        }
        if (groundedR)
        {
            rb.AddForceAtPosition(wheelR.transform.up * antiRoll, wheelR.transform.position);
        }
    }

    private void UpdateWheelPoses()
    {
        UpdateWheelPose(wheelFL, wheelFLTransform);
        UpdateWheelPose(wheelFR, wheelFRTransform);
        UpdateWheelPose(wheelRL, wheelRLTransform);
        UpdateWheelPose(wheelRR, wheelRRTransform);
    }

    private void UpdateWheelPose(WheelCollider collider, Transform trans)
    {
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);

        trans.position = pos;
        trans.rotation = rot;
    }
}
