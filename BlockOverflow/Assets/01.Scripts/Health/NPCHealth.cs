using System;
using UnityEngine;

public class NPCHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float npcHealth = 10f;
    public float CurrentHealth { get; private set; }
    public float MaxHealth => npcHealth;
    public bool IsDead => CurrentHealth <= 0f;
    
    public Action OnDeath;
    public Action OnHit;
    
    private void Awake()
    {
        CurrentHealth = npcHealth;
    }
    public void TakeDamage(float damageAmount)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damageAmount);
        
        if (IsDead) Die();
        else OnHit?.Invoke();
    }
    
    public void Die()
    {
        OnDeath?.Invoke();
    }
    
    private void OnEnable()
    {
        CurrentHealth = npcHealth;
    }
}
