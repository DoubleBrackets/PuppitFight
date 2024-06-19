using System;
using UnityEngine;

/// <summary>
///     Gets the target affect by manually configured mapping based on a source affect
/// </summary>
public class TargetAffectMapper : MonoBehaviour, IAffectProvider
{
    [Serializable]
    public struct AffectMapping
    {
        public string SourceAffect;
        public string TargetAffect;
    }

    [SerializeField]
    private PuppitLimb _puppitLimb;

    [SerializeField]
    private AffectMapping[] _affectMappings;

    public string GetCurrentAffectName()
    {
        string sourceAffect = _puppitLimb.GetPrevailingAffect();
        foreach (AffectMapping mapping in _affectMappings)
        {
            if (mapping.SourceAffect == sourceAffect)
            {
                return mapping.TargetAffect;
            }
        }

        Debug.LogError($"No mapping found for source affect {sourceAffect}");
        return string.Empty;
    }
}