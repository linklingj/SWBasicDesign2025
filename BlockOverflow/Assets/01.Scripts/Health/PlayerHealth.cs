using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    
    [SerializeField] private float maxHealth = 100f;
    public float MaxHealth => maxHealth;
    
    [ReadOnly, ShowInInspector] public float CurrentHealth{get; private set;}
    public bool IsDead { get; private set; }

    private void Awake()
    {
        //gameObject.SetActive(true);
        CurrentHealth = maxHealth;
        IsDead = false;
    }


    public void TakeDamage(float damageAmount)
    {
        if (IsDead) return;
    
        CurrentHealth = Mathf.Max(0, CurrentHealth - damageAmount);
        if (CurrentHealth <= 0f)
        {
            CurrentHealth = 0f;
            Die();
        }
    }
    public void Die()
    {
        if (IsDead) return;

        IsDead = true;
        Debug.Log($"{gameObject.name} has died.");
        gameObject.SetActive(false); //걍 꺼도 됨?
        
        
    }
    
    public void Heal(float healAmount)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + healAmount);
    }
    
    public void Respawn()
    {
        IsDead = false;
        CurrentHealth = maxHealth;
        gameObject.SetActive(true);
    }
}
