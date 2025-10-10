using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInput;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6.0f;
    
    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float coyoteTime = 0.1f;     // 지상 이탈 후 관용
    [SerializeField] private float jumpBuffer = 0.1f;     // 점프 입력 버퍼
    [SerializeField] private float jumpCutMultiplier = 0.5f; // 점프키 떼면 상승속도 감쇠
    [SerializeField] private int   maxAirJumps = 1;       // 공중 점프 1회
    
    [Header("Ground / Wall Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.15f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float wallCheckDistance = 0.35f;
    
    [Header("Crouch")]
    [SerializeField] private Collider2D standCollider;    // 서있을 때 콜라이더
    [SerializeField] private Collider2D crouchCollider;   // 숙였을 때 콜라이더
    [SerializeField] private float crouchMoveMultiplier = 0.5f;
    
    [Header("Wall Stick / Jump")]
    [SerializeField] private float wallStickMaxTime = 3f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(10f, 12f);

    [Header("Shoot")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Rigidbody2D bulletPrefab;
    [SerializeField] private float bulletSpeed = 16f;
    [SerializeField] private float fireCooldown = 0.15f;

    private Rigidbody2D rb;
    private PlayerControls input;
    private Vector2 moveInput;
    private bool isFacingRight = true;
    
    // 점프 보조
    private float lastGroundedTime;
    private float lastJumpPressedTime;
    private int   airJumpsAvailable;
    
    // 벽 매달림 보조
    private bool  wallStickLockout;   // 3초 만료 후 지상 닿기 전까지 재부착 금지
    private float wallStickTimer;

    private float nextFireTime;

    public PlayerControls.PlayerActions Player => input.Player; // ← 타입도 PlayerControls로
    public FSM<PlayerController> StateMachine { get; private set; }
    public Rigidbody2D Rb => rb;  // 상태에서 필요할 때 읽기용

    // 외부/상태에서 필요한 정보 제공
    public Vector2 GetMoveInput() => moveInput;
    public bool IsCrouching { get; private set; }
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = new PlayerControls();
        StateMachine = new FSM<PlayerController>(this);
    }

    void Start()
    {
        StateMachine.Set<IdleState>();
        StateMachine.Update();
    }

    private void OnEnable()
    {
        Player.Enable();
        Player.Move.performed += OnMove;
        Player.Move.canceled  += OnMove;

        Player.Jump.started   += OnJumpPressed;
        Player.Attack.started += OnAttackPressed;
    }
    private void OnDisable()
    {
        Player.Move.performed -= OnMove;
        Player.Move.canceled  -= OnMove;

        Player.Jump.started   -= OnJumpPressed;
        Player.Attack.started -= OnAttackPressed;

        Player.Disable();
    }
    
    private void Update()
    {
        // 바라보는 방향 업데이트(간단: 이동 입력 기준)
        if (moveInput.x > 0.01f) isFacingRight = true;
        else if (moveInput.x < -0.01f) isFacingRight = false;

        // FSM 틱
        StateMachine.Update();

        // 지상 시간 캐시
        if (IsGrounded()) lastGroundedTime = Time.time;
    }
    
    // --- 입력 콜백 ---
    private void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }
    private void OnJumpPressed(InputAction.CallbackContext _)
    {
        lastJumpPressedTime = Time.time;
    }
    private void OnAttackPressed(InputAction.CallbackContext _)
    {
        // 즉시 발사는 상태에서 일원화하자(WasPressedThisFrame으로 처리)
        // 필요하면 여기서도 바로 Fire() 호출 가능
    }
    
    // --- 유틸 / 행위 ---
    public void ApplyMovement()
    {
        float speed = moveSpeed * (IsCrouching ? crouchMoveMultiplier : 1f);
        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
    }
    public void StopImmediately() => rb.linearVelocity = Vector2.zero;
    
    public bool IsGrounded()
    {
        if (!groundCheck) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);
    }

    public bool IsPressingDown() => moveInput.y < -0.5f;

    public bool CanConsumeJumpBuffered()
    {
        // 지상 관용 + 입력 버퍼
        return (Time.time - lastGroundedTime) <= coyoteTime
            && (Time.time - lastJumpPressedTime) <= jumpBuffer;
    }

    public void EnterAir()
    {
        airJumpsAvailable = maxAirJumps;
    }

    public bool TryGroundOrBufferedJump()
    {
        if (CanConsumeJumpBuffered())
        {
            DoJump();
            return true;
        }
        return false;
    }

    public bool TryAirJump()
    {
        if (airJumpsAvailable > 0)
        {
            airJumpsAvailable--;
            DoJump();
            return true;
        }
        return false;
    }

    public void DoJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        lastJumpPressedTime = -999f; // 버퍼 소모
    }

    public void CutJumpEarly()
    {
        if (rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
    }

    public bool IsTouchingWall(out Vector2 wallNormal)
    {
        wallNormal = Vector2.zero;
        var dir = new Vector2(Mathf.Sign(moveInput.x), 0f);
        if (dir == Vector2.zero) dir = isFacingRight ? Vector2.right : Vector2.left;

        var hit = Physics2D.Raycast(transform.position, dir, wallCheckDistance, groundMask);
        if (hit.collider != null)
        {
            wallNormal = hit.normal;
            return true;
        }
        return false;
    }

    public void BeginWallStick()
    {
        wallStickTimer = wallStickMaxTime;
        rb.linearVelocity = new Vector2(0f, Mathf.Min(rb.linearVelocity.y, -1f)); // 천천히 미끄러지거나 정지
    }

    public void TickWallStick()
    {
        wallStickTimer -= Time.deltaTime;
    }

    public bool IsWallStickExpired() => wallStickTimer <= 0f;

    public void BreakWallStickUntilLand()
    {
        wallStickLockout = true;
    }

    public bool CanWallStickAgain() => !wallStickLockout;

    public void ClearWallStickLockoutOnLand()
    {
        if (IsGrounded()) wallStickLockout = false;
    }

    public void DoWallJump(Vector2 wallNormal)
    {
        Vector2 push = (Vector2.up + (-wallNormal)).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(push.x * wallJumpForce.x, wallJumpForce.y), ForceMode2D.Impulse);
        BreakWallStickUntilLand(); // 착지 전 재부착 금지
    }

    public void StartCrouch()
    {
        IsCrouching = true;
        if (standCollider) standCollider.enabled = false;
        if (crouchCollider) crouchCollider.enabled = true;
    }

    public void EndCrouch()
    {
        // 머리 위 여유 체크 필요하면 추가(Physics2D.OverlapBox 등)
        IsCrouching = false;
        if (crouchCollider) crouchCollider.enabled = false;
        if (standCollider) standCollider.enabled = true;
    }

    public void Fire()
    {
        if (Time.time < nextFireTime) return;
        if (!bulletPrefab || !firePoint) return;

        var b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        var dir = (isFacingRight ? Vector2.right : Vector2.left);
        b.linearVelocity = dir * bulletSpeed;

        nextFireTime = Time.time + fireCooldown;
    }
}