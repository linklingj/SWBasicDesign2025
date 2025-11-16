using System;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    
    [SerializeField] private float maxHealth = 100f;
    public float MaxHealth => maxHealth;
    [ReadOnly, ShowInInspector] public float CurrentHealth{get; private set;}
    public bool IsDead { get; private set; }
    
    [SerializeField] private SpriteRenderer sr;
    private Color originalColor;
    
    

    private void Awake()
    {
        //gameObject.SetActive(true);
        CurrentHealth = maxHealth;
        IsDead = false;
        if (sr != null)
        {
            originalColor = sr.color;
            //sr.color = Color.green;
        }
    }


    public void TakeDamage(float damageAmount)
    {
        if (IsDead) return;
    
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
    
    private IEnumerator HitFlash()
    {
        if (sr == null)
        {
            Debug.LogWarning("HitFlash: sr가 null 입니다!", this);
            yield break;
        }
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = originalColor;
        
    }
    
    
    
}
