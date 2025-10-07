using UnityEngine;

public class Bullet : PoolObject {
    [SerializeField] protected BulletData bulletData;

    protected Rigidbody2D rb;
    protected float speed;
    protected float range;
    protected Vector2 moveDir;
    protected Vector2 startPosition;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector3 direction)
    {
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
                Release();
                return;
            }
        }

        rb.MovePosition(nextPosition);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Release();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Release();
    }
}
