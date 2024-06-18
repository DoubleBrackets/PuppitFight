using UnityEngine;

public class StaticModifierProvider : IModifierProvider
{
    [SerializeField]
    private string _staticModifier;

    public string GetCurrentModifier()
    {
        return _staticModifier;
    }
}