using System;
using System.Collections.Generic;
using System.IO;
using Puppitor;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
///     Monobehavior that wraps an Affector and links it to various controllers (e.g Action Provider). Has a target
///     AffectVector
/// </summary>
public class PuppitLimb : MonoBehaviour
{
    [SerializeField]
    [TextArea]
    private string _affectRuleFilePath;

    [SerializeField]
    private AffectVectorContainer _affectVectorContainer;

    [SerializeField]
    private GameObject _modifierProviderContainer;

    [SerializeField]
    private GameObject _actionProviderContainer;

    [SerializeField]
    private string _equilibriumAction;

    private AffectVector AffectVector => _affectVectorContainer.AffectVector;

    public event Action OnFinishSetup;

    private Affecter _affecter;

    private string _lastAction;
    private string _lastOneshotAction;
    private int _oneshotActionCount;
    private string _modifier;

    private IModifierProvider _modifierProvider;
    private IActionProvider _actionProvider;

    private void Start()
    {
        SetupAffector();
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
            ApplyAffector(AffectVector, _lastAction, _modifier, Time.deltaTime);
        }
    }

    private void OnDestroy()
    {
        OnGUIService.Instance.OnGUIEvent -= DrawDebugGUI;
    }

    private void OnValidate()
    {
        if (_modifierProviderContainer.GetComponent<IModifierProvider>() == null)
        {
            Debug.LogWarning("No modifier provider found");
        }

        if (_actionProviderContainer.GetComponent<IActionProvider>() == null)
        {
            Debug.LogWarning("No action provider found");
        }
    }

    private void SetModifierProvider(IModifierProvider modifierProvider)
    {
        _modifierProvider = modifierProvider;
    }

    private void SetActionProvider(IActionProvider actionProvider)
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
                AffectVector,
                actionName,
                _modifierProvider.GetCurrentModifier(),
                Time.deltaTime);

            if (_lastOneshotAction != actionName)
            {
                _oneshotActionCount = 0;
            }

            _lastOneshotAction = actionName;
            _oneshotActionCount++;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error firing oneshot action: {e}");
            throw;
        }
    }

    /// <summary>
    ///     Wraps the Affector's UpdateAffect method. Intended for use with AI action selection
    /// </summary>
    public void ApplyAffector(AffectVector affectVector, string actionName, string modifierName, float multiplier)
    {
        try
        {
            _affecter.UpdateAffect(affectVector, actionName, modifierName, multiplier);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error applying affector: {e}");
            throw;
        }
    }

    private void SetupAffector()
    {
        try
        {
            UnityWebRequest req;
            var jsonString = string.Empty;
            string path = Application.streamingAssetsPath + "/" + _affectRuleFilePath;
            Debug.Log($"path: {path}");
#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log("Unity webgl");
            UnityWebRequest request = UnityWebRequest.Get(path);
            UnityWebRequestAsyncOperation handler = request.SendWebRequest();

            handler.completed += (op) =>
            {
                jsonString = handler.webRequest.downloadHandler.text;
                FinishSetup(jsonString);
            };

#else
            jsonString =
                File.ReadAllText(path);
            Debug.Log("File read. JSON: " + jsonString);
            FinishSetup(jsonString);
#endif
        }
        catch (Exception e)
        {
            Debug.LogError($"Error setting up actor {e}");
            throw;
        }
    }

    private void FinishSetup(string jsonString)
    {
        _affecter = new Affecter(jsonString, equilibriumAction: _equilibriumAction);

        // Append, in case multiple affectors are sharing the same affect vector
        _affecter.AppendToAffectVector(AffectVector);

        var modifierProvider = _modifierProviderContainer.GetComponent<IModifierProvider>();
        if (modifierProvider == null)
        {
            Debug.LogWarning("No modifier provider found");
        }
        else
        {
            SetModifierProvider(modifierProvider);
        }

        var actionProvider = _actionProviderContainer.GetComponent<IActionProvider>();
        if (actionProvider == null)
        {
            Debug.LogWarning("No action provider found");
        }
        else
        {
            SetActionProvider(actionProvider);
        }

        OnFinishSetup?.Invoke();
    }

    private void DrawDebugGUI()
    {
        AffectVector.DrawAffectVectorGUI();
        GUILayout.Label($"Last action: {_lastAction}");
        GUILayout.Label($"Modifier: {_modifier}");
        GUILayout.Label($"Prevailing affect: {_affecter.GetPrevailingAffect(AffectVector)}");
        GUILayout.Label($"Oneshot action: {_lastOneshotAction} ({_oneshotActionCount})");
    }

    public string GetPrevailingAffect()
    {
        return _affecter.GetPrevailingAffect(AffectVector);
    }

    public List<string> GetAllActionNames()
    {
        return _affecter.GetAllActionNames();
    }

    public List<string> GetAllModifierNames()
    {
        return _affecter.GetAllModifierNames();
    }

    public AffectVector GetAffectVectorCopy()
    {
        return new AffectVector(AffectVector);
    }

    public AffectVector MakeAffectVector()
    {
        return _affecter.MakeAffectVector();
    }
}