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

    private void Start()
    {
        position = transform.position;
    }
}
