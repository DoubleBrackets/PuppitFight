using UnityEngine;

public class BossAttacking : MonoBehaviour
{
    [SerializeField]
    private Transform _target;

    [SerializeField]
    private Projectile _bulletPrefab;

    [SerializeField]
    private float _cooldown;

    [SerializeField]
    private float _spread;

    private float _timer;

    private void Update()
    {
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