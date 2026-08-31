using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadMovement : MonoBehaviour
{
    [SerializeField] private List<string> _textureNames = new List<string> { "_MainTex", "_SpecTex", "_NormalTex", "_EmissionTex" };
    private MeshRenderer _meshRenderer;
    private Vector2 _offset = Vector2.zero;
    public float _speed = 0.1f;

    void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    
    void Update()
    {
        if (GameManager.Instance.GameInSession)
        {
            _offset.y += _speed * Time.deltaTime * GameManager.Instance.GameplaySpeedFactor; 
            foreach (string texture in _textureNames){
                _meshRenderer.material.SetTextureOffset(texture, _offset);
            }
        }
    }
}
