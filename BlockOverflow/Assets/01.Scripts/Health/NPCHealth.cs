using System;
using UnityEngine;

public class NPCHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float npcHealth = 10f;
    public float CurrentHealth { get; private set; }
    public float MaxHealth => npcHealth;
    public bool IsDead => CurrentHealth <= 0f;
    
    public Action<int> OnDeath;
    public Action OnHit;
    
    private void Awake()
    {
        CurrentHealth = npcHealth;
    }
    public void TakeDamage(float damageAmount, int damagingPlayerIdx = -1)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damageAmount);

        Debug.Log(damagingPlayerIdx);
        if (IsDead)
        {
            OnDeath?.Invoke(damagingPlayerIdx);
            Die();
        }
        
        else OnHit?.Invoke();
    }
    
    
    
    public void Die()
    {
    }
    
    private void OnEnable()
    {
        CurrentHealth = npcHealth;
    }
}
