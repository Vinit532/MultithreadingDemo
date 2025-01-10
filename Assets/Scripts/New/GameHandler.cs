using UnityEngine;
using System.Collections;
using System.Threading;
using UnityEngine.AI; // Required for NavMesh operations

public class GameHandler : MonoBehaviour
{
    public GameObject policeCarPrefab; // Reference to the PoliceCar prefab
    public Transform playerCar; // Reference to the PlayerCar
    public float spawnRadius = 50f; // Radius around PlayerCar to spawn PoliceCars
    public float speedThreshold = 120f; // Speed limit to trigger PoliceCar spawn
    public float spawnInterval = 20f; // Interval to spawn additional PoliceCars

    private Rigidbody playerRb; // Rigidbody reference for PlayerCar
    private bool isPoliceChasing = false;

    private bool isGameRunning = true; // Flag to track if the game is running


    void Start()
    {
        isGameRunning = true; // Game is running
        playerRb = playerCar.GetComponent<Rigidbody>();

        // Ensure UnityMainThreadDispatcher is initialized
        if (UnityMainThreadDispatcher.Instance() == null)
        {
            Debug.LogError("UnityMainThreadDispatcher is not initialized in the scene.");
        }

        // Start monitoring PlayerCar's speed on a separate thread
        Thread monitorThread = new Thread(MonitorPlayerSpeed);
        monitorThread.Start();
    }


    void MonitorPlayerSpeed()
    {
        while (isGameRunning) // Check if the game is running
        {
            if (playerRb == null)
            {
                Debug.LogError("PlayerCar's Rigidbody is not assigned!");
                return;
            }

            float playerSpeed = 0f;

            // Enqueue speed calculation to the main thread
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                playerSpeed = playerRb.velocity.magnitude * 3.6f; // Convert m/s to km/h
            });

            // Allow the thread to wait briefly for the result
            Thread.Sleep(50);

            // Trigger police chase if speed exceeds the threshold
            if (playerSpeed > speedThreshold && !isPoliceChasing)
            {
                isPoliceChasing = true;

                // Enqueue PoliceCar spawning to the main thread
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    SpawnPoliceCar();
                    StartCoroutine(SpawnAdditionalPoliceCars());
                });
            }

            Thread.Sleep(100); // Check speed every 100ms
        }

        Debug.Log("MonitorPlayerSpeed thread has exited.");
    }


    void SpawnPoliceCar()
    {
        // Generate a random position within the radius
        Vector3 randomPosition = playerCar.position + Random.insideUnitSphere * spawnRadius;
        randomPosition.y = playerCar.position.y; // Match height with PlayerCar

        // Adjust the position to be on the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPosition, out hit, spawnRadius, NavMesh.AllAreas))
        {
            // Spawn PoliceCar at the nearest valid position on the NavMesh
            GameObject newPoliceCar = Instantiate(policeCarPrefab, hit.position, Quaternion.identity);
            PoliceCarController controller = newPoliceCar.GetComponent<PoliceCarController>();
            if (controller != null)
            {
                controller.playerCar = playerCar; // Assign PlayerCar for chasing
            }
        }
        else
        {
            Debug.LogWarning("Failed to find a valid NavMesh position for the PoliceCar.");
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

    void OnApplicationQuit()
    {
        isGameRunning = false; // Game is no longer running
    }

}
