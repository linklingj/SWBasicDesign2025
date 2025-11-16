using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damageAmount);
    bool IsDead { get; }
    float CurrentHealth { get; }
    float MaxHealth { get; }

    void Die();


}
