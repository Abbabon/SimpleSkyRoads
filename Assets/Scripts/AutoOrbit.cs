using UnityEngine;
using Unity.Cinemachine;

// Spins a CinemachineOrbitalFollow at a constant rate. Nobody is steering these orbits -
// they are attract-mode idles, so there is no CinemachineInputAxisController involved.
public class AutoOrbit : MonoBehaviour
{
    [SerializeField] private CinemachineOrbitalFollow _orbit;
    [SerializeField] private float _degreesPerSecond = 12f;

    private void Update()
    {
        // the horizontal axis wraps at +-180, so keep the value inside that range
        float angle = _orbit.HorizontalAxis.Value + _degreesPerSecond * Time.deltaTime;
        _orbit.HorizontalAxis.Value = Mathf.Repeat(angle + 180f, 360f) - 180f;
    }
}
