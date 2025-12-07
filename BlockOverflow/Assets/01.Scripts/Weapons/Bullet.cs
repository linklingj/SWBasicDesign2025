using UnityEngine;

public class Bullet : PoolObject {
    
    [Header("Impact (pooled)")]
    [SerializeField] private GameObject impactPrefab;    // 오브젝트 풀에 등록된 임팩트 프리팹
    [SerializeField] private float impactOffset = 0.02f; // 벽에 반쯤 박히지 않게 약간 앞쪽으로

    protected Rigidbody2D rb;
    protected float speed;
    protected float range;
    protected float damage;
    protected Vector2 moveDir;
    protected Vector2 startPosition;

    protected int playerIdx;
    
    private bool released;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }



    public void Init(Vector2 pos, Vector3 direction, BulletData data, int playerIndex = 1)
    {
        released = false;
        if (!rb) rb = GetComponent<Rigidbody2D>();
        playerIdx = playerIndex;

        speed = data.bulletSpeed;
        range = data.range;
        
        Vector2 planarDirection = new Vector2(direction.x, direction.y);
        moveDir = planarDirection.sqrMagnitude > 0f ? planarDirection.normalized : Vector2.zero;
        
        startPosition = pos;
        rb.position = startPosition;

        if (moveDir == Vector2.zero || speed <= 0f)
        {
            Release();
        }
    }
    

    private void FixedUpdate()
    {
        if (!released) MoveBullet();
    }

    public void SetDamage(float damage) => this.damage = damage;

    protected virtual void MoveBullet()
    {
        if (!rb) return;

        Vector2 currentPosition = rb.position;
        Vector2 nextPosition = currentPosition + moveDir * (speed * Time.fixedDeltaTime);

        if (range > 0f)
        {
            if ((currentPosition - startPosition).magnitude >= range)
            {
                //DespawnWithImpact(currentPosition + moveDir * impactOffset, -moveDir);
                Release();
                return;
            }
        }

        rb.MovePosition(nextPosition);
    }

    private void OnTriggerEnter2D(Collider2D other)
    { 
        if (other.transform.CompareTag("Bullet")) return;
        var damageable = other.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            if ((PlayerHealth)damageable)
            {
                if (other.gameObject.GetComponentInParent<PlayerController>().PlayerIndex == playerIdx);
                {
                    return;
                }
            }
            damageable.TakeDamage(damage, playerIdx);
            Vector2 p = other.ClosestPoint(rb.position);
            SpawnImpact(p, -moveDir);
            if (!isUltimate)
            {
                AudioPlayer.Instance.Play("Player_Hit");
            }
            else AudioPlayer.Instance.Play("Ulti_Hit");
        }

        if (!isUltimate)
        {
            Vector2 p = other.ClosestPoint(rb.position);
            DespawnWithImpact(p, -moveDir);
        }
        // 🔥 궁극탄은 계속 진행 (파괴 X)
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Bullet")) return;
        if (!isUltimate)
        {
            ContactPoint2D cp = collision.GetContact(0);
            DespawnWithImpact(cp.point + cp.normal * impactOffset, cp.normal);
        }


        
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

        Quaternion baseRot = Quaternion.Euler(0, 90f, 0f);
        Quaternion finalRot = rot * baseRot;
        ObjectPoolManager.Instance.Get(impactPrefab, pos, finalRot);
        
        
    }
    
    public bool isUltimate;

    public void SetUltimate(bool value)
    {
        isUltimate = value;
    }

    
}
