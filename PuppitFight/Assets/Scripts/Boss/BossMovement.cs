using System;
using UnityEngine;

public class BossMovement : MonoBehaviour, IModifierProvider, IActionProvider
{
    private enum Modifiers
    {
        Towards,
        Away,
        Sideways,
        Neutral
    }

    private enum Actions
    {
        Moving,
        Nothing,
        Resting
    }

    [SerializeField]
    private Transform _target;

    [SerializeField]
    private float _tetherDistance;

    [SerializeField]
    private float _acceleration;

    [SerializeField]
    private float _speed;

    [SerializeField]
    private Rigidbody2D _rb;

    private Modifiers _currentModifier = Modifiers.Neutral;
    private Actions _currentAction = Actions.Resting;

    private void Update()
    {
        Vector2 vectorToTarget = _target.position - transform.position;

        Vector2 targetVelocity = vectorToTarget.normalized * _speed;
        if (vectorToTarget.magnitude < _tetherDistance)
        {
            targetVelocity *= -1;
        }

        _rb.velocity = Vector2.MoveTowards(_rb.velocity, targetVelocity, _acceleration * Time.deltaTime);

        CalculateModifier(_rb.velocity, vectorToTarget);
        CalculateAction(_rb.velocity);
    }

    public string GetCurrentAction()
    {
        return _currentAction.ToString().ToLower();
    }

    public event Action<string> OnOneshotAction;

    public string GetCurrentModifier()
    {
        return _currentModifier.ToString().ToLower();
    }

    private void CalculateModifier(Vector2 input, Vector2 vecToTarget)
    {
        float dot = Vector2.Dot(vecToTarget.normalized, input);

        if (dot > 0.6f)
        {
            _currentModifier = Modifiers.Towards;
        }
        else if (dot < -0.6f)
        {
            _currentModifier = Modifiers.Away;
        }
        else
        {
            _currentModifier = Modifiers.Sideways;
        }
    }

    private void CalculateAction(Vector2 input)
    {
        if (input.magnitude > 0)
        {
            _currentAction = Actions.Moving;
        }
        else
        {
            _currentAction = Actions.Nothing;
        }
    }
}