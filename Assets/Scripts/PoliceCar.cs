using UnityEngine;

public class PoliceCar : MonoBehaviour
{
    [Header("Car Components")]
    public Rigidbody rb;
    public Transform targetPlayer;

    [Header("AI Settings")]
    public float maxSpeed = 250f;          // Maximum speed
    public float acceleration = 100f;     // Acceleration force
    public float turnSpeed = 5f;          // Turn responsiveness
    public float brakeForce = 3000f;      // Braking force
    public float detectionRadius = 50f;   // Radius to detect player
    public float interceptionMultiplier = 1.5f; // Aggressiveness in interception

    private float currentSpeed;
    private bool isIntercepting;

    public void Initialize(GameObject playerCar)
    {
        rb = GetComponent<Rigidbody>();
        targetPlayer = playerCar.transform;
    }

    private void FixedUpdate()
    {
        if (targetPlayer == null) return;

        PursuePlayer();
        HandleSteering();
        DynamicBraking();
    }

    private void PursuePlayer()
    {
        // Calculate current speed in km/h
        currentSpeed = rb.velocity.magnitude * 3.6f;

        if (currentSpeed < maxSpeed)
        {
            // Apply forward force for acceleration
            Vector3 forwardForce = transform.forward * acceleration * Time.fixedDeltaTime;
            rb.AddForce(forwardForce, ForceMode.Acceleration);
        }
    }

    private void HandleSteering()
    {
        if (targetPlayer == null) return;

        Vector3 targetPosition = CalculateInterceptionPoint();
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;

        // Smoothly rotate toward the target
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * turnSpeed);
    }

    private Vector3 CalculateInterceptionPoint()
    {
        // Predict player's future position for interception
        Vector3 playerVelocity = targetPlayer.GetComponent<Rigidbody>().velocity;
        Vector3 predictedPosition = targetPlayer.position + playerVelocity * interceptionMultiplier;
        return predictedPosition;
    }

    private void DynamicBraking()
    {
        if (targetPlayer == null) return;

        // Check distance and relative position to the player
        float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);
        Vector3 relativeVelocity = transform.InverseTransformDirection(rb.velocity);

        // Apply brakes if overshooting
        if (distanceToPlayer < detectionRadius && relativeVelocity.z > 0)
        {
            rb.AddForce(-transform.forward * brakeForce * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Logic to handle collision with the player
            Debug.Log("Police collided with Player!");
        }
    }
}
