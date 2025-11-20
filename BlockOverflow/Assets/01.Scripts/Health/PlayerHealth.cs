using System;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;
using DG.Tweening;

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
        if (DamageTextSpawner.Instance) DamageTextSpawner.Instance.ShowDamage(damageAmount, transform.position);

        
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
        
        DOTween.Kill(transform);  // (선택) 혹시 모를 이전 트윈 정리

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOMoveY(transform.position.y + 4f, 0.3f).SetEase(Ease.OutQuad))  // 위로
            .Append(transform.DOMoveY(transform.position.y, 0.5f).SetEase(Ease.InQuad)); // 아래로
        
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