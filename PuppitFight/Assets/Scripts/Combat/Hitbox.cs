using System;
using UnityEngine;

public struct AttackData
{
    public float Damage;
    public float Knockback;
    public Vector2 Direction;
}

public class Hitbox : MonoBehaviour
{
    public event Action<AttackData> OnHit;

    public void Hit(AttackData attackData)
    {
        OnHit?.Invoke(attackData);
    }
}