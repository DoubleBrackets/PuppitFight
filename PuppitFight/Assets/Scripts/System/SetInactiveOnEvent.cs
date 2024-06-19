using Events;
using UnityEngine;

public class SetInactiveOnEvent : MonoBehaviour
{
    [SerializeField]
    private SOEvent _event;

    [SerializeField]
    private bool _active;

    [SerializeField]
    private GameObject _target;

    private void Awake()
    {
        _event.AddListener(OnEvent);
    }

    private void OnDestroy()
    {
        _event.RemoveListener(OnEvent);
    }

    private void OnEvent()
    {
        _target.SetActive(_active);
    }
}