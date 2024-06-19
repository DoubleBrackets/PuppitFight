using System;
using UnityEngine;

public class ProtagAttacking : MonoBehaviour, IActionProvider, IModifierProvider
{
    [SerializeField]
    private PuppitLimb _puppitLimb;

    [SerializeField]
    private Projectile _bulletPrefab;

    [SerializeField]
    private AffectTypes.AttackActions _equilibriumAction;

    [SerializeField]
    private Transform _target;

    [SerializeField]
    private float _cooldown;

    private float _timer;

    private AffectTypes.AttackModifiers _currentModifier = AffectTypes.AttackModifiers.Neutral;

    private void Update()
    {
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
        }

        _currentModifier = AffectTypes.AttackModifiers.Neutral;

        if (Input.GetMouseButton(0) && _timer <= 0)
        {
            Shoot();
        }
    }

    public string GetCurrentAction()
    {
        return _equilibriumAction.ToString().ToLower();
    }

    public event Action<string> OnOneshotAction;

    public string GetCurrentModifier()
    {
        return _currentModifier.ToString().ToLower();
    }

    private void Shoot()
    {
        Projectile bullet = Instantiate(_bulletPrefab, transform.position, Quaternion.identity);

        Vector2 shootDir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        shootDir.Normalize();

        Vector2 targetDirection = _target.position - transform.position;
        targetDirection.Normalize();

        if (Vector2.Dot(shootDir, targetDirection) > 0)
        {
            _currentModifier = AffectTypes.AttackModifiers.Towards;
        }
        else
        {
            _currentModifier = AffectTypes.AttackModifiers.Away;
        }

        bullet.Launch(transform.position,
            shootDir);

        // Express aggression with fast cooldown
        if (_puppitLimb.GetPrevailingAffect() == AffectTypes.Emotions.Aggressive.ToName())
        {
            _timer = _cooldown / 2;
        }
        else
        {
            _timer = _cooldown;
        }

        OnOneshotAction?.Invoke(AffectTypes.AttackActions.Shoot.ToName());
    }
}