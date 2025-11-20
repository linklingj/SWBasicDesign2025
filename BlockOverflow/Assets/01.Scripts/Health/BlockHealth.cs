using UnityEngine;

public class BlockHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float blockHealth = 10f;
    public float CurrentHealth { get; private set; }
    public float MaxHealth => blockHealth;
    public bool IsDead => CurrentHealth <= 0f;
    
    private void Awake()
    {
        CurrentHealth = blockHealth;
    }
    public void TakeDamage(float damageAmount)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - 1); // 블럭은 타수 기반
        
        if (IsDead) Die();
    }
    
    public void Die()
    {
        Destroy(gameObject);
        //SetActive(false);
    }
    
    private void OnEnable()
    {
        CurrentHealth = blockHealth;
        
    }
    
    

}
