using System;

/// <summary>
///     Utility class for getting typing for the action/modifier names. Not the best approach but dirty and prevents magic
///     strings
/// </summary>
public static class AffectTypes
{
    public enum Emotions
    {
        Aggressive,
        Fear,
        Wary,
        Confusion
    }

    public enum MovementActions
    {
        Moving,
        Nothing,
        Resting
    }

    public enum AttackActions
    {
        Shoot,
        Resting
    }

    public enum MovementModifiers
    {
        Towards,
        Away,
        Sideways,
        Neutral
    }

    public enum AttackModifiers
    {
        Towards,
        Away,
        Neutral
    }

    public static string ToName<T>(this T enumValue) where T : Enum
    {
        return enumValue.ToString().ToLower();
    }
}