using UnityEngine;

public class Bullet : PoolObject {
    [SerializeField] protected BulletData bulletData;
    
    [Header("Impact (pooled)")]
    [SerializeField] private GameObject impactPrefab;    // 오브젝트 풀에 등록된 임팩트 프리팹
    [SerializeField] private float impactOffset = 0.02f; // 벽에 반쯤 박히지 않게 약간 앞쪽으로

    protected Rigidbody2D rb;
    protected float speed;
    protected float range;
    protected float damage;
    protected Vector2 moveDir;
    protected Vector2 startPosition;
    
    
    
    private bool released;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector3 direction)
    {
        released = false;
        if (!rb) rb = GetComponent<Rigidbody2D>();

        if (bulletData == null)
        {
            Debug.LogWarning($"{name} is missing BulletData.", this);
            Release();
            return;
        }

        speed = bulletData.bulletSpeed;
        range = bulletData.range;
        Vector2 planarDirection = new Vector2(direction.x, direction.y);
        moveDir = planarDirection.sqrMagnitude > 0f ? planarDirection.normalized : Vector2.zero;
        startPosition = rb ? rb.position : (Vector2)transform.position;

        if (moveDir == Vector2.zero || speed <= 0f)
        {
            Release();
        }
    }

    private void FixedUpdate()
    {
        MoveBullet();
    }

    public void SetDamage(float damage) => this.damage = damage;

    protected virtual void MoveBullet()
    {
        if (!rb) return;

        Vector2 currentPosition = rb.position;
        Vector2 nextPosition = currentPosition + moveDir * (speed * Time.fixedDeltaTime);

        if (range > 0f)
        {
            float travelledSqr = (nextPosition - startPosition).sqrMagnitude;
            if (travelledSqr >= range * range)
            {
                DespawnWithImpact(currentPosition + moveDir * impactOffset, -moveDir);
                //Release();
                return;
            }
        }

        rb.MovePosition(nextPosition);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {   
        //Debug.Log("istrigger");
        Vector2 p = other.ClosestPoint(rb.position);
        var damageable = other.GetComponentInParent<IDamageable>(); // 여기
        if(damageable != null)
        {
            damageable.TakeDamage(damage);
            Debug.Log("damage!");
        }
        DespawnWithImpact(p, -moveDir);
        //Release();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ContactPoint2D cp = collision.GetContact(0);
        DespawnWithImpact(cp.point + cp.normal * impactOffset, cp.normal);
        //Release();
    }
    
    private void DespawnWithImpact(Vector2 pos, Vector2 normal)
    {
        if (released) return;
        released = true;
        SpawnImpact(pos, normal);
        Release();
    }

    private void SpawnImpact(Vector2 pos, Vector2 normal)
    {
        if (!impactPrefab) return;

        float angle = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
        
        ObjectPoolManager.Instance.Get(impactPrefab, pos, rot);
    }
    
    
    
    
}
