using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;

public class ObservationPoint : MonoBehaviour
{
    [field: SerializeField]
    public int Index { get; set; }
    [field: SerializeField]
    public bool IsAvailable { get; set; } = true;

    [field: SerializeField]
    public Vector3 position;
    
    [field:SerializeField]
    public ParticleSystem particles;

    private void Awake()
    {
        particles = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        position = transform.position;
    }
}
