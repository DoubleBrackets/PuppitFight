using System;
using System.Collections.Generic;
using Puppitor;
using UnityEngine;

public class DebugController : MonoBehaviour, IModifierProvider, IActionProvider
{
    [SerializeField]
    private Puppit _puppit;

    private string _modifier;
    private string _action;

    private Affecter _affecter;
    private List<string> _affectNames;
    private List<string> _modifierNames;

    private void Start()
    {
        _puppit.SetModifierProvider(this);
        _puppit.SetActionProvider(this);
        _affecter = _puppit.Affecter;

        _affectNames = _affecter.GetAllActionNames();
        _modifierNames = _affecter.GetAllModifierNames();

        _modifier = _modifierNames[0];
        _action = _affectNames[0];
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
}