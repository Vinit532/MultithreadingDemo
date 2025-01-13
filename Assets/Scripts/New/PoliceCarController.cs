using System.Collections;
using UnityEngine;

public class PoliceCarController : MonoBehaviour
{
    public Transform playerCar; // Reference to the PlayerCar
    public float maxSpeed = 250f; // PoliceCar is faster than PlayerCar
    public float acceleration = 2000f;
    public float stoppingDistance = 5f; // Distance at which PoliceCar interrupts PlayerCar
    public float pushForce = 5000f; // Force to push PlayerCar

    // WheelColliders
    public WheelCollider frontLeftWheel;
    public WheelCollider frontRightWheel;
    public WheelCollider rearLeftWheel;
    public WheelCollider rearRightWheel;

    // Rigidbody for physics-based movement
    private Rigidbody rb;

    private float currentSpeed;
    private bool isReversing = false; // Track if the car is reversing
    private bool isStuck = false; // Track if the car is stuck

    private Vector3 lastPosition; // To check if the car is stuck
    private float stuckCheckInterval = 2f; // Time interval to check for being stuck
    private float stuckDistanceThreshold = 0.5f; // Threshold to determine if the car is stuck

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 1500f;
        rb.centerOfMass = new Vector3(0, -0.5f, 0);

        // Initialize position tracking for stuck detection
        lastPosition = transform.position;
        InvokeRepeating(nameof(CheckIfStuck), stuckCheckInterval, stuckCheckInterval);
    }

    void FixedUpdate()
    {
        if (playerCar != null && !isStuck)
        {
            ChasePlayer();
        }
    }

    void ChasePlayer()
    {
        Vector3 direction = (playerCar.position - transform.position).normalized;

        // Steer towards PlayerCar
        float targetSteerAngle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
        frontLeftWheel.steerAngle = targetSteerAngle / 2; // Dividing to make turning smooth
        frontRightWheel.steerAngle = targetSteerAngle / 2;

        // Accelerate towards PlayerCar
        currentSpeed = rb.velocity.magnitude * 3.6f; // Convert m/s to km/h
        if (currentSpeed < maxSpeed)
        {
            rearLeftWheel.motorTorque = acceleration;
            rearRightWheel.motorTorque = acceleration;
        }

        // Check distance to stop PlayerCar
        float distanceToPlayer = Vector3.Distance(transform.position, playerCar.position);
        if (distanceToPlayer <= stoppingDistance)
        {
            InterruptPlayer();
        }

        // Check if the car needs to brake and reverse
        if (IsDodged())
        {
            StartCoroutine(BrakeAndReverse());
        }
    }

    void InterruptPlayer()
    {
        // Apply a push force to PlayerCar
        Rigidbody playerRb = playerCar.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            Vector3 pushDirection = (playerCar.position - transform.position).normalized;
            playerRb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
        }
    }

    private bool IsDodged()
    {
        // Check if the car has been dodged by the PlayerCar
        Vector3 toPlayer = (playerCar.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, toPlayer);
        return angle > 45f; // Dodged if angle is greater than 45 degrees
    }

    private IEnumerator BrakeAndReverse()
    {
        if (isReversing) yield break; // Prevent multiple reverses
        isReversing = true;

        rearLeftWheel.motorTorque = 0;
        rearRightWheel.motorTorque = 0;

        // Briefly apply brakes
        rearLeftWheel.brakeTorque = 3000f;
        rearRightWheel.brakeTorque = 3000f;
        yield return new WaitForSeconds(0.5f);

        // Release brakes and reverse
        rearLeftWheel.brakeTorque = 0;
        rearRightWheel.brakeTorque = 0;

        rearLeftWheel.motorTorque = -acceleration;
        rearRightWheel.motorTorque = -acceleration;
        yield return new WaitForSeconds(1.5f); // Reverse for a short duration

        // Stop reversing and resume chasing
        rearLeftWheel.motorTorque = 0;
        rearRightWheel.motorTorque = 0;
        isReversing = false;
    }

    private void CheckIfStuck()
    {
        // Determine if the PoliceCar is stuck (not making forward progress)
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        if (distanceMoved < stuckDistanceThreshold && !isStuck)
        {
            StartCoroutine(EscapeStuck());
        }
        else
        {
            lastPosition = transform.position;
        }
    }

    private IEnumerator EscapeStuck()
    {
        isStuck = true;

        // Stop current movement
        rearLeftWheel.motorTorque = 0;
        rearRightWheel.motorTorque = 0;

        // Reverse slightly to escape
        rearLeftWheel.motorTorque = -acceleration;
        rearRightWheel.motorTorque = -acceleration;
        yield return new WaitForSeconds(1.5f);

        // Rotate in a random direction
        float randomTurn = Random.Range(-30f, 30f);
        frontLeftWheel.steerAngle = randomTurn;
        frontRightWheel.steerAngle = randomTurn;
        yield return new WaitForSeconds(1f);

        // Resume chasing
        frontLeftWheel.steerAngle = 0;
        frontRightWheel.steerAngle = 0;
        rearLeftWheel.motorTorque = acceleration;
        rearRightWheel.motorTorque = acceleration;

        isStuck = false;
    }
}
