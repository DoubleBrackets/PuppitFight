using Events;
using UnityEngine;

public class CombatEntity : MonoBehaviour
{
    [Header("Dependencies")]

    [SerializeField]
    private Hitbox _hitbox;

    [Header("Emitting Events")]

    [SerializeField]
    private FloatEvent _onHealthChanged;

    [SerializeField]
    private FloatEvent _onMaxHealthSet;

    [SerializeField]
    private VoidEvent _onDeath;

    [Header("Entity Stats")]

    [SerializeField]
    private float _maxHealth;

    private float _currentHealth;

    private bool _isDead;

    private void Start()
    {
        _onMaxHealthSet.Raise(_maxHealth);
        _currentHealth = _maxHealth;
        _onHealthChanged.Raise(_currentHealth);
        _hitbox.OnHit += HandleOnHit;
    }

    private void OnDestroy()
    {
        _hitbox.OnHit -= HandleOnHit;
    }

    private void HandleOnHit(AttackData attackData)
    {
        TakeDamage(attackData.Damage);
    }

    public void TakeDamage(float damage)
    {
        if (_isDead)
        {
            return;
        }

        _currentHealth -= damage;

        _onHealthChanged.Raise(_currentHealth);

        if (_currentHealth <= 0)
        {
            _onDeath.Raise();
            _isDead = true;
        }
    }
}