using System;
using System.Collections.Generic;
using System.IO;
using Puppitor;
using UnityEngine;

public class Puppit : MonoBehaviour
{
    [SerializeField]
    [TextArea]
    private string _affectRuleFilePath;

    private AffectVector _affectVector;
    private Affecter _affecter;

    private string _lastAction;
    private string _modifier;

    private void Start()
    {
        SetupAffector();
        OnGUIService.Instance.OnGUIEvent += DrawDebugGUI;
    }

    private void Update()
    {
        List<string> affectNames = _affecter.GetAllActionNames();
        List<string> modifierNames = _affecter.GetAllModifierNames();

        var start = (int)KeyCode.A;
        for (var i = 0; i < modifierNames.Count; i++)
        {
            if (Input.GetKeyDown((KeyCode)(start + i)))
            {
                _modifier = modifierNames[i];
            }
        }

        start = (int)KeyCode.Alpha1;
        for (var i = 0; i < affectNames.Count; i++)
        {
            if (Input.GetKey((KeyCode)(start + i)))
            {
                _lastAction = affectNames[i];
                _affecter.UpdateAffect(
                    _affectVector,
                    affectNames[i],
                    "neutral",
                    Time.deltaTime);
                break;
            }
        }
    }

    private void OnDestroy()
    {
        OnGUIService.Instance.OnGUIEvent -= DrawDebugGUI;
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
}