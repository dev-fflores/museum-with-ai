using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawmeableItem : MonoBehaviour
{
    public Vector3 initialPosition;
    public Quaternion initialRotation;
    private Rigidbody _rgb;
    public string groundTag = "Ground";

    private void Start() {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        _rgb = GetComponent<Rigidbody>();
    }
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag(groundTag))
        {
            Debug.Log("Object in ground");
            transform.gameObject.SetActive(false);
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            _rgb.velocity = Vector3.zero;
            transform.gameObject.SetActive(true);
        }
    }
}
