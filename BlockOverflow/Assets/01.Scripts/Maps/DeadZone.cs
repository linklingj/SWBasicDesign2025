using UnityEngine;

public class DeadZone : MonoBehaviour
{
    
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var damageable = collision.collider.GetComponentInParent<IDamageable>(); // 여기
        Debug.Log("hello");
        if(damageable != null)
        {
            damageable.Die();
        }
    }
}
