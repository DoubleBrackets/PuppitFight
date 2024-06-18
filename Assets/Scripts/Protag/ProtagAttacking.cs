using System;
using UnityEngine;

public class ProtagAttacking : MonoBehaviour, IActionProvider, IModifierProvider
{
    private enum Modifiers
    {
        Towards,
        Away,
        Neutral
    }

    private enum Actions
    {
        Shoot,
        Resting
    }

    [SerializeField]
    private Projectile _bulletPrefab;

    [SerializeField]
    private Actions _equilibriumAction;

    [SerializeField]
    private Transform _target;

    [SerializeField]
    private float _cooldown;

    private float _timer;

    private Modifiers _currentModifier = Modifiers.Neutral;

    private void Update()
    {
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
        }

        if (Input.GetMouseButton(0) && _timer <= 0)
        {
            _timer = _cooldown;
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
            _currentModifier = Modifiers.Towards;
        }
        else
        {
            _currentModifier = Modifiers.Away;
        }

        bullet.Launch(transform.position,
            shootDir);

        OnOneshotAction?.Invoke(Actions.Shoot.ToString().ToLower());
    }
}