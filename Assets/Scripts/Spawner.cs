using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> _spawnedObjects;
    [SerializeField] private List<GameObject> _spawnLocations;

    [SerializeField] private float _baseSpawnRate = 5.0f;
    private float _spawnTimer;

    [SerializeField] private bool _affectedByDifficulyLevel = true;

    private void Awake()
    {
        //immediately spawn if this an asteroid for example; the others are meant to wait
        _spawnTimer = _affectedByDifficulyLevel ? _baseSpawnRate : 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.GameInSession)
        {
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= _baseSpawnRate)
            {
                Spawn();
                RefreshSpawnRate();
            }
        }
    }

    // extracted to a method to enable randomization of spawn times later
    // this also takes into account the movement speed that's rising
    private void RefreshSpawnRate()
    {
        _spawnTimer = _affectedByDifficulyLevel ? GameManager.Instance.GameplayDifficultyFactor : 0f;
    }

    private void Spawn()
    {
        Instantiate(_spawnedObjects[UnityEngine.Random.Range(0, _spawnedObjects.Count)],
                                           _spawnLocations[UnityEngine.Random.Range(0, _spawnLocations.Count)].transform.position,
                                           Quaternion.identity);
    }
}
