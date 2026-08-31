using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasterEggMovement : MonoBehaviour
{
    Transform _transform;
    [SerializeField] private Space _rotationSpace;
    [SerializeField] private Vector3 _movementVector = new Vector3(40, 0, 0);
    [SerializeField] private float _spawnScore = 50f;

    private Vector3 _startingPosition;

    void Awake()
    {
        _transform = GetComponent<Transform>();
        _startingPosition = _transform.position;

        GameManager.OnSessionStarted += Reset;
    }


    void Update()
    {
        if (GameManager.Instance.Score > _spawnScore){
            _transform.Translate(_movementVector * Time.deltaTime);
        }
    }

    private void Reset(){
        _transform.position = _startingPosition;
    }
}
