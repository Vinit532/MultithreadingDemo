using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour
{
    public static GameHandler Instance;

    public GameObject policeCarPrefab; // PoliceCar prefab
    public Transform playerCar; // Reference to PlayerCar
    public float respawnTime = 15f; // Time before respawning a new PoliceCar

    private GameObject currentPoliceCar;

    void Awake()
    {
        Instance = this;
    }

    public void TriggerPoliceSpawn(Vector3 spawnPosition)
    {
        if (currentPoliceCar == null)
        {
            SpawnPoliceCar(spawnPosition);
        }
    }

    private void SpawnPoliceCar(Vector3 spawnPosition)
    {
        currentPoliceCar = Instantiate(policeCarPrefab, spawnPosition, Quaternion.identity);
        PoliceCarController policeController = currentPoliceCar.GetComponent<PoliceCarController>();

        if (policeController != null)
        {
            policeController.playerCar = playerCar; // Set PlayerCar as the target
        }

        Invoke(nameof(CheckPoliceStatus), respawnTime);
    }

    private void CheckPoliceStatus()
    {
        if (currentPoliceCar == null) // If police failed to catch the player
        {
            SpawnPoliceCar(playerCar.position); // Respawn near the PlayerCar
        }
    }
}
