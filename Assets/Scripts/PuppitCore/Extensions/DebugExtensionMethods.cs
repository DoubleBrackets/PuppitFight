using System.Collections.Generic;
using Puppitor;
using UnityEngine;

/// <summary>
///     Extension methods for drawing debug information relating to Puppits
/// </summary>
public static class DebugExtensionMethods
{
    public static void LogAffectVector(this AffectVector affectVector)
    {
        var affectVectorString = string.Empty;
        foreach (KeyValuePair<string, double> affect in affectVector)
        {
            affectVectorString += $"{affect.Key}: {affect.Value:F3}\n";
        }

        Debug.Log(affectVectorString);
    }

    public static void LogPrevailingAffect(this AffectVector affectVector)
    {
        List<string> prevailingAffects = affectVector.GetAllPrevailingAffects();
        var prevailingAffectsString = string.Empty;
        foreach (string affect in prevailingAffects)
        {
            prevailingAffectsString += $"{affect}\n";
        }

        Debug.Log(prevailingAffectsString);
    }

    public static void DrawAffectVectorGUI(this AffectVector affectVector)
    {
        foreach (KeyValuePair<string, double> affect in affectVector)
        {
            GUILayout.Label($"{affect.Key}: {affect.Value:F3}");
        }
    }
}