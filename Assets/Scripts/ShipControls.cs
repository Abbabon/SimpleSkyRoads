using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    //the on-screen buttons; only shown on a touch build, see Awake.
    [SerializeField] private GameObject _mobileControls;

    private InputAction _steer;
    private InputAction _boost;

    // Input.GetAxis used to smooth the raw key press for us (the old Horizontal axis ramped at
    // 3 units/second, and snapped through zero when you reversed). The Input System hands us the
    // raw -1/0/1 instead, so the same ramp is reproduced here - without it the ship jumps to full
    // speed on the first frame and the lean animation looks like a flick.
    private const float SteerRamp = 3f;
    private float _smoothedSteer;

    private void Awake()
    {
        _shipTransform = GetComponent<Transform>();
        _shipBoxCollider = GetComponent<BoxCollider>();
        _shipHalfWidth = _shipBoxCollider.size.x / 2;

        //the project-wide actions asset, which the Input System enables for us on entering play mode.
        InputActionMap player = InputSystem.actions.FindActionMap(Constants.PlayerActionMap, throwIfNotFound: true);
        _steer = player.FindAction(Constants.SteerAction, throwIfNotFound: true);
        _boost = player.FindAction(Constants.BoostAction, throwIfNotFound: true);

        GameManager.OnSessionStarted += ResetLocation;

#if UNITY_ANDROID || UNITY_IOS
        _mobileControls.SetActive(true);
#else
        _mobileControls.SetActive(false);
#endif
    }

    private void ResetLocation()
    {
        _shipTransform.position = _startingLocation.position;
        _smoothedSteer = 0f;
    }

    void Update()
    {
        if (GameManager.Instance.GameInSession)
        {
            float horizontalMovement = SmoothSteer(_steer.ReadValue<float>());
            LocalMove(horizontalMovement, _shipSpeed);
            HorizontalLean(_shipTransform, horizontalMovement, 80, .05f);

            if (_boost.WasPressedThisFrame()){
                GameManager.Instance.IsPlayerBoosting = true;
                SoundManager.Instance.PlaySoundEffect(SoundEffect.Boost);
            }
            if (_boost.WasReleasedThisFrame())
            {
                GameManager.Instance.IsPlayerBoosting = false;
            }
        }
    }

    // 'snap': when the player steers the other way, drop straight to zero instead of coasting
    // through the middle, then ramp up in the new direction.
    private float SmoothSteer(float rawSteer)
    {
        if (rawSteer != 0f && Mathf.Sign(rawSteer) != Mathf.Sign(_smoothedSteer))
        {
            _smoothedSteer = 0f;
        }

        _smoothedSteer = Mathf.MoveTowards(_smoothedSteer, rawSteer, SteerRamp * Time.deltaTime);
        return _smoothedSteer;
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
