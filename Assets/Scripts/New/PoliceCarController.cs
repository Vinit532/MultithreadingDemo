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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 1500f;
        rb.centerOfMass = new Vector3(0, -0.5f, 0);
    }

    void FixedUpdate()
    {
        if (playerCar != null)
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
}
