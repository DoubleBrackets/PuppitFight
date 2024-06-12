using System;
using UnityEngine;

public class ProtagMovement : MonoBehaviour, IModifierProvider, IActionProvider
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
    private float _speed;

    [SerializeField]
    private float _accel;

    [SerializeField]
    private Rigidbody2D _rb;

    [SerializeField]
    private Transform _target;

    [SerializeField]
    private Puppit _puppit;

    private Modifiers _currentModifier = Modifiers.Neutral;

    private Actions _currentAction = Actions.Resting;

    private void Start()
    {
        _puppit.SetActionProvider(this);
        _puppit.SetModifierProvider(this);
    }

    private void Update()
    {
        var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        Vector3 vectorToTarget = _target.position - transform.position;

        CalculateModifier(input, vectorToTarget);
        CalculateAction(input);

        // Hard coded pair
        if (_currentAction == Actions.Nothing)
        {
            _currentModifier = Modifiers.Neutral;
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