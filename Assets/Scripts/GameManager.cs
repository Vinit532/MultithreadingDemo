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
    private float nextPoliceSpawnTime = 0f; // Cooldown for spawning additional police cars
    private float policeSpawnInterval = 10f; // Time between police spawns

    private void Start()
    {
        // Instantiate PlayerCar at the spawn point
        if (playerCarPrefab && playerSpawnPoint)
        {
            playerCar = Instantiate(playerCarPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
        }
        else
        {
            Debug.LogError("PlayerCarPrefab or PlayerSpawnPoint is missing in GameManager.");
        }
    }

    private void Update()
    {
        if (playerCar != null)
        {
            MonitorPlayerSpeed();
        }
    }

    private void MonitorPlayerSpeed()
    {
        // Get the Rigidbody component of the PlayerCar
        Rigidbody playerRb = playerCar.GetComponent<Rigidbody>();
        if (playerRb == null) return;

        // Calculate the player's speed in km/h
        float playerSpeed = playerRb.velocity.magnitude * 3.6f;

        // Check if PlayerCar exceeds the safe speed limit
        if (playerSpeed > safeSpeedLimit && Time.time >= nextPoliceSpawnTime)
        {
            SpawnPoliceCar();
            nextPoliceSpawnTime = Time.time + policeSpawnInterval; // Set cooldown for next spawn
        }
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
