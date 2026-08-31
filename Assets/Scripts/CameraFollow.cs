using UnityEngine;

// Parks the camera rigidly behind the ship - from Awake and on every frame after it.
// No damping and no per-state distances: the framing is identical on the main menu,
// during play, while boosting and on game over.
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 2f, -6f);

    private Transform _transform;

    private void Awake()
    {
        _transform = GetComponent<Transform>();
        SnapBehindTarget();
    }

    private void LateUpdate()
    {
        SnapBehindTarget();
    }

    private void SnapBehindTarget()
    {
        if (_target == null)
            return;

        _transform.position = _target.position + _offset;
        _transform.LookAt(_target);
    }
}
