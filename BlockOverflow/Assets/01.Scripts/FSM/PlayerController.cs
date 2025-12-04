using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject movedust;
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6.0f;
    [SerializeField] private float airControlMultiplier = 0.8f;
    [SerializeField] private float crouchMoveMultiplier = 0.5f;
    private float totalMoveSpeed;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 15f;
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

    [Header("Ultimate / Cutscene")]
    //[SerializeField] private PersonaCutscene cutscene;
    [SerializeField] private UltimateWeapon ultimateWeapon;
    [SerializeField] private UltimateCutsceneDirector cutsceneDirector;
    
    [Header("Special Ability")]
    public Observable<bool> specialAbility;
    [SerializeField] private GameObject specialAvailibleEffect;
    
    [Header("Wall Check")]
    [SerializeField] private Transform wallCheckLeftUp;
    [SerializeField] private Transform wallCheckLeftDown;
    [SerializeField] private Transform wallCheckRightUp;
    [SerializeField] private Transform wallCheckRightDown;
    [SerializeField] private float wallCheckRadius = 0.2f;
    public void ConsumeJumpPress() => JumpThisFrame = false;
    
    private Rigidbody2D rb;
    private PlayerInput playerInput;

    private Vector2 moveInput;
    private bool isFacingRight = true;
    
    private Vector3 dustright = new Vector3(-.4f, -.5f, 0);
    private Vector3 dustleft = new Vector3(.4f, -.5f, 0);
    

    // 입력 플래그
    public bool jumpPressedThisFrame;
    public bool jumpReleasedThisFrame;
    public bool attackPressedThisFrame;
    public bool crouchHeld;

    private bool jumpPressedBuffered;   // 🔥 추가: 진짜 물리 입력 버퍼
    

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
    private float originalGravity;
    
    public float OriginalGravity { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        
        totalMoveSpeed = moveSpeed;

        StateMachine = new FSM<PlayerController>(this);

        // 플레이어마다 InputAction 독립
        if (playerInput.actions != null)
        {
            actionsCopy = Instantiate(playerInput.actions);
            playerInput.actions = actionsCopy;
        }
        
        specialAbility.AddListener(OnSpecialAbility);
        originalGravity = rb.gravityScale;
    }

    private void Start()
    {
        StateMachine.Set<IdleState>();
        
        //컷신은 일단 주석
        //if (cutscene == null)
        //    cutscene = FindObjectOfType<PersonaCutscene>();

        if (ultimateWeapon != null)
        {
            Transform weaponFirePoint = transform.Find("Weapon/FirePos");
            Weapon normalWeapon = GetComponentInChildren<Weapon>(); // ⭐ 현재 무기 가져오기

            if (weaponFirePoint != null && normalWeapon != null)
            {
                ultimateWeapon.Init(normalWeapon.Data, 0); // WeaponData 전달
                ultimateWeapon.SetFirePoint(weaponFirePoint); // FirePos 등록
            }
            else
            {
                Debug.LogWarning("[Ultimate] FirePos or Weapon missing");
            }
        }
        
        if (cutsceneDirector == null)
        {
            cutsceneDirector = FindObjectOfType<UltimateCutsceneDirector>();

            if (cutsceneDirector == null)
                Debug.LogError("❌ UltimateCutsceneDirector not found in scene!");
            else
                Debug.Log("✔ Director auto-assigned!");
        }
        specialAbility.Value = false;
    }

    public void SetUpgrades(float speedIncrease) {
        totalMoveSpeed = moveSpeed + speedIncrease;
    }


    private void Update()
    {
        if (IsTouchingWall(out _))
            Debug.Log("WALL!");
        if (!CanControl)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        // 🔥 한 프레임짜리 점프 입력 확정
        JumpThisFrame = jumpPressedBuffered;
        jumpPressedBuffered = false;
        
        StateMachine.Update();

        // ==== 방향 처리 ==== (Update 내 기존 코드 교체)
        if (moveInput.x > 0.01f)
        {
            isFacingRight = true;

            movedust.transform.localPosition = dustright;
            movedust.transform.rotation = Quaternion.Euler(180, 0, 0);
        }
        else if (moveInput.x < -0.01f)
        {
            isFacingRight = false;

            movedust.transform.localPosition = dustleft;
            movedust.transform.rotation = Quaternion.Euler(-180, 0, 0);
        }


        // 착지 처리
        if (IsGrounded())
        {
            movedust.SetActive(true);   
            lastGroundedTime = Time.time;
            ClearWallStickLockoutOnLand();
            
            jumpReleasedThisFrame = false;   // ⭐ 점프 릴리즈 버그 방지
            
            if (rb.linearVelocity.y <= 0f)
                hasStartedJump = false;
        }

        bool isTouchingWallNow = IsTouchingWall(out _);
        if (isTouchingWallNow && !wasTouchingWall)
        {
            // 벽에 새로 닿는 순간!
            wasTouchingWall = true;
            airJumpsAvailable = maxAirJumps; 
            wallStickLockout = false; 
        }
        else if (!isTouchingWallNow)
        {
            wasTouchingWall = false;
        }
        if (!IsTouchingWall(out _) && wasTouchingWall)
        {
            airJumpsAvailable = maxAirJumps; // 🔥 한 번 더 보장
        }
        
    }
    

    // ==== INPUT CALLBACKS ====
    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (!CanControl) { moveInput = Vector2.zero; return; }
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        movedust.SetActive(false);
        if (ctx.started)
        {
            lastJumpPressedTime = Time.time;

            jumpPressedBuffered = true;      // 🔥 한 프레임짜리 입력 버퍼
            jumpPressedThisFrame = true;     // 이건 너가 쓰고 있으면 유지

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
    
    
    //컷신당시 OnUlti
    // public void OnUltimate(InputAction.CallbackContext ctx)
    // {
    //     if (!ctx.started || !CanControl) return;
    //
    //     Debug.Log("ULTIMATE TRIGGERED");
    //
    //     if (cutscene == null)
    //         cutscene = FindObjectOfType<PersonaCutscene>();
    //
    //     if (cutscene == null)
    //     {
    //         Debug.LogWarning("PersonaCutscene is not found! Make sure it is active.");
    //         return;
    //     }
    //
    //     SetControl(false);
    //
    //     cutscene.Play(() =>
    //     {
    //         if (ultimateWeapon != null)
    //         {
    //             ultimateWeapon.Fire(); // 🚀 궁극기 탄 발사!
    //         }
    //         else
    //         {
    //             Debug.LogWarning("UltimateWeaponController not assigned!");
    //         }
    //
    //         SetControl(true);
    //     });
    // }
    public void OnUltimate(InputAction.CallbackContext ctx)
    {
        if (!ctx.started || !CanControl) return;
        Debug.Log("ULTIMATE START");

        SetControl(false);

        Transform firePos = transform.Find("Weapon/FirePos");
        if (!firePos) firePos = transform;

        cutsceneDirector.Play(
            this.transform,
            firePos,
            () =>
            {
                ultimateWeapon?.Fire();
                SetControl(true);
            }
        );
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
            wallStickLockout = false;
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
        float baseSpeed = totalMoveSpeed;

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
        wallNormal = Vector2.zero;

        bool leftUp   = Physics2D.OverlapCircle(wallCheckLeftUp.position,   wallCheckRadius, groundMask);
        bool leftDown = Physics2D.OverlapCircle(wallCheckLeftDown.position, wallCheckRadius, groundMask);
        bool rightUp  = Physics2D.OverlapCircle(wallCheckRightUp.position,  wallCheckRadius, groundMask);
        bool rightDown= Physics2D.OverlapCircle(wallCheckRightDown.position,wallCheckRadius, groundMask);

        if (leftUp || leftDown)
        {
            wallNormal = Vector2.right; // 왼쪽 벽 → 오른쪽으로 튕김
            return true;
        }

        if (rightUp || rightDown)
        {
            wallNormal = Vector2.left; // 오른쪽 벽 → 왼쪽으로 튕김
            return true;
        }

        return false;
    }




    // ==== WALL STICK ====
    public void BeginWallStick()
    {
        wallStickTimer = wallStickMaxTime;
        rb.gravityScale = 0f; // 🔥 중력 제거 → 벽 달라붙기
        rb.linearVelocity = new Vector2(0f, 0f);
    }

    
    public void OnSpecialAbility(bool enabled)
    {
        specialAvailibleEffect.SetActive(enabled);
    }

    public void TickWallStick() => wallStickTimer -= Time.deltaTime;
    public bool IsWallStickExpired() => wallStickTimer <= 0f;
    public void BreakWallStickUntilLand() => wallStickLockout = true;
    public bool CanWallStickAgain() => !wallStickLockout;
    public void ClearWallStickLockoutOnLand() => wallStickLockout = false;

    public void DoWallJump(Vector2 wallNormal)
    {
        hasStartedJump = true;
        jumpStartFrame = Time.frameCount;

        rb.linearVelocity = Vector2.zero;

        // 🔥 벽에서 반대 방향 + 위로 튕기기
        Vector2 force = new Vector2(
            wallNormal.x * wallJumpForce.x * 1.3f, // ← 여기서 wallNormal.x 그대로!
            wallJumpForce.y * 1.1f
        );

        rb.AddForce(force, ForceMode2D.Impulse);

        // 원하면 이거 유지
        // StartCoroutine(LockHorizontalControl());

        // 벽 점프는 항상 "1단 점프" 취급 → 공중 점프 리필
        airJumpsAvailable = maxAirJumps;
        wallStickLockout = false;
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
    
    private IEnumerator LockHorizontalControl()
    {
        CanControl = false;
        yield return new WaitForSeconds(0.08f); // 손맛 튜닝 가능
        CanControl = true;
    }
    

}