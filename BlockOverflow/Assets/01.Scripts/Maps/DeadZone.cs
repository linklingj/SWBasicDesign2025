using UnityEngine;

public class DeadZone : MonoBehaviour
{
    
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var damageable = collision.collider.GetComponentInParent<IDamageable>();
        if(damageable != null)
        {
            damageable.Die();
        }
    }
}
