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
    protected Vector2 lastPosition;

    protected int playerIdx;
    
    private bool released;
    
    protected bool reflectOnWalls = false;

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
        lastPosition = startPosition;
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
    public void SetReflectOnWalls(bool value) => reflectOnWalls = value;

    protected virtual void MoveBullet()
    {
        if (!rb) return;

        Vector2 currentPosition = rb.position;
        lastPosition = currentPosition;
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
        // 1) 우선 데미지 처리 (적이나 플레이어 등)
        var damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            Vector2 hitPoint = other.ClosestPoint(rb.position);
            SpawnImpact(hitPoint, -moveDir);

            if (!isUltimate)
            {
                DespawnWithImpact(hitPoint, -moveDir);
            }
            // 데미지를 준 경우에는 반사 처리하지 않고 종료
            return;
        }

        // 2) 환경(벽 등)에 대한 반사 처리: 트리거 콜라이더여도 반사 가능
        if (reflectOnWalls && !isUltimate)
        {
            // lastPosition -> 현재 위치로 레이캐스트하여 실제 히트 노말을 구한다.
            Vector2 from = lastPosition;
            Vector2 to = rb.position;
            Vector2 dir = to - from;
            float dist = dir.magnitude;

            if (dist > 0.0001f)
            {
                dir /= dist;
                RaycastHit2D hit = Physics2D.Raycast(from, dir, dist);
                if (hit.collider != null)
                {
                    Vector2 normal = hit.normal;

                    // 이동 방향 반사
                    moveDir = Vector2.Reflect(moveDir, normal).normalized;

                    // 벽에 박히지 않도록 히트 지점에서 살짝 띄워놓기
                    Vector2 newPos = hit.point + normal * impactOffset;
                    rb.position = newPos;

                    SpawnImpact(newPos, normal);
                    return; // 반사 후 계속 진행 (디스폰하지 않음)
                }
            }

            // 레이캐스트 실패 시 대략적인 노멀로 반사 (센터 기준)
            Vector2 approxNormal = ((Vector2)rb.position - (Vector2)other.bounds.center).normalized;
            moveDir = Vector2.Reflect(moveDir, approxNormal).normalized;
            Vector2 approxPos = rb.position + approxNormal * impactOffset;
            rb.position = approxPos;
            SpawnImpact(approxPos, approxNormal);
            return;
        }

        // 3) 반사 모드가 아니고 궁극탄도 아니라면 기존처럼 디스폰
        if (!isUltimate)
        {
            Vector2 p = other.ClosestPoint(rb.position);
            DespawnWithImpact(p, -moveDir);
        }
        // 🔥 궁극탄은 계속 진행 (파괴 X)
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
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
