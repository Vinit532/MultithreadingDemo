using System.Threading;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Car Prefabs")]
    public GameObject playerCarPrefab;    // Reference for PlayerCar prefab
    public GameObject policeCarPrefab;   // Reference for PoliceCar prefab

    [Header("Spawn Settings")]
    public Transform playerSpawnPoint;   // Starting point for PlayerCar
    public float policeSpawnOffset = 20f; // Distance behind the player where PoliceCar spawns
    public float safeSpeedLimit = 80f;    // Speed limit to trigger police spawn (in km/h)

    private GameObject playerCar;         // Instance of PlayerCar
    private Rigidbody playerRb;           // Cached Rigidbody reference of PlayerCar
    private float nextPoliceSpawnTime = 0f; // Cooldown for spawning additional police cars
    private float policeSpawnInterval = 10f; // Time between police spawns

    private bool isGameRunning = true;     // Flag to control the monitoring thread
    private float playerSpeed;             // Thread-safe player speed (updated on the main thread)

    private Thread speedMonitorThread;     // Separate thread for speed monitoring
    private readonly object speedLock = new object(); // Lock for thread-safe access to `playerSpeed`

    private void Start()
    {
        // Instantiate PlayerCar at the spawn point
        if (playerCarPrefab && playerSpawnPoint)
        {
            playerCar = Instantiate(playerCarPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
            playerRb = playerCar.GetComponent<Rigidbody>(); // Cache Rigidbody reference on the main thread
        }
        else
        {
            Debug.LogError("PlayerCarPrefab or PlayerSpawnPoint is missing in GameManager.");
        }

        // Start the monitoring thread
        StartSpeedMonitorThread();
    }

    private void Update()
    {
        if (playerRb != null)
        {
            // Safely calculate speed on the main thread
            float currentSpeed = playerRb.velocity.magnitude * 3.6f; // Speed in km/h

            // Update the thread-safe variable
            lock (speedLock)
            {
                playerSpeed = currentSpeed;
            }

            // Check if the speed exceeds the limit and spawn police on the main thread
            if (playerSpeed > safeSpeedLimit && Time.time >= nextPoliceSpawnTime)
            {
                SpawnPoliceCar();
                nextPoliceSpawnTime = Time.time + policeSpawnInterval; // Set cooldown for next spawn
            }
        }
    }

    private void OnDestroy()
    {
        // Ensure the thread stops when the GameManager is destroyed
        isGameRunning = false;
        if (speedMonitorThread != null && speedMonitorThread.IsAlive)
        {
            speedMonitorThread.Join(); // Wait for the thread to finish
        }
    }

    private void StartSpeedMonitorThread()
    {
        speedMonitorThread = new Thread(() =>
        {
            while (isGameRunning)
            {
                // Access the speed in a thread-safe manner
                float monitoredSpeed;

                lock (speedLock)
                {
                    monitoredSpeed = playerSpeed; // Safely read the current speed
                }

                // Simulate processing the speed on the worker thread
                if (monitoredSpeed > safeSpeedLimit)
                {
                    Debug.Log($"[Thread] Player is speeding! Current Speed: {monitoredSpeed} km/h");
                }

                // Add a small delay to prevent overloading the CPU
                Thread.Sleep(50);
            }
        });

        speedMonitorThread.IsBackground = true; // Make the thread stop with the application
        speedMonitorThread.Start();
    }

    private void SpawnPoliceCar()
    {
        if (policeCarPrefab == null || playerCar == null)
        {
            Debug.LogError("PoliceCarPrefab or PlayerCar instance is missing. Cannot spawn PoliceCar.");
            return;
        }

        // Calculate spawn position behind or around the player
        Vector3 spawnPosition = playerCar.transform.position - playerCar.transform.forward * policeSpawnOffset;
        spawnPosition.y += 1f; // Adjust height to avoid spawning underground

        // Instantiate the PoliceCar
        GameObject policeCar = Instantiate(policeCarPrefab, spawnPosition, Quaternion.identity);

        // Initialize the PoliceCar script
        PoliceCar policeCarScript = policeCar.GetComponent<PoliceCar>();
        if (policeCarScript != null)
        {
            policeCarScript.Initialize(playerCar);
        }
        else
        {
            Debug.LogError("PoliceCar script is missing on the PoliceCar prefab.");
        }
    }
}
