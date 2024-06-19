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
    private VoidEvent _onDamage;

    [SerializeField]
    private FloatEvent _onMaxHealthSet;

    [SerializeField]
    private VoidEvent _onDeath;

    [Header("Entity Stats")]

    [SerializeField]
    private float _maxHealth;

    [SerializeField]
    private float _regenInterval;

    private float _currentHealth;

    private bool _isDead;

    private float regenTimer;

    private void Start()
    {
        _onMaxHealthSet.Raise(_maxHealth);
        _currentHealth = _maxHealth;
        _onHealthChanged.Raise(_currentHealth);
        _hitbox.OnHit += HandleOnHit;
    }

    private void Update()
    {
        regenTimer += Time.deltaTime;
        if (regenTimer >= _regenInterval)
        {
            Regen();
            regenTimer = 0;
        }
    }

    private void OnDestroy()
    {
        _hitbox.OnHit -= HandleOnHit;
    }

    private void HandleOnHit(AttackData attackData)
    {
        TakeDamage(attackData.Damage);
    }

    public void Regen()
    {
        if (_isDead)
        {
            return;
        }

        _currentHealth++;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        _onHealthChanged.Raise(_currentHealth);
    }

    public void TakeDamage(float damage)
    {
        if (_isDead)
        {
            return;
        }

        _currentHealth -= damage;

        _onHealthChanged.Raise(_currentHealth);
        _onDamage.Raise();

        if (_currentHealth <= 0)
        {
            _onDeath.Raise();
            _isDead = true;
        }
    }
}