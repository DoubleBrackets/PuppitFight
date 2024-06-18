using System.Collections.Generic;
using Puppitor;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
///     Simple greedy algorithm that selects an Action/Modifier pair for NPC AI
/// </summary>
public class PuppitGreedySelector : MonoBehaviour
{
    [FormerlySerializedAs("_puppit")]
    [SerializeField]
    private PuppitLimb _puppitLimb;

    [SerializeField]
    private GameObject _affectProviderContainer;

    public string SelectedAction => _selectedAction;
    public string SelectedModifier => _selectedModifier;

    private List<string> _actionNames;
    private List<string> _modifierNames;

    private IAffectProvider _targetAffectProvider;

    private string _selectedAction;
    private string _selectedModifier;

    private void Awake()
    {
        _puppitLimb.OnFinishSetup += Setup;
        _targetAffectProvider = _affectProviderContainer.GetComponent<IAffectProvider>();
        if (_targetAffectProvider == null)
        {
            Debug.LogError("No IAffectProvider found in container");
        }
    }

    private void Update()
    {
        AffectVector currentAffectVector = _puppitLimb.GetAffectVectorCopy();

        // The "goal" affect vector is one where the target affect provider is 1.0f
        AffectVector targetAffectVector = _puppitLimb.MakeAffectVector();
        targetAffectVector[_targetAffectProvider.GetCurrentAffectName()] = 1.0f;

        double bestScore = targetAffectVector.DotProduct(currentAffectVector);

        // Try all possible action/modifier pairs and select the one that gets closest to the target affect vector
        foreach (string action in _actionNames)
        {
            foreach (string modifier in _modifierNames)
            {
                var newAffectVector = new AffectVector(currentAffectVector);
                _puppitLimb.ApplyAffector(newAffectVector, action, modifier);

                double score = targetAffectVector.DotProduct(newAffectVector);

                if (score > bestScore)
                {
                    bestScore = score;
                    _selectedAction = action;
                    _selectedModifier = modifier;
                }
            }
        }
    }

    private void Setup()
    {
        _actionNames = _puppitLimb.GetAllActionNames();
        _modifierNames = _puppitLimb.GetAllModifierNames();
    }
}