using System;
using UnityEngine;

public class ProtagMovement : MonoBehaviour, IModifierProvider, IActionProvider
{
    [SerializeField]
    private float _speed;

    [SerializeField]
    private float _accel;

    [SerializeField]
    private Rigidbody2D _rb;

    [SerializeField]
    private Transform _target;

    private AffectTypes.MovementModifiers _currentModifier = AffectTypes.MovementModifiers.Neutral;

    private AffectTypes.MovementActions _currentAction = AffectTypes.MovementActions.Resting;

    private void Update()
    {
        var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        Vector3 vectorToTarget = _target.position - transform.position;

        CalculateModifier(input, vectorToTarget);
        CalculateAction(input);

        // Hard coded pair
        if (_currentAction == AffectTypes.MovementActions.Nothing)
        {
            _currentModifier = AffectTypes.MovementModifiers.Neutral;
        }

        _rb.velocity = Vector2.MoveTowards(_rb.velocity, input.normalized * _speed, _accel * Time.deltaTime);
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
            _currentAction = AffectTypes.MovementActions.Nothing;
        }
    }
}