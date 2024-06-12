using System;
using System.IO;
using Puppitor;
using UnityEngine;

public class Puppit : MonoBehaviour
{
    [SerializeField]
    [TextArea]
    private string _affectRuleFilePath;

    public Affecter Affecter => _affecter;

    private AffectVector _affectVector;
    private Affecter _affecter;

    private string _lastAction;
    private string _modifier;

    private IModifierProvider _modifierProvider;
    private IActionProvider _actionProvider;

    private void Awake()
    {
        SetupAffector();
    }

    private void Start()
    {
        OnGUIService.Instance.OnGUIEvent += DrawDebugGUI;
    }

    private void Update()
    {
        if (_modifierProvider != null)
        {
            _modifier = _modifierProvider.GetCurrentModifier();
        }

        if (_actionProvider != null)
        {
            _lastAction = _actionProvider.GetCurrentAction();
            _affecter.UpdateAffect(_affectVector, _lastAction, _modifier, Time.deltaTime);
        }
    }

    private void OnDestroy()
    {
        OnGUIService.Instance.OnGUIEvent -= DrawDebugGUI;
    }

    public void SetModifierProvider(IModifierProvider modifierProvider)
    {
        _modifierProvider = modifierProvider;
    }

    public void SetActionProvider(IActionProvider actionProvider)
    {
        if (_actionProvider != null)
        {
            _actionProvider.OnOneshotAction -= OnOneshotAction;
        }

        _actionProvider = actionProvider;
        _actionProvider.OnOneshotAction += OnOneshotAction;
    }

    private void OnOneshotAction(string actionName)
    {
        try
        {
            _affecter.UpdateAffect(
                _affectVector,
                actionName,
                _modifierProvider.GetCurrentModifier(),
                Time.deltaTime);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error firing oneshot action: {e}");
            throw;
        }
    }

    private void SetupAffector()
    {
        try
        {
            string jsonString = File.ReadAllText(_affectRuleFilePath);

            _affecter = new Affecter(jsonString);
            _affectVector = _affecter.MakeAffectVector();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error setting up actor {e}");
            throw;
        }
    }

    private void DrawDebugGUI()
    {
        _affectVector.DrawAffectVectorGUI();
        GUILayout.Label($"Last action: {_lastAction}");
        GUILayout.Label($"Modifier: {_modifier}");
        GUILayout.Label($"Prevailing affect: {_affecter.GetPrevailingAffect(_affectVector)}");
    }

    public string GetPrevailingAffect()
    {
        return _affecter.GetPrevailingAffect(_affectVector);
    }
}