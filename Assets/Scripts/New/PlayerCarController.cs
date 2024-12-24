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

    [Header("Audio Settings")]
    public AudioSource engineSound;
    public AudioClip reverseClip; // Reverse sound effect
    public AudioClip turningClip; // Turning sound effect
    public float maxPitch = 3f; // Pitch at max speed
    public float minPitch = 1f; // Pitch at idle
    public float maxVolume = 1f; // Volume at max speed
    public float minVolume = 0.3f; // Volume at idle

    private AudioSource reverseSoundSource;
    private AudioSource turningSoundSource;

    private Rigidbody rb;
    private float currentSpeed;
    private float currentSteerAngle;

    private bool isReversing = false;
    private bool isTurning = false;

    void Start()
    {
        // Initialize Rigidbody
        rb = GetComponent<Rigidbody>();

        // Initialize Engine Sound
        if (engineSound == null)
        {
            Debug.LogError("Engine sound is not assigned!");
        }
        engineSound.loop = true;
        engineSound.Play();

        // Initialize Reverse Sound
        reverseSoundSource = gameObject.AddComponent<AudioSource>();
        reverseSoundSource.clip = reverseClip;
        reverseSoundSource.loop = true;
        reverseSoundSource.volume = 0f; // Start muted
        reverseSoundSource.Play();

        // Initialize Turning Sound
        turningSoundSource = gameObject.AddComponent<AudioSource>();
        turningSoundSource.clip = turningClip;
        turningSoundSource.loop = true;
        turningSoundSource.volume = 0f; // Start muted
        turningSoundSource.Play();
    }

    void Update()
    {
        // Update wheel positions and rotations
        UpdateWheelPositions(frontLeftWheel, frontLeftTransform);
        UpdateWheelPositions(frontRightWheel, frontRightTransform);
        UpdateWheelPositions(rearLeftWheel, rearLeftTransform);
        UpdateWheelPositions(rearRightWheel, rearRightTransform);

        // Update audio based on car state
        UpdateEngineSound();
    }

    void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        HandleBraking();
    }

    void HandleMotor()
    {
        float acceleration = Input.GetAxis("Vertical"); // W/S or Up/Down Arrow
        if (currentSpeed < maxSpeed)
        {
            float torque = motorForce * acceleration;
            rearLeftWheel.motorTorque = torque;
            rearRightWheel.motorTorque = torque;

            isReversing = acceleration < 0; // Check if the car is reversing
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

        isTurning = Mathf.Abs(steeringInput) > 0.1f; // Check if turning
    }

    void HandleBraking()
    {
        bool isBraking = Input.GetKey(KeyCode.Space);

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

    void UpdateEngineSound()
    {
        // Update engine sound pitch and volume based on speed
        float normalizedSpeed = Mathf.InverseLerp(0, maxSpeed, currentSpeed); // Normalized speed (0 to 1)
        engineSound.pitch = Mathf.Lerp(minPitch, maxPitch, normalizedSpeed);
        engineSound.volume = Mathf.Lerp(minVolume, maxVolume, normalizedSpeed);

        // Update reverse sound
        reverseSoundSource.volume = isReversing ? 1f : 0f;

        // Update turning sound
        turningSoundSource.volume = isTurning ? 0.5f : 0f; // Play turning sound at half volume
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
}
