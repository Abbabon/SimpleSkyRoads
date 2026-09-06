using UnityEngine;
using Unity.Cinemachine;

// Decides which Cinemachine camera is live. The CinemachineBrain on Main Camera does the
// actual blending; all this script does is move priorities around as the game state changes.
public class CameraDirector : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _playCamera;
    [SerializeField] private CinemachineCamera _boostCamera;
    [SerializeField] private CinemachineCamera _crashCamera;

    // highest priority wins. the menu camera is authored above the play camera in the scene, so
    // the game opens on its orbit; these lift the others above it when their moment comes.
    private const int PlayPriority = 30;
    private const int CrashPriority = 35;
    private const int BoostPriority = 40;

    private void Awake()
    {
        GameManager.OnSessionStarted += ShowPlayCamera;
        GameManager.OnSessionEnded += ShowCrashCamera;
    }

    private void OnDestroy()
    {
        GameManager.OnSessionStarted -= ShowPlayCamera;
        GameManager.OnSessionEnded -= ShowCrashCamera;
    }

    private void Update()
    {
        // there is no boost event to subscribe to, so poll. IsPlayerBoosting is only cleared on the
        // next StartGame, hence the GameInSession check - dying mid-boost must not leave this camera up.
        bool boosting = GameManager.Instance.GameInSession && GameManager.Instance.IsPlayerBoosting;
        _boostCamera.Priority = boosting ? BoostPriority : 0;
    }

    // the first start and every retry: back behind the ship, and the crash camera gets out of the way
    private void ShowPlayCamera()
    {
        _crashCamera.Priority = 0;
        _playCamera.Priority = PlayPriority;
    }

    // the ship stays where it died until retry, so orbiting its transform orbits the crash site
    private void ShowCrashCamera(bool newHiScore)
    {
        _crashCamera.Priority = CrashPriority;
    }
}
