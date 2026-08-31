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


    private void Awake()
    {
        GameManager.OnSessionStarted += Reset;
        GameManager.OnSessionEnded += Hide;
        GameManager.OnPickedUpShield += TurnOnShield;
    }

    private void Update()
    {
        //turning off shield 
        if (_shield.activeInHierarchy && !GameManager.Instance.IsPlayerShielded)
        {
            _shield.SetActive(false);
        }
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
