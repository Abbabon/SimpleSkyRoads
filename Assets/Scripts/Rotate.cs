using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    Transform _transform;
    [SerializeField] private Space _rotationSpace;
    [SerializeField] private Vector3 _rotationSpeed = new Vector3(0, 30, 0);

    void Awake(){
        _transform = GetComponent<Transform>();
    }

    void Update(){
        _transform.Rotate(_rotationSpeed * Time.deltaTime, _rotationSpace);
    }
}
