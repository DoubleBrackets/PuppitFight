using System.Collections.Generic;
using Puppitor;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
///     Simple greedy algorithm that selects an Action/Modifier pair for NPC AI
/// </summary>
public class PuppitGreedySelector : MonoBehaviour
{
    public struct Selection
    {
        public string Action;
        public string Modifier;
        public double Score;
    }

    [FormerlySerializedAs("_puppit")]
    [SerializeField]
    private PuppitLimb _puppitLimb;

    [SerializeField]
    private GameObject _affectProviderContainer;

    [SerializeField]
    private bool _isOneshots;

    public IEnumerable<Selection> Selections => _selections;

    private readonly List<Selection> _selections = new();

    private List<string> _actionNames;
    private List<string> _modifierNames;

    private IAffectProvider _targetAffectProvider;

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
        _selections.Clear();

        AffectVector currentAffectVector = _puppitLimb.GetAffectVectorCopy();

        // The "goal" affect vector is one where the target affect provider is 1.0f
        AffectVector targetAffectVector = _puppitLimb.MakeAffectVector();
        string targetAffectName = _targetAffectProvider.GetCurrentAffectName();
        targetAffectVector[targetAffectName] = 1.0f;

        targetAffectVector.Normalize();

        // Try all possible action/modifier pairs and select the one that gets closest to the target affect vector
        foreach (string action in _actionNames)
        {
            foreach (string modifier in _modifierNames)
            {
                var newAffectVector = new AffectVector(currentAffectVector);
                float multiplier = _isOneshots ? 1.0f : Time.deltaTime;
                _puppitLimb.ApplyAffector(newAffectVector, action, modifier, multiplier);

                newAffectVector.Normalize();

                double score = targetAffectVector.DotProduct(newAffectVector);

                _selections.Add(new Selection
                {
                    Action = action,
                    Modifier = modifier,
                    Score = score
                });
            }
        }

        // Sort the selections by score, descending
        _selections.Sort((a, b) => b.Score.CompareTo(a.Score));
    }

    private void OnValidate()
    {
        if (_affectProviderContainer.GetComponent<IAffectProvider>() == null)
        {
            Debug.LogError("No IAffectProvider found in container");
        }
    }

    private void Setup()
    {
        _actionNames = _puppitLimb.GetAllActionNames();
        _modifierNames = _puppitLimb.GetAllModifierNames();
    }
}