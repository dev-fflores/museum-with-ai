using System;
using Managers;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class AgentController : MonoBehaviour
{
    #region Properties
    
    private const int MAX_PRIORITY = 1;
    private GameObject _anotherAgent;

    public NavMeshAgent agent;
    public float agentSpeed = 1f;
    public Animator animator;

    [Range(0f, 1f)] public float priority;

    private Vector3 _destination;

    [SerializeField] private ObservationPoint _observationPointTarget;

    [SerializeField] private bool _wantToSeeStatue = false;

    [SerializeField] private float _avoidanceDistance = 5f;
    [SerializeField] private string _agentTag = "IA";
    [SerializeField] private bool _collisionWithAnotherAgent = false;
    
    private static readonly int IsIdle = Animator.StringToHash("isIdle");

    #endregion

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

    #region Behaviour Tree

    [Panda.Task]
    private void WantToSeeStatue()
    {

        if (_wantToSeeStatue)
        {
            // Debug.Log("I want to see a statue");
            Panda.Task.current.Succeed();
        }
        else
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
        // GameManager.Instance.observationPoints[_observationPointTarget.Index].IsSelected = true;
        // Debug.Log($"Selected statue: {_statueTarget.Index} at position: {_destination}");
        Panda.Task.current.Succeed();
    }

    [Panda.Task]
    private void GoToStatue()
    {
        animator.SetBool(IsIdle, false);
        // SetPriority(MAX_PRIORITY);
        agent.SetDestination(_destination);

        

        if (GameManager.Instance.availableObservationPoints.Count == 0 || _observationPointTarget == null || !_observationPointTarget.IsAvailable || IsCollidingWithAnotherAgent())
        {
            _collisionWithAnotherAgent = false;
            _anotherAgent = null;
            Panda.Task.current.Fail();
            return;
        }

        // Si el agente llego a su destino, marcar como completada la tarea.
        if (AgentArrivedAtDestination())
        {
            Debug.Log($"{gameObject.name} arrived at statue with position: {_destination}");
            GameManager.Instance.SetObservationPointAvailability(_observationPointTarget.Index, false);
            animator.SetBool(IsIdle, true);
            Panda.Task.current.Succeed();
            // transform.forward = GameManager.Instance.observationPoints[_observationPointTarget.Index].transform.parent.position - transform.position;
            transform.LookAt(GameManager.Instance.observationPoints[_observationPointTarget.Index].transform.parent.position);
            
            // Quiero que mi agente mire hacia el objeto padre de observationPoint, que se alinea su localPosition con la posición del observationPoint
        }
    }

    [Panda.Task]
    private void ContinueWatchingStatue()
    {
        var randomNumber = GetRandomNumber01();

        if (randomNumber == 1)
        {
            // Debug.Log($"{gameObject.name} wants to continue watching the statue");
            _wantToSeeStatue = true;
            GameManager.Instance.SetObservationPointAvailability(_observationPointTarget.Index, false);
            GameManager.Instance.observationPoints[_observationPointTarget.Index].particles.Stop();
            Panda.Task.current.Succeed();
        }
        else
        {
            // Debug.Log($"{gameObject.name} doesn't want to continue watching the statue");
            _wantToSeeStatue = false;
            GameManager.Instance.SetObservationPointAvailability(_observationPointTarget.Index, true);
            // GameManager.Instance.observationPoints[_observationPointTarget.Index].IsSelected = false;
            GameManager.Instance.observationPoints[_observationPointTarget.Index].particles.Play();
            _observationPointTarget = null;
            Panda.Task.current.Fail();
        }
    }


    [Panda.Task]
    private void WaitSeconds()
    {
        // Debug.Log($"{gameObject.name} is waiting");
        // Contador de tiempo para esperar
        float waitTime = Random.Range(1, 5);

        float initialTime = 0f;
        initialTime += Time.deltaTime;

        while (initialTime < waitTime)
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
    

    [Panda.Task]
    private void GoToDestination()
    {
        
        AgentSetDestination(_destination);

        // Si el agente llego a su destino, marcar como completada la tarea.
        if (AgentArrivedAtDestination())
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


    #endregion

    #region Functions
    
    private bool IsCollidingWithAnotherAgent()
    {
        if (_collisionWithAnotherAgent)
        {
            if (this.priority < _anotherAgent.GetComponent<AgentController>().GetPriority())
            {
                return true;
            }
        }
        return false;
    }

    private void AgentSetDestination(Vector3 destination)
    {
        if (_collisionWithAnotherAgent)
        {
            GetRandomDestinationInNavMesh();
            _collisionWithAnotherAgent = false;
        }
        
        agent.SetDestination(destination);
        agent.speed = agentSpeed;
        // AvoidObstacles();
        // Debug.Log($"Agent {gameObject.name} is going to destination: {destination}");
    }

    private void GetRandomDestinationInNavMesh()
    {
        // if (_observationPointTarget != null)
        // {
        //     GameManager.Instance.observationPoints[_observationPointTarget.Index].IsAvailable = true;
        //     Debug.Log($"Statue {_observationPointTarget.Index} is available again");
        // }

        float radius = agent.height * 2.0f;
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
    
    public float GetPriority()
    {
        return priority;
    }
    
    private bool AgentArrivedAtDestination()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !agent.hasPath;
    }

    #endregion
    
    #region Debug

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag(_agentTag))
        {
            AgentController otherAgentController = other.gameObject.GetComponent<AgentController>();
            _anotherAgent = other.gameObject;
            // if (otherAgentController != null && otherAgentController.priority < this.priority)
            if (otherAgentController != null && this.priority < otherAgentController.GetPriority())
            {
                Debug.Log($"{gameObject.name} collided with {other.gameObject.name}");
                // Cuando hay una colisión con otro jugador con menor prioridad, genera una nueva posición aleatoria y se mueve hacia ella.
                // otherAgentController.GoToDestination();

                _collisionWithAnotherAgent = true;
            }
            
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag(_agentTag))
        {
            if (other.gameObject == _anotherAgent)
            {
                _collisionWithAnotherAgent = true;
            }
        }
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

    #endregion
}
