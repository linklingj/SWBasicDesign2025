using System;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    
    [SerializeField] private float maxHealth = 100f;
    public float MaxHealth => maxHealth;
    public float FinalMaxHealth => maxHealth + stats.healthIncrease;
    [ReadOnly, ShowInInspector] public float CurrentHealth{get; private set;}
    public bool IsDead { get; private set; }
    public bool TookDamageThisFrame { get; private set; }

    [SerializeField] private PlayerStats stats;
    [SerializeField] private SpriteRenderer sr;
    private Color originalColor;
    [SerializeField] private Color hurtColor;
    
    public Action OnDeath;

    private void Awake()
    {
        CurrentHealth = maxHealth + stats.healthIncrease;
        IsDead = false;
        if (sr != null)
        {
            originalColor = sr.color;
        }
    }
    
    public void TakeDamage(float damageAmount)
    {
        if (IsDead) return;
    
        TookDamageThisFrame = true;
        CurrentHealth = Mathf.Max(0, CurrentHealth - damageAmount);
        // 혹시 전에 돌던 코루틴 있으면 끊고 다시
        //StopCoroutine(nameof(HitFlash));
        StartCoroutine(HitFlash());
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
        
        OnDeath?.Invoke();
    }
    
    public void Heal(float healAmount)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + healAmount);
    }
    
    public void Spawn()
    {
        IsDead = false;
        CurrentHealth = maxHealth + stats.healthIncrease;
    }
    
    private IEnumerator HitFlash()
    {
        if (sr == null)
        {
            yield break;
        }
        sr.color = hurtColor;
        yield return new WaitForSeconds(0.1f);
        sr.color = originalColor;
        
    }
    
    private void LateUpdate()
    {
        TookDamageThisFrame = false;
    }
    
}