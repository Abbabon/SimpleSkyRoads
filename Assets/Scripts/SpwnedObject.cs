using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpawnedObject : MonoBehaviour
{
    private Transform _transform;
    [SerializeField] private float _speed;
    [SerializeField] private Vector3 _movementDirection = Vector3.back;

    private void Awake()
    {
        _transform = GetComponent<Transform>();
        GameManager.OnSessionStarted += DestroyOnSessionStart;
    }

    private void DestroyOnSessionStart()
    {
        if (_transform != null)
            Destroy(_transform.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.GameInSession)
            _transform.Translate(_movementDirection * (Time.deltaTime * _speed * GameManager.Instance.GameplaySpeedFactor),
                                Space.World);
    }
}
