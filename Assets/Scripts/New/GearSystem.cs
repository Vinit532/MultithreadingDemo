using UnityEngine;

public class GearSystem : MonoBehaviour
{
    public int[] gearSpeeds = { 0, 20, 40, 60, 80, 120 }; // Speed thresholds for each gear
    public float[] gearRatios = { 0f, 1f, 1.5f, 2f, 2.5f, 3f }; // Multipliers for motor force

    private int currentGear = 1;
    private float currentSpeed;

    public PlayerCarController carController;

    void Update()
    {
        currentSpeed = GetComponent<Rigidbody>().velocity.magnitude * 3.6f; // Convert m/s to km/h
        ChangeGear();
    }

    private void ChangeGear()
    {
        for (int i = gearSpeeds.Length - 1; i >= 0; i--)
        {
            if (currentSpeed >= gearSpeeds[i])
            {
                currentGear = i + 1;
                carController.motorForce = 1500f * gearRatios[i];
                break;
            }
        }
    }
}
