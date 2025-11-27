using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damageAmount, int damagingPlayerIdx = -1);
    bool IsDead { get; }
    float CurrentHealth { get; }
    float MaxHealth { get; }

    void Die();

}
