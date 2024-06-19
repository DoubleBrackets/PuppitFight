using UnityEngine;
using UnityEngine.Events;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private float _speed;

    [SerializeField]
    private float _knockback;

    [SerializeField]
    private float _damage;

    [SerializeField]
    private float _lifetime;

    [SerializeField]
    private LayerMask _hitLayer;

    [SerializeField]
    private Rigidbody2D _rb;

    [SerializeField]
    private UnityEvent _onHit;

    private float _timer;

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _lifetime)
        {
            _timer = float.MinValue;
            _onHit.Invoke();
            _rb.isKinematic = true;
            Destroy(gameObject, 1f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Hitbox hitbox))
        {
            if (_hitLayer.value != (_hitLayer.value | (1 << other.gameObject.layer)))
            {
                return;
            }

            hitbox.Hit(new AttackData
            {
                Direction = _rb.velocity.normalized,
                Knockback = _knockback,
                Damage = _damage
            });

            _onHit.Invoke();
            _rb.isKinematic = true;
            Destroy(gameObject, 1f);
        }
    }

    public void Launch(Vector2 pos, Vector2 direction)
    {
        transform.position = pos;
        _rb.velocity = direction.normalized * _speed;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
}