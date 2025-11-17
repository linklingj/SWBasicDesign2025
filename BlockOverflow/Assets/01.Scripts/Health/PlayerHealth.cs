using System;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    public float MaxHealth => maxHealth;
    public float extraHealth = 0;
    public float FinalMaxHealth => maxHealth + extraHealth;
    [ReadOnly, ShowInInspector] public float CurrentHealth{get; private set;}
    public bool IsDead { get; private set; }
    public bool TookDamageThisFrame { get; private set; }

    [SerializeField] private SpriteRenderer sr;
    private Color originalColor;
    [SerializeField] private Color hurtColor;
    
    public Action OnDeath;

    private void Awake()
    {
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
        //일단 막아놓기
        //if (DamageTextSpawner.Instance) DamageTextSpawner.Instance.ShowDamage(damageAmount, transform.position);

        
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
    
    public void Spawn(float extraHealth)
    {
        this.extraHealth = extraHealth;
        CurrentHealth = maxHealth + extraHealth;
        IsDead = false;
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