using System;

/// <summary>
///     Generic interface that provides a current action, applied continuously over time and an event for "oneshot" actions
/// </summary>
public interface IActionProvider
{
    string GetCurrentAction();
    event Action<string> OnOneshotAction;
}