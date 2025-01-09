using UnityEngine;
using System.Collections;
using System.Threading;

public class GameHandler : MonoBehaviour
{
    public GameObject policeCarPrefab; // Reference to the PoliceCar prefab
    public Transform playerCar; // Reference to the PlayerCar
    public float spawnRadius = 50f; // Radius around PlayerCar to spawn PoliceCars
    public float speedThreshold = 120f; // Speed limit to trigger PoliceCar spawn
    public float spawnInterval = 20f; // Interval to spawn additional PoliceCars

    private Rigidbody playerRb; // Rigidbody reference for PlayerCar
    private bool isPoliceChasing = false;

    void Start()
    {
        // Cache PlayerCar's Rigidbody on the main thread
        playerRb = playerCar.GetComponent<Rigidbody>();

        // Start monitoring PlayerCar's speed on a separate thread
        Thread monitorThread = new Thread(MonitorPlayerSpeed);
        monitorThread.Start();
    }

    void MonitorPlayerSpeed()
    {
        while (true)
        {
            if (playerRb == null)
            {
                Debug.LogError("PlayerCar's Rigidbody is not assigned!");
                return;
            }

            // Get PlayerCar's speed (This must be done on the main thread)
            float playerSpeed = 0f;
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                playerSpeed = playerRb.velocity.magnitude * 3.6f; // Convert m/s to km/h
            });

            // Allow the thread to wait briefly for the result
            Thread.Sleep(50); // Small delay to avoid excessive CPU usage

            // Trigger police chase if speed exceeds the threshold
            if (playerSpeed > speedThreshold && !isPoliceChasing)
            {
                isPoliceChasing = true;

                // Spawn initial PoliceCar and start periodic spawning
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    SpawnPoliceCar();
                    StartCoroutine(SpawnAdditionalPoliceCars());
                });
            }

            Thread.Sleep(100); // Check speed every 100ms
        }
    }

    void SpawnPoliceCar()
    {
        Vector3 spawnPosition = playerCar.position + Random.insideUnitSphere * spawnRadius;
        spawnPosition.y = playerCar.position.y; // Match height with PlayerCar

        GameObject newPoliceCar = Instantiate(policeCarPrefab, spawnPosition, Quaternion.identity);
        PoliceCarController controller = newPoliceCar.GetComponent<PoliceCarController>();
        if (controller != null)
        {
            controller.playerCar = playerCar; // Assign PlayerCar for chasing
        }
    }

    IEnumerator SpawnAdditionalPoliceCars()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnPoliceCar();
        }
    }
}
