using UnityEngine;

public class Boss : MonoBehaviour
{
    [SerializeField]
    private Transform _target;

    [SerializeField]
    private Projectile _bulletPrefab;

    [SerializeField]
    private float _cooldown;

    [SerializeField]
    private float _spread;

    [SerializeField]
    private float _tetherDistance;

    [SerializeField]
    private float _acceleration;

    [SerializeField]
    private float _speed;

    [SerializeField]
    private Rigidbody2D _rb;

    private float _timer;

    private void Update()
    {
        Vector2 vectorToTarget = _target.position - transform.position;

        Vector2 targetVelocity = vectorToTarget.normalized * _speed;
        if (vectorToTarget.magnitude < _tetherDistance)
        {
            targetVelocity *= -1;
        }

        _rb.velocity = Vector2.MoveTowards(_rb.velocity, targetVelocity, _acceleration * Time.deltaTime);

        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
        }

        if (_timer <= 0)
        {
            _timer = _cooldown;
            Shoot();
        }
    }

    private void Shoot()
    {
        Projectile bullet = Instantiate(_bulletPrefab, transform.position, Quaternion.identity);
        Vector2 direction = (_target.position - transform.position).normalized;
        // add some randomness to the direction
        direction = Quaternion.Euler(0, 0, Random.Range(-_spread, _spread)) * direction;

        bullet.Launch(transform.position, direction);
    }
}