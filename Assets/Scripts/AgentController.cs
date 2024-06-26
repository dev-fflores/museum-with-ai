using Managers;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class AgentController : MonoBehaviour
{
    public Camera cam;
    public NavMeshAgent agent;
    public float agentSpeed = 1f;
    public Animator animator;
    
    [Range(0f, 1f)]
    public float priority;

    private Vector3 _destination;

    [SerializeField] private ObservationPoint _observationPointTarget;
    
    [SerializeField] private bool _wantToSeeStatue = false;
    
    [SerializeField] private float _avoidanceDistance = 5f;
    [SerializeField] private string _agentTag = "IA";
    private static readonly int IsIdle = Animator.StringToHash("isIdle");

    // Update is called once per frame

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        _observationPointTarget = null;
    }

    private void Start()
    { 
        GetRandomDestinationInNavMesh();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag(_agentTag))
        {
            AgentController otherAgentController = other.gameObject.GetComponent<AgentController>();
            if (otherAgentController != null && otherAgentController.priority < this.priority)
            {
                // Debug.Log($"{gameObject.name} collided with {other.gameObject.name}");
                // Cuando hay una colisión con otro jugador con menor prioridad, genera una nueva posición aleatoria y se mueve hacia ella.
                // otherAgentController.GoToDestination();
                
                GetRandomDestinationInNavMesh();
            }
        }
    }

    [Panda.Task]
    private void WantToSeeStatue()
    {

        if (_wantToSeeStatue)
        {
            // Debug.Log("I want to see a statue");
            Panda.Task.current.Succeed();
        }else
        {
            // Debug.Log("I don't want to see a statue");
            Panda.Task.current.Fail();
        }
    }
    
    [Panda.Task]
    private void SelectStatue()
    {
        // Seleccionar una estatua aleatoria
        _observationPointTarget = GameManager.Instance.GetRandomAvailableObservationPoint();

        if (_observationPointTarget == null)
        {
            Panda.Task.current.Fail();
            return;
        }
        
        _destination = _observationPointTarget.position;
        // Debug.Log($"Selected statue: {_statueTarget.Index} at position: {_destination}");
        Panda.Task.current.Succeed();
    }

    [Panda.Task]
    private void GoToStatue()
    {
        animator.SetBool(IsIdle, false);
        AgentSetDestination(_destination);
        AvoidObstacles();

        if (GameManager.Instance.availableObservationPoints.Count == 0 || _observationPointTarget == null)
        {
            Panda.Task.current.Fail();
            return;
        }

        // Si el agente llego a su destino, marcar como completada la tarea.
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !agent.hasPath)
        {
            Debug.Log($"{gameObject.name} arrived at statue with position: {_destination}");
            GameManager.Instance.SetObservationPointAvailability(_observationPointTarget.Index, false);
            animator.SetBool(IsIdle, true);
            Panda.Task.current.Succeed();
            // transform.forward = GameManager.Instance.statues[_statueTarget.Index].transform.position - transform.position;
        }
    }

    [Panda.Task]
    private void ContinueWatchingStatue()
    {
        var randomNumber = GetRandomNumber01();
        
        if (randomNumber == 1)
        {
            Debug.Log($"{gameObject.name} wants to continue watching the statue");
            _wantToSeeStatue = true;
            // GameManager.Instance.SetObservationPointAvailability(_observationPointTarget.Index, false);
            GameManager.Instance.observationPoints[_observationPointTarget.Index].particles.Stop();
            Panda.Task.current.Succeed();
        }
        else
        {
            Debug.Log($"{gameObject.name} doesn't want to continue watching the statue");
            _wantToSeeStatue = false;
            Panda.Task.current.Fail();
        }
    }


    [Panda.Task]
    private void WaitSeconds()
    {
        Debug.Log($"{gameObject.name} is waiting");
        // Contador de tiempo para esperar
        float waitTime = Random.Range(1, 5);

        float initialTime = 0f;
        initialTime += Time.deltaTime;

        while(initialTime < waitTime)
        {
            initialTime += Time.deltaTime;
        }
        
        Panda.Task.current.Succeed();
    }

    [Panda.Task]
    private void FinishedWatchingStatue()
    {
        // Marcar la estatua como disponible
        GameManager.Instance.SetObservationPointAvailability(_observationPointTarget.Index, true);
        GameManager.Instance.observationPoints[_observationPointTarget.Index].particles.Play();
        _observationPointTarget = null;
        _wantToSeeStatue = false;
        Panda.Task.current.Fail();
    }


    [Panda.Task]
    private void SelectDestinationPoint()
    {
        animator.SetBool(IsIdle, false);
        GetRandomDestinationInNavMesh();
        Panda.Task.current.Succeed();
    }
    
    // TODO: Pensar quién debería colocar el flag de que el ObservationPoint esta disponible nuevamente.

    [Panda.Task]
    private void GoToDestination()
    {
        // GetRandomPositionOnNavMesh();
        AgentSetDestination(_destination);

        // Si el agente llego a su destino, marcar como completada la tarea.
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !agent.hasPath)
        {
            var randomNumber = 1;
            
            if (randomNumber == 1)
            {
                _wantToSeeStatue = true;
                Panda.Task.current.Fail();
                // Debug.Log($"{gameObject.name} arrived at destination with position: {_destination}");
            }
            else
            {
                _wantToSeeStatue = false;
                Panda.Task.current.Succeed();
            }
        }
    }

    private void AgentSetDestination(Vector3 destination)
    {
        agent.SetDestination(destination);
        agent.speed = agentSpeed;
        AvoidObstacles();
    }

    private void GetRandomDestinationInNavMesh()
    {
        // if (_observationPointTarget != null)
        // {
        //     GameManager.Instance.observationPoints[_observationPointTarget.Index].IsAvailable = true;
        //     Debug.Log($"Statue {_observationPointTarget.Index} is available again");
        // }
        
        float radius = 10.0f; // Define el radio dentro del cual se generará la posición aleatoria.
        // float margin = 1.0f; // Define el margen que quieres mantener desde el borde del NavMesh.
        Vector3 randomDirection = Random.insideUnitSphere * (radius);
        randomDirection += transform.position;
        // _destination = Vector3.zero;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, (radius), 1))
        {
            _destination = hit.position;
        }
    }
    
    private int GetRandomNumber01()
    {
        return Random.Range(0, 2);
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
