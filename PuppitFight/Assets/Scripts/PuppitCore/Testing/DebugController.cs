using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Debug Affecter controller mapping default numkeys to affecter actions and modifiers
/// </summary>
public class DebugController : MonoBehaviour, IModifierProvider, IActionProvider
{
    [SerializeField]
    private PuppitLimb _targetPuppitLimb;

    private string _modifier;
    private string _action;

    private List<string> _affectNames;
    private List<string> _modifierNames;

    private void Awake()
    {
        _targetPuppitLimb.OnFinishSetup += Setup;
    }

    private void Update()
    {
        var start = (int)KeyCode.A;
        for (var i = 0; i < _modifierNames.Count; i++)
        {
            if (Input.GetKeyDown((KeyCode)(start + i)))
            {
                _modifier = _modifierNames[i];
            }
        }

        start = (int)KeyCode.Alpha1;
        for (var i = 0; i < _affectNames.Count; i++)
        {
            if (Input.GetKey((KeyCode)(start + i)))
            {
                _action = _affectNames[i];
                break;
            }
        }
    }

    public string GetCurrentAction()
    {
        return _action;
    }

    public event Action<string> OnOneshotAction;

    public string GetCurrentModifier()
    {
        return _modifier;
    }

    private void Setup()
    {
        _affectNames = _targetPuppitLimb.GetAllActionNames();
        _modifierNames = _targetPuppitLimb.GetAllModifierNames();

        _modifier = _modifierNames[0];
        _action = _affectNames[0];
    }
}