using UnityEngine;
using UnityEngine.AI;

public class PoliceCarController : MonoBehaviour
{
    public Transform playerCar;
    public float chaseSpeed = 15f;
    public float pushForce = 500f;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = chaseSpeed;
        }
    }

    void Update()
    {
        if (agent != null && agent.isOnNavMesh) // Ensure the agent is active and on a NavMesh
        {
            if (playerCar != null)
            {
                agent.SetDestination(playerCar.position); // Set destination to PlayerCar
            }
        }
        else
        {
            Debug.LogError("NavMeshAgent is not properly placed on a NavMesh!");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("PlayerCar"))
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                Vector3 pushDirection = (collision.transform.position - transform.position).normalized;
                playerRb.AddForce(pushDirection * pushForce, ForceMode.Impulse); // Push PlayerCar
            }
        }
    }
}
