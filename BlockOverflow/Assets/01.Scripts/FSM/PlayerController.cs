using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInput;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6.0f;
    [SerializeField] private float airControlMultiplier = 0.7f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBuffer = 0.1f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private int maxAirJumps = 1;

    [Header("Ground / Wall Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.25f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float wallCheckDistance = 0.35f;

    [Header("Crouch")]
    [SerializeField] private Collider2D standCollider;
    [SerializeField] private Collider2D crouchCollider;
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
    private bool wasGroundedLastFrame;
    private float lastGroundedTime;
    private float lastJumpPressedTime;
    private int airJumpsAvailable;

    // 벽 매달림 보조
    private bool wallStickLockout;
    private float wallStickTimer;

    private float nextFireTime;

    public PlayerControls.PlayerActions Player => input.Player;
    public FSM<PlayerController> StateMachine { get; private set; }
    public Rigidbody2D Rb => rb;
    public Vector2 GetMoveInput() => moveInput;
    public bool IsCrouching { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = new PlayerControls();
        StateMachine = new FSM<PlayerController>(this);
    }

    private void Start()
    {
        StateMachine.Set<IdleState>();
        StateMachine.Update();
    }

    private void OnEnable()
    {
        Player.Enable();
        Player.Move.performed += OnMove;
        Player.Move.canceled += OnMove;
        Player.Jump.started += OnJumpPressed;
        Player.Attack.started += OnAttackPressed;
    }

    private void OnDisable()
    {
        Player.Move.performed -= OnMove;
        Player.Move.canceled -= OnMove;
        Player.Jump.started -= OnJumpPressed;
        Player.Attack.started -= OnAttackPressed;
        Player.Disable();
    }

    private void Update()
    {
        // 방향 전환
        if (moveInput.x > 0.01f) isFacingRight = true;
        else if (moveInput.x < -0.01f) isFacingRight = false;

        // FSM 업데이트
        StateMachine.Update();

        // GroundCheck 판정
        bool groundedNow = IsGrounded();

        // 착지 프레임에만 1회 리필
        if (groundedNow && !wasGroundedLastFrame)
        {
            EnterAir(); // 공중 점프 카운트 리필
            ClearWallStickLockoutOnLand();
        }

        if (groundedNow)
            lastGroundedTime = Time.time;

        wasGroundedLastFrame = groundedNow;
    }

    // ===== 입력 콜백 =====
    private void OnMove(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    private void OnJumpPressed(InputAction.CallbackContext _) => lastJumpPressedTime = Time.time;
    private void OnAttackPressed(InputAction.CallbackContext _) { }

    // ===== 이동 / 점프 =====
    public void ApplyMovement()
    {
        float baseSpeed = moveSpeed * (IsCrouching ? crouchMoveMultiplier : 1f);
        float targetSpeed = moveInput.x * baseSpeed;

        if (IsGrounded())
        {
            rb.linearVelocity = new Vector2(targetSpeed, rb.linearVelocity.y);
        }
        else
        {
            // 공중에서도 빠르게 방향전환
            float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, (moveSpeed * 6f) * Time.deltaTime);
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
        }
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
        return (Time.time - lastGroundedTime) <= coyoteTime
            && (Time.time - lastJumpPressedTime) <= jumpBuffer;
    }

    public void EnterAir() => airJumpsAvailable = maxAirJumps;

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

    // ===== 벽 판정 =====
    public bool IsTouchingWall(out Vector2 wallNormal)
    {
        wallNormal = Vector2.zero;
        Vector2 dir = new Vector2(Mathf.Sign(moveInput.x), 0f);
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
        rb.linearVelocity = new Vector2(0f, Mathf.Min(rb.linearVelocity.y, -1f));
    }

    public void TickWallStick() => wallStickTimer -= Time.deltaTime;
    public bool IsWallStickExpired() => wallStickTimer <= 0f;
    public void BreakWallStickUntilLand() => wallStickLockout = true;
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
        BreakWallStickUntilLand();
    }

    // ===== 숙이기 =====
    public void StartCrouch()
    {
        IsCrouching = true;
        if (standCollider) standCollider.enabled = false;
        if (crouchCollider) crouchCollider.enabled = true;
    }

    public void EndCrouch()
    {
        IsCrouching = false;
        if (crouchCollider) crouchCollider.enabled = false;
        if (standCollider) standCollider.enabled = true;
    }

    // ===== 사격 =====
    public void Fire()
    {
        if (Time.time < nextFireTime) return;
        if (!bulletPrefab || !firePoint) return;

        var b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Vector2 dir = (isFacingRight ? Vector2.right : Vector2.left);
        b.linearVelocity = dir * bulletSpeed;
        nextFireTime = Time.time + fireCooldown;
    }
}
