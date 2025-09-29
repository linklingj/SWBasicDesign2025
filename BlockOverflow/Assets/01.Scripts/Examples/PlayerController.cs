// csharp
using UnityEngine;
using UnityEngine.Events;
using PlayerInput;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")] //속도, rigidbody, Vector만들어두기
    [SerializeField] private float moveSpeed=6.0f; 

    private Vector2 moveInput;
    private Rigidbody2D rb;
    private Controls input;
    //외부에서 사용가능하도록
    public Controls.PlayerActions Player => input.Player;
    public Observable<int> health = new Observable<int>(100);
    public FSM<PlayerController> StateMachine { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = new Controls();
        StateMachine = new FSM<PlayerController>(this);
    }
    private void OnEnable()
    {
        // 플레이 액션맵 활성화 및 Move 입력 구독
        Player.Enable();
        Player.Move.performed += OnMove;
        Player.Move.canceled  += OnMove;
    }

    private void OnDisable()
    {
        Player.Move.performed -= OnMove;
        Player.Move.canceled  -= OnMove;
        Player.Disable();
    }
    void Start()
    {
        StateMachine.Set<IdleState>();
        StateMachine.Update();

    }
    private void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>(); //여기로 인풋값 들어옴
    }

    void Update()
    {
        // update FSM (will call current state's OnUpdate)
        StateMachine.Update();
    }
    
    public void ApplyMovement()
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }
    public Vector2 GetMoveInput() => moveInput;
    // 필요 시 상태에서 속도를 직접 0으로 하고 싶을 때
    public void StopImmediately() => rb.linearVelocity = Vector2.zero;

}