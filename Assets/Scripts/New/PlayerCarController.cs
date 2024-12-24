using UnityEngine;

public class PlayerCarController : MonoBehaviour
{
    // Wheel Colliders
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    // Wheel Transforms
    public Transform frontLeftTransform;
    public Transform frontRightTransform;
    public Transform rearLeftTransform;
    public Transform rearRightTransform;

    // Car properties
    [Header("Car Properties")]
    public float motorForce = 1500f;
    public float brakeForce = 7000f;
    public float maxSpeed = 200f;
    public float decelerationRate = 100f;

    [Header("Traction Settings")]
    public float gripFactor = 2.0f; // Sideways friction multiplier for better grip
    public float tractionControlThreshold = 10f; // Max allowable wheel slip

    [Header("Rigidbody Settings")]
    public float mass = 1200f;
    public float drag = 0.1f; // Small drag to naturally reduce speed
    public Vector3 centerOfMass = new Vector3(0, -0.5f, 0);

    private Rigidbody rb;
    private float currentSpeed;
    private float currentSteerAngle;

    private bool isBraking = false;

    // Audio
    private AudioSource engineSound;

    void Start()
    {
        // Setup Rigidbody
        rb = GetComponent<Rigidbody>();
        rb.mass = mass;
        rb.drag = drag;
        rb.centerOfMass = centerOfMass;

        // Setup Audio
        engineSound = GetComponent<AudioSource>();
        if (engineSound == null)
        {
            Debug.LogError("No AudioSource found on the car.");
        }

        // Initialize friction to prevent slipping
        SetWheelFriction(gripFactor);
    }

    void Update()
    {
        // Update wheel positions and rotations
        UpdateWheelPositions(frontLeftWheel, frontLeftTransform);
        UpdateWheelPositions(frontRightWheel, frontRightTransform);
        UpdateWheelPositions(rearLeftWheel, rearLeftTransform);
        UpdateWheelPositions(rearRightWheel, rearRightTransform);

        // Update engine sound
        UpdateEngineSound();
    }

    void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        HandleBraking();
        ApplyTractionControl();
    }

    void HandleMotor()
    {
        float acceleration = Input.GetAxis("Vertical"); // W/S or Up/Down Arrow

        if (currentSpeed < maxSpeed)
        {
            float torque = motorForce * acceleration;
            rearLeftWheel.motorTorque = torque;
            rearRightWheel.motorTorque = torque;
        }
        else
        {
            rearLeftWheel.motorTorque = 0f;
            rearRightWheel.motorTorque = 0f;
        }

        currentSpeed = rb.velocity.magnitude * 3.6f; // Convert m/s to km/h
    }

    void HandleSteering()
    {
        float steeringInput = Input.GetAxis("Horizontal");
        currentSteerAngle = steeringInput * 35f; // 35 degrees max steering angle
        frontLeftWheel.steerAngle = currentSteerAngle;
        frontRightWheel.steerAngle = currentSteerAngle;
    }

    void HandleBraking()
    {
        isBraking = Input.GetKey(KeyCode.Space);

        if (isBraking)
        {
            rearLeftWheel.brakeTorque = brakeForce;
            rearRightWheel.brakeTorque = brakeForce;
            frontLeftWheel.brakeTorque = brakeForce;
            frontRightWheel.brakeTorque = brakeForce;

            // Stop the car when speed is near zero
            if (currentSpeed <= 5f)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        else
        {
            rearLeftWheel.brakeTorque = 0f;
            rearRightWheel.brakeTorque = 0f;
            frontLeftWheel.brakeTorque = 0f;
            frontRightWheel.brakeTorque = 0f;
        }
    }

    void ApplyTractionControl()
    {
        // Limit torque to prevent wheel slipping
        foreach (WheelCollider wheel in new[] { rearLeftWheel, rearRightWheel })
        {
            WheelHit hit;
            if (wheel.GetGroundHit(out hit))
            {
                if (Mathf.Abs(hit.forwardSlip) > tractionControlThreshold)
                {
                    wheel.motorTorque *= 0.5f; // Reduce torque if wheels are slipping
                }
            }
        }
    }

    void SetWheelFriction(float grip)
    {
        // Configure friction for all wheels
        AdjustWheelFriction(frontLeftWheel, grip);
        AdjustWheelFriction(frontRightWheel, grip);
        AdjustWheelFriction(rearLeftWheel, grip);
        AdjustWheelFriction(rearRightWheel, grip);
    }

    void AdjustWheelFriction(WheelCollider wheel, float stiffness)
    {
        WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;
        sidewaysFriction.stiffness = stiffness;
        wheel.sidewaysFriction = sidewaysFriction;

        WheelFrictionCurve forwardFriction = wheel.forwardFriction;
        forwardFriction.stiffness = stiffness;
        wheel.forwardFriction = forwardFriction;
    }

    void UpdateWheelPositions(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 position;
        Quaternion rotation;
        wheelCollider.GetWorldPose(out position, out rotation);

        wheelTransform.position = position;

        // Rotate tires based on speed
        float rotationSpeed = currentSpeed * (wheelCollider.motorTorque >= 0 ? 1 : -1);
        wheelTransform.Rotate(rotationSpeed * Time.deltaTime * 360f, 0, 0, Space.Self);

        // Update Y-axis rotation for turning
        if (wheelCollider == frontLeftWheel || wheelCollider == frontRightWheel)
        {
            wheelTransform.localRotation = Quaternion.Euler(
                wheelTransform.localRotation.eulerAngles.x,
                currentSteerAngle,
                0 // Lock Z-axis to 0
            );
        }
    }

    void UpdateEngineSound()
    {
        if (engineSound != null)
        {
            engineSound.pitch = Mathf.Lerp(1f, 3f, currentSpeed / maxSpeed); // Dynamic pitch based on speed
            if (!engineSound.isPlaying)
            {
                engineSound.Play();
            }
        }
    }
}
