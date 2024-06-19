using System;
using System.Threading.Tasks;
using Panda;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class AgentController : MonoBehaviour
{
    public Camera cam;
    public NavMeshAgent agent;
    public float agentSpeed = 1f;
    
    [Range(0f, 1f)]
    public float priority = 0;

    private Vector3 _destination;
    
    [SerializeField] private float _avoidanceDistance = 5f;
    [SerializeField] private string _agentTag = "IA";
    
    // Update is called once per frame
    void Update()
    {
        // if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !agent.hasPath)
        // {
        //     MoveToRandomPosition();
        // }
        // AvoidObstacles();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag(_agentTag))
        {
            AgentController otherAgentController = other.gameObject.GetComponent<AgentController>();
            if (otherAgentController != null && otherAgentController.priority < this.priority)
            {
                Debug.Log($"{gameObject.name} collided with {other.gameObject.name}");
                // Cuando hay una colisión con otro jugador con menor prioridad, genera una nueva posición aleatoria y se mueve hacia ella.
                otherAgentController.GoToDestination();
            }
        }
    }
    [Panda.Task]
    private void SelectDestinationPoint()
    {
        float radius = 10.0f; // Define el radio dentro del cual se generará la posición aleatoria.
        float margin = 1.0f; // Define el margen que quieres mantener desde el borde del NavMesh.
        Vector3 randomDirection = Random.insideUnitSphere * (radius - margin);
        randomDirection += transform.position;
        _destination = Vector3.zero;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, (radius - margin), 1))
        {
            _destination = hit.position;
            Panda.Task.current.Succeed();
        }
    }
    [Panda.Task]
    private void GoToDestination()
    {
        // GetRandomPositionOnNavMesh();
        agent.SetDestination(_destination);
        agent.speed = agentSpeed;
        AvoidObstacles();

        // Si el agente llego a su destino, marcar como completada la tarea.
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !agent.hasPath)
        {
            Panda.Task.current.Succeed();
            Debug.Log("Agent arrived to destination");
        }
    }
    
    private void AvoidObstacles()
    {
        // Raycast para detectar agentes cercanos
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, _avoidanceDistance))
        {
            if (hit.collider.CompareTag(_agentTag))
            {
                // Calcula una dirección de evasión
                Vector3 evasionDirection = Vector3.Cross(transform.up, hit.normal).normalized;
                // Añade la dirección de evasión a la dirección actual del agente
                agent.SetDestination(transform.position + evasionDirection);
            }
        }
    }
    
    public void SetPriority(float newPriority)
    {
        priority = newPriority;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * _avoidanceDistance);
        
        const float radiusDestination = 0.5f;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_destination, radiusDestination);
        
        const float radiusCurrentPositionAgent = 0.5f;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radiusCurrentPositionAgent);
        
        
    }
}
