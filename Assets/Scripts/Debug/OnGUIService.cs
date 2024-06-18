using System;
using UnityEngine;

/// <summary>
///     Utility class to allow GUILayout to be used in multiple classes
/// </summary>
public class OnGUIService : MonoBehaviour
{
    public static OnGUIService Instance { get; private set; }

    public event Action OnGUIEvent;

    private bool _showDebug;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ToggleDebug();
        }
    }

    private void OnGUI()
    {
        if (!_showDebug)
        {
            return;
        }

        GUI.color = Color.black;
        OnGUIEvent?.Invoke();
        GUI.color = Color.white;
    }

    public void ToggleDebug()
    {
        _showDebug = !_showDebug;
    }
}