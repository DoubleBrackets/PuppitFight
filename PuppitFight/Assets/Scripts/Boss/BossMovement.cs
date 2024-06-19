using System;
using System.Linq;
using UnityEngine;

public class BossMovement : MonoBehaviour, IModifierProvider, IActionProvider
{
    [SerializeField]
    private PuppitGreedySelector _greedySelector;

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

    private AffectTypes.MovementModifiers _currentModifier = AffectTypes.MovementModifiers.Neutral;
    private AffectTypes.MovementActions _currentAction = AffectTypes.MovementActions.Resting;

    private void Update()
    {
        Vector2 vectorToTarget = _target.position - transform.position;
        Vector2 input = Vector2.zero;
        Vector2 targetVelocity = Vector2.zero;

        PuppitGreedySelector.Selection selectedActionModifier = _greedySelector.Selections.First();

        if (selectedActionModifier.Action == AffectTypes.MovementActions.Moving.ToName())
        {
            if (selectedActionModifier.Modifier == AffectTypes.MovementModifiers.Towards.ToName())
            {
                targetVelocity = vectorToTarget.normalized * _speed;
            }
            else if (selectedActionModifier.Modifier == AffectTypes.MovementModifiers.Away.ToName())
            {
                targetVelocity = -vectorToTarget.normalized * _speed;

                if (vectorToTarget.magnitude > _tetherDistance)
                {
                    targetVelocity *= -1;
                }
            }
            else if (selectedActionModifier.Modifier == AffectTypes.MovementModifiers.Sideways.ToName())
            {
                targetVelocity = new Vector2(vectorToTarget.y, -vectorToTarget.x).normalized * _speed;
            }
            else
            {
                targetVelocity = Vector2.zero;
            }
        }
        else
        {
            targetVelocity = Vector2.zero;
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
            _currentModifier = AffectTypes.MovementModifiers.Towards;
        }
        else if (dot < -0.6f)
        {
            _currentModifier = AffectTypes.MovementModifiers.Away;
        }
        else
        {
            _currentModifier = AffectTypes.MovementModifiers.Sideways;
        }
    }

    private void CalculateAction(Vector2 input)
    {
        if (input.magnitude > 0)
        {
            _currentAction = AffectTypes.MovementActions.Moving;
        }
        else
        {
            _currentAction = AffectTypes.MovementActions.Resting;
        }
    }
}