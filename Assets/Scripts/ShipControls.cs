using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipControls : MonoBehaviour
{
    public float _shipSpeed = 18;
    
    [SerializeField] private Transform _startingLocation;
    private Transform _shipTransform;

    //used for position clamping:
    [SerializeField] private Transform _leftLimiter;
    [SerializeField] private Transform _rightLimiter;
    private BoxCollider _shipBoxCollider;
    private float _shipHalfWidth;

    private void Awake()
    {
        _shipTransform = GetComponent<Transform>();
        _shipBoxCollider = GetComponent<BoxCollider>();
        _shipHalfWidth = _shipBoxCollider.size.x / 2;

        GameManager.OnSessionStarted += ResetLocation;
    }

    private void ResetLocation()
    {
        _shipTransform.position = _startingLocation.position;
    }

    void Update()
    {
        if (GameManager.Instance.GameInSession)
        {
            float horizontalMovement = Input.GetAxis("Horizontal");
            LocalMove(horizontalMovement, _shipSpeed);
            HorizontalLean(_shipTransform, horizontalMovement, 80, .05f);

            if (Input.GetKeyDown(KeyCode.Space)){
                GameManager.Instance.IsPlayerBoosting = true;
                SoundManager.Instance.PlaySoundEffect(SoundEffect.Boost);
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                GameManager.Instance.IsPlayerBoosting = false;
            }
        }
    }

    void LocalMove(float horizontalMovement, float speed)
    {
        // pay <3 that the clamping refers to the center of the ship.
        float clampedX = Mathf.Clamp((
                                _shipTransform.localPosition.x + horizontalMovement * speed * Time.deltaTime),
                                _leftLimiter.position.x + _shipHalfWidth,
                                _rightLimiter.position.x - _shipHalfWidth);
        Vector3 newPosition = new Vector3(clampedX,_shipTransform.localPosition.y, _shipTransform.localPosition.z);
        _shipTransform.localPosition = newPosition;
    }

    void HorizontalLean(Transform target, float movementAmount, float leanLimit, float lerpTime)
    {
        Vector3 targetEulerAngels = target.localEulerAngles;
        target.localEulerAngles = new Vector3(targetEulerAngels.x, targetEulerAngels.y,
                                              Mathf.LerpAngle(targetEulerAngels.z, -movementAmount * leanLimit, lerpTime));
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.GetComponent<Asteroid>())
        {
            GameManager.Instance.PlayerHit();
            Destroy(collision.gameObject);
        }
        else if (collision.GetComponent<Battery>()){
            GameManager.Instance.PickedUpBattery();
            Destroy(collision.gameObject);
        }
        else if (collision.GetComponent<Crystal>()){
            GameManager.Instance.PickedUpCrystal();
            Destroy(collision.gameObject);
        }
    }
}
