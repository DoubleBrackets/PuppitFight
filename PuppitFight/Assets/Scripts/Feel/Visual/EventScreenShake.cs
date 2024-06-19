using Cinemachine;
using Events;
using UnityEngine;

public class EventScreenShake : MonoBehaviour
{
    [SerializeField]
    private SOEvent _onScreenShake;

    [SerializeField]
    private CinemachineImpulseSource _impulseSource;

    private void Start()
    {
        _onScreenShake.AddListener(HandleScreenShake);
    }

    private void OnDestroy()
    {
        _onScreenShake.RemoveListener(HandleScreenShake);
    }

    private void HandleScreenShake()
    {
        _impulseSource.GenerateImpulse();
    }
}