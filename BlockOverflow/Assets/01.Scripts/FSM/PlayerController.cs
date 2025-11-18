using System;
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
    [SerializeField] public int maxAirJumps = 1;

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
    [SerializeField] private float wallStickMaxTime = 0.3f;

    private Rigidbody2D rb;
    private PlayerInput playerInput;

    private Vector2 moveInput;
    private bool isFacingRight = true;

    // 입력 플래그
    public bool jumpPressedThisFrame;
    public bool jumpReleasedThisFrame;
    public bool attackPressedThisFrame;
    public bool crouchHeld;
    

    // 점프 관련
    private float lastGroundedTime;
    private float lastJumpPressedTime;
    public bool hasStartedJump = false;
    private float jumpStartTime;     // (옵션) 시간 단위로도 쓸 수 있음
    public int jumpStartFrame;      // 🔹 점프 시작한 프레임

    public int airJumpsAvailable;
    public bool wasTouchingWall = false;

    private bool wallStickLockout;
    private float wallStickTimer;

    // FSM
    public FSM<PlayerController> StateMachine { get; private set; }
    public bool JumpThisFrame { get; private set; }
    public bool JumpHeld { get; private set; }

    private InputActionAsset actionsCopy;

    public Rigidbody2D Rb => rb;
    public Vector2 MoveInput => moveInput;
    public bool IsCrouching { get; private set; }
    public bool CanControl { get; private set; } = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        StateMachine = new FSM<PlayerController>(this);

        // 플레이어마다 InputAction 독립
        if (playerInput.actions != null)
        {
            actionsCopy = Instantiate(playerInput.actions);
            playerInput.actions = actionsCopy;
        }
    }

    private void Start()
    {
        StateMachine.Set<IdleState>();
    }

    private void Update()
    {
        if (!CanControl)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        StateMachine.Update();

        if (moveInput.x > 0.01f) isFacingRight = true;
        else if (moveInput.x < -0.01f) isFacingRight = false;

        // 착지 처리
        if (IsGrounded())
        {
            lastGroundedTime = Time.time;
            ClearWallStickLockoutOnLand();

            // ✅ 땅에 있고 위로 안 날아갈 때는 "점프 중 아님"
            if (rb.linearVelocity.y <= 0f)
                hasStartedJump = false;
        }

        // 벽 닿았을 때 공중 점프 회복
        if (IsTouchingWall(out _) && !wasTouchingWall)
        {
            wasTouchingWall = true;
            airJumpsAvailable = maxAirJumps;
        }
        else if (!IsTouchingWall(out _))
        {
            wasTouchingWall = false;
        }

        // 🔥 이 프레임에서 JumpThisFrame 사용 끝
        JumpThisFrame = false;
    }
    

    // ==== INPUT CALLBACKS ====
    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (!CanControl) { moveInput = Vector2.zero; return; }
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            lastJumpPressedTime = Time.time;

            JumpThisFrame = true;
            jumpPressedThisFrame = true;

            JumpHeld = true;
        }
        else if (ctx.canceled)
        {

            jumpReleasedThisFrame = true;
            JumpHeld = false;
        }
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

    public void OnAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            attackPressedThisFrame = true;
    }

    // === CONSUME HELPERS ===
    public bool ConsumeJumpReleased()
    {
        var v = jumpReleasedThisFrame;
        jumpReleasedThisFrame = false;
        return v;
    }

    public bool ConsumeAttackPressed()
    {
        var v = attackPressedThisFrame;
        attackPressedThisFrame = false;
        return v;
    }

    // ===== JUMP LOGIC =====
    public void EnterAir() => airJumpsAvailable = maxAirJumps;

    // 🔧 코요테 + 버퍼 기반 지상점프
    public bool TryGroundOrBufferedJump()
    {
        // "언제 마지막으로 땅에 있었는지" + "언제 점프를 눌렀는지"
        bool withinCoyote = (Time.time - lastGroundedTime) <= coyoteTime;
        bool withinBuffer = (Time.time - lastJumpPressedTime) <= jumpBuffer;

        if (withinCoyote && withinBuffer)
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
        hasStartedJump = true;
        jumpStartFrame = Time.frameCount;
        jumpStartTime = Time.time;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        lastJumpPressedTime = -999f;
    }

    public void CutJumpEarly()
    {
        // 아직 진짜 점프 시작 전이면 무시
        if (!hasStartedJump) return;

        // ✅ 점프한 바로 그 프레임에는 절대 컷하지 않기
        if (Time.frameCount == jumpStartFrame)
            return;

        // (선택) 점프 후 최소 0.05초는 유지해도 됨
        // if (Time.time - jumpStartTime < 0.05f)
        //     return;

        if (Rb.linearVelocity.y > 0)
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * jumpCutMultiplier
            );
    }
    // ===== CROUCH =====
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

    // ===== MOVEMENT =====
    public void ApplyMovement()
    {
        float baseSpeed = moveSpeed;

        if (IsCrouching)
            baseSpeed *= crouchMoveMultiplier;

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

    // ==== WALL CHECK ====
    public bool IsTouchingWall(out Vector2 wallNormal)
    {
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, groundMask);
        RaycastHit2D hitLeft  = Physics2D.Raycast(transform.position, Vector2.left,  wallCheckDistance, groundMask);

        if (hitRight.collider != null)
        {
            wallNormal = hitRight.normal;
            return true;
        }
        if (hitLeft.collider != null)
        {
            wallNormal = hitLeft.normal;
            return true;
        }

        wallNormal = Vector2.zero;
        return false;
    }

    // ==== WALL STICK ====
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
        rb.AddForce(new Vector2(pushDir.x * wallJumpForce.x, wallJumpForce.y),
            ForceMode2D.Impulse);

        // 벽점프 후에도 공중점프 남겨두기
        airJumpsAvailable = maxAirJumps;
    }

    // ==== GROUNDED ====
    public bool IsGrounded()
    {
        if (!groundCheck) return false;
        return Physics2D.OverlapCircle(
            groundCheck.position,
            groundRadius,
            groundMask
        );
    }

    // ==== CONTROL ENABLE ====
    public void SetControl(bool value)
    {
        CanControl = value;
        if (!value)
            rb.linearVelocity = Vector2.zero;
    }
}