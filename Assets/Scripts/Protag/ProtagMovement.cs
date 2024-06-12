using UnityEngine;

public class ProtagMovement : MonoBehaviour
{
    [SerializeField]
    private float _speed;

    [SerializeField]
    private float _accel;

    [SerializeField]
    private Rigidbody2D _rb;

    private void Update()
    {
        var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _rb.velocity = Vector2.MoveTowards(_rb.velocity, input.normalized * _speed, _accel * Time.deltaTime);
    }
}