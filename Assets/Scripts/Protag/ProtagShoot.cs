using UnityEngine;

public class ProtagShoot : MonoBehaviour
{
    [SerializeField]
    private Projectile _bulletPrefab;

    [SerializeField]
    private float _cooldown;

    private float _timer;

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

    private void Shoot()
    {
        Projectile bullet = Instantiate(_bulletPrefab, transform.position, Quaternion.identity);
        bullet.Launch(transform.position,
            (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized);
    }
}