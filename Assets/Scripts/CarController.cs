using UnityEngine;

public class CarController : MonoBehaviour
{
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

    [Header("Car Settings")]
    public float maxMotorTorque = 500f;  // Maximum torque for acceleration
    public float maxSteerAngle = 25f;   // Sportier steering angle
    public float brakeForce = 1000f;    // Stronger braking force
    public float jumpForce = 500f;      // Jumping force
    public float topSpeed = 200f;       // Maximum speed (in km/h)
    public Rigidbody rb;                // Rigidbody for physics

    [Header("Performance Settings")]
    public float accelerationRate = 10f; // Rate at which car accelerates
    public float decelerationRate = 5f;  // Rate at which car slows down
    public float driftStabilizer = 1.2f; // Lateral force to prevent sliding
    public float antiRollForce = 6000f;  // Stronger anti-roll bar for sport handling

    private float motorInput;
    private float steerInput;
    private float brakeInput;
    private float currentSpeed;          // Current speed of the car
    private float currentMotorTorque;    // Smoothly adjusted motor torque

    private void Start()
    {
        // Ensure Rigidbody is assigned
        if (!rb)
        {
            rb = GetComponent<Rigidbody>();
        }
        rb.centerOfMass = new Vector3(0, -0.5f, 0); // Lower center of mass for stability
    }

    private void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        ApplyAntiRollBar();
        UpdateWheelPoses();
        ApplyDriftStabilizer();
    }

    private void Update()
    {
        HandleInput();
        HandleJump();
    }

    private void HandleInput()
    {
        motorInput = Input.GetAxis("Vertical"); // Forward/Backward
        steerInput = Input.GetAxis("Horizontal"); // Left/Right
        brakeInput = Input.GetKey(KeyCode.Space) ? brakeForce : 0f; // Brake
    }

    private void HandleMotor()
    {
        // Calculate current speed in km/h
        currentSpeed = rb.velocity.magnitude * 3.6f;

        // Limit speed to topSpeed
        if (currentSpeed < topSpeed)
        {
            // Gradual acceleration
            currentMotorTorque = Mathf.Lerp(currentMotorTorque, motorInput * maxMotorTorque, Time.fixedDeltaTime * accelerationRate);
        }
        else
        {
            currentMotorTorque = 0f; // Stop accelerating beyond top speed
        }

        // Apply torque to rear wheels
        wheelRL.motorTorque = currentMotorTorque;
        wheelRR.motorTorque = currentMotorTorque;

        // Apply brake force
        wheelRL.brakeTorque = brakeInput;
        wheelRR.brakeTorque = brakeInput;
        wheelFL.brakeTorque = brakeInput;
        wheelFR.brakeTorque = brakeInput;

        // Gradual deceleration
        if (motorInput == 0)
        {
            currentMotorTorque = Mathf.Lerp(currentMotorTorque, 0, Time.fixedDeltaTime * decelerationRate);
        }
    }

    private void HandleSteering()
    {
        // Steer front wheels with a smooth transition
        float steerAngle = steerInput * maxSteerAngle;
        wheelFL.steerAngle = steerAngle;
        wheelFR.steerAngle = steerAngle;
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
        float antiRoll = (travelL - travelR) * this.antiRollForce;

        if (groundedL)
        {
            rb.AddForceAtPosition(wheelL.transform.up * -antiRoll, wheelL.transform.position);
        }
        if (groundedR)
        {
            rb.AddForceAtPosition(wheelR.transform.up * antiRoll, wheelR.transform.position);
        }
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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

    private void ApplyDriftStabilizer()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);

        if (localVelocity.x > 0.5f || localVelocity.x < -0.5f)
        {
            Vector3 counterForce = -transform.right * localVelocity.x * driftStabilizer;
            rb.AddForce(counterForce, ForceMode.Acceleration);
        }
    }
}
