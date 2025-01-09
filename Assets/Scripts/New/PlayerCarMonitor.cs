using UnityEngine;

public class PlayerCarMonitor : MonoBehaviour
{
    public float speedLimit = 100f; // Speed limit for triggering police spawn
    private Rigidbody playerRigidbody;

    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float currentSpeed = playerRigidbody.velocity.magnitude * 3.6f; // Convert m/s to km/h

        if (currentSpeed > speedLimit)
        {
           // GameHandler.Instance.TriggerPoliceSpawn(transform.position); // Notify GameManager
        }
    }
}
