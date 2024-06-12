using System;
using UnityEngine;

/// <summary>
///     Utility class to allow GUILayout to be used in multiple classes
/// </summary>
public class OnGUIService : MonoBehaviour
{
    public static OnGUIService Instance { get; private set; }

    public event Action OnGUIEvent;

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

    private void OnGUI()
    {
        GUI.color = Color.black;
        OnGUIEvent?.Invoke();
        GUI.color = Color.white;
    }
}