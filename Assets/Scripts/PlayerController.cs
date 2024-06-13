using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    public Camera cam;
    public NavMeshAgent agent;
    public float agentSpeed = 1f;
    [SerializeField] private bool isArrived = false;

    private Vector3 destination;
    // Update is called once per frame
    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !agent.hasPath)
        {
            MoveToRandomPosition();
        }
    }


    private Vector3 GetRandomPositionOnNavMesh()
    {
        float radius = 10.0f; // Define el radio dentro del cual se generará la posición aleatoria.
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }
        return finalPosition;
    }
    public void MoveToRandomPosition()
    {
        destination = GetRandomPositionOnNavMesh();
        agent.SetDestination(destination);
        agent.speed = agentSpeed;
    }
}
