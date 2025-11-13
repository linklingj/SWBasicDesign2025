using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6.0f;
    [SerializeField] private float airControlMultiplier = 0.8f;
    [SerializeField] private float crouchMoveMultiplier = 0.5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBuffer = 0.1f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    [SerializeField] private int maxAirJumps = 1;

    [Header("Ground / Wall Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.15f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float wallCheckDistance = 0.35f;

    [Header("Crouch")]
    [SerializeField] private Collider2D standCollider;
    [SerializeField] private Collider2D crouchCollider;

    [Header("Wall Jump")]
    [SerializeField] private Vector2 wallJumpForce = new Vector2(10f, 12f);
    [SerializeField] private float wallStickMaxTime = 3f;

    [Header("Shoot / Aim")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Rigidbody2D bulletPrefab;
    [SerializeField] private float bulletSpeed = 16f;
    [SerializeField] private float fireCooldown = 0.15f;

    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private Vector2 moveInput;
    private Vector2 aimInput;
    private bool isFacingRight = true;
    private float nextFireTime;

    // 점프/공격 플래그
    [HideInInspector] public bool jumpPressedThisFrame;
    [HideInInspector] public bool jumpReleasedThisFrame;
    [HideInInspector] public bool attackPressedThisFrame;
    [HideInInspector] public bool crouchHeld;

    private float lastGroundedTime;
    private float lastJumpPressedTime;
    private int airJumpsAvailable;

    private bool wallStickLockout;
    private float wallStickTimer;

    public FSM<PlayerController> StateMachine { get; private set; }
    public Rigidbody2D Rb => rb;
    public Vector2 GetMoveInput() => moveInput;
    public Vector2 GetAimInput() => aimInput;
    public bool IsCrouching { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        StateMachine = new FSM<PlayerController>(this);
    }

    private void Start()
    {
        StateMachine.Set<IdleState>();
    }

    private void Update()
    {
        StateMachine.Update();

        if (moveInput.x > 0.01f) isFacingRight = true;
        else if (moveInput.x < -0.01f) isFacingRight = false;

        if (IsGrounded())
        {
            lastGroundedTime = Time.time;
            ClearWallStickLockoutOnLand();
        }
    }

    // Consume helpers
    public bool ConsumeJumpPressed()  { var v = jumpPressedThisFrame;  jumpPressedThisFrame  = false; return v; }
    public bool ConsumeJumpReleased() { var v = jumpReleasedThisFrame; jumpReleasedThisFrame = false; return v; }
    public bool ConsumeAttackPressed(){ var v = attackPressedThisFrame; attackPressedThisFrame = false; return v; }

    // ================= Movement =================
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
            float lerpFactor = 0.15f + 0.85f * airControlMultiplier;
            float smoothedX = Mathf.Lerp(rb.linearVelocity.x, targetSpeed, lerpFactor);
            rb.linearVelocity = new Vector2(smoothedX, rb.linearVelocity.y);
        }
    }

    public void StopImmediately() => rb.linearVelocity = Vector2.zero;

    public bool IsGrounded()
    {
        if (!groundCheck) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundMask);
    }

    public bool IsPressingDown() => moveInput.y < -0.5f;

    // ================= Jump =================
    public void EnterAir() => airJumpsAvailable = maxAirJumps;

    public bool TryGroundOrBufferedJump()
    {
        if ((Time.time - lastGroundedTime) <= coyoteTime &&
            (Time.time - lastJumpPressedTime) <= jumpBuffer)
        {
            DoJump();
            EnterAir();
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
        lastJumpPressedTime = -999f;
    }

    public void CutJumpEarly()
    {
        if (rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
    }

    // ================= Wall Logic =================
    public bool IsTouchingWall(out Vector2 wallNormal)
    {
        wallNormal = Vector2.zero;
        Vector2 dir = new Vector2(isFacingRight ? 1f : -1f, 0f);
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
    public void ClearWallStickLockoutOnLand() => wallStickLockout = false;

    public void DoWallJump(Vector2 wallNormal)
    {
        Vector2 pushDir = (Vector2.up + (-wallNormal)).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(pushDir.x * wallJumpForce.x, wallJumpForce.y), ForceMode2D.Impulse);
        BreakWallStickUntilLand();
    }

    // ================= Crouch =================
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

    // ================= Shooting =================
    public void Fire()
    {
        if (Time.time < nextFireTime || !bulletPrefab || !firePoint) return;

        Vector2 dir = aimInput.sqrMagnitude > 0.1f
            ? aimInput.normalized
            : (isFacingRight ? Vector2.right : Vector2.left);

        var b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        b.linearVelocity = dir * bulletSpeed;
        nextFireTime = Time.time + fireCooldown;
    }

    // ================= Input Callbacks =================
    public void OnMove(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    public void OnAim(InputAction.CallbackContext ctx) => aimInput = ctx.ReadValue<Vector2>();
    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            lastJumpPressedTime = Time.time;
            jumpPressedThisFrame = true;
        }
        else if (ctx.canceled)
        {
            jumpReleasedThisFrame = true;
            CutJumpEarly();
        }
    }
    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            attackPressedThisFrame = true;
    }
    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            crouchHeld = true;
            StartCrouch();
        }
        else if (ctx.canceled)
        {
            crouchHeld = false;
            EndCrouch();
        }
    }
}
