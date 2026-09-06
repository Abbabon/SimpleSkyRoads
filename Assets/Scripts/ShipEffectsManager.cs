using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ShipEffectsManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> _basicObjects;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private BoxCollider _boxCollider;
    [SerializeField] private GameObject _shield;

    [SerializeField] private float _dangerRadius = 20f;
    [SerializeField] private Color _dangerColor = Color.red;
    [SerializeField] private float _sampleInterval = 0.05f;

    private Color _baseColor;
    private float _sampleTimer;

    // keeping the readings around makes the tint smooth instead of jumpy
    private List<float> _dangerReadings = new List<float>();

    // a running CSV of the danger curve; we dump it when tuning the difficulty
    private string _dangerLog = "";
    public string DangerLog { get { return _dangerLog; } }

    // serialized for visualization, not for tweaking
    [SerializeField] private float _medianDanger;


    private void Awake()
    {
        GameManager.OnSessionStarted += Reset;
        GameManager.OnSessionEnded += Hide;
        GameManager.OnPickedUpShield += TurnOnShield;

        _baseColor = _meshRenderer.material.color;
    }

    private void Update()
    {
        //turning off shield 
        if (_shield.activeInHierarchy && !GameManager.Instance.IsPlayerShielded)
        {
            _shield.SetActive(false);
        }

        if (GameManager.Instance.GameInSession)
        {
            RefreshDangerTint();
        }
    }

    // the ship glows red when a rock is about to hit it
    private void RefreshDangerTint()
    {
        Asteroid[] asteroids = FindObjectsByType<Asteroid>(FindObjectsSortMode.None);

        float nearest = _dangerRadius;
        foreach (Asteroid asteroid in asteroids)
        {
            float distance = Vector3.Distance(transform.position, asteroid.transform.position);
            if (distance < nearest)
                nearest = distance;
        }

        _meshRenderer.material.color = Color.Lerp(_dangerColor, _baseColor, nearest / _dangerRadius);

        // sample a few times a second so the log doesn't get huge
        _sampleTimer += Time.deltaTime;
        if (_sampleTimer >= _sampleInterval)
        {
            _dangerReadings.Add(nearest);
            _sampleTimer = 0f;
        }

        _dangerLog = "";
        foreach (float reading in _dangerReadings)
        {
            _dangerLog += reading.ToString("F2") + ",";
        }

        // a median is steadier than an average when a single rock whips past
        List<float> sorted = new List<float>(_dangerReadings);
        sorted.Sort();
        _medianDanger = sorted.Count > 0 ? sorted[sorted.Count / 2] : _dangerRadius;
    }

    // there IS some DRY fault in the Hide / Reset methods, but I tried to encapsulate it and it was way uglier so I changed my mind.
    // called when a session ends: the ship simply disappears, there is no impact VFX
    private void Hide(bool newHiScore)
    {
        foreach (GameObject basicObject in _basicObjects)
        {
            basicObject.SetActive(false);
        }

        _meshRenderer.enabled = false;
        _boxCollider.enabled = false;
        _shield.SetActive(false);
    }

    // called when a session started
    private void Reset()
    {
        foreach (GameObject basicObject in _basicObjects)
        {
            basicObject.SetActive(true);
        }

        _meshRenderer.enabled = true;
        _boxCollider.enabled = true;
    }

    private void TurnOnShield(){
        _shield.SetActive(true);
    }
}
