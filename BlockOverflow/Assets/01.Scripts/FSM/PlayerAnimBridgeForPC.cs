using System;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimBridgeForPC : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerController controller; // 루트 PlayerController 드래그
    [SerializeField] private Animator anim;               // Body의 Animator (자동 할당)
    
    [Header("Tuning")]
    //[SerializeField] private float walkSpeedThreshold = 0.05f; // Idle↔Walk 기준
    [SerializeField] private float turnMinSpeed = 0.1f;        // Turn 트리거 최소 속도
    [SerializeField] private float doubleJumpVySpike = 5.0f;   // 더블점프 감지용 vy 증가량

    // 리플렉션 캐시 (StateMachine.Current 같은 멤버 찾기용)
    FieldInfo  smField;
    PropertyInfo smProp;
    FieldInfo  currentField;
    PropertyInfo currentProp;

    // 이전 프레임 값들 (변화 감지)
    string lastStateName = null;
    bool   lastGrounded  = true;
    float  lastMoveSign  = 0f;
    float  lastVy        = 0f;

    void Reset()
    {
        anim = GetComponent<Animator>();
        controller = GetComponentInParent<PlayerController>();
    }

    void Awake()
    {
        if (!anim) anim = GetComponent<Animator>();
        if (!controller) controller = GetComponentInParent<PlayerController>();

        CacheFSMMembers();          // StateMachine과 Current 같은 멤버 찾아두기
        BootstrapInitialState();    // 시작 상태 한번 반영
    }

    void Update()
    {
        if (controller == null || anim == null) return;
        
        
        

        // 1) 기본 파라미터 매 프레임 반영
        bool grounded = controller.IsGrounded();
        anim.SetBool("IsGrounded", grounded);

        anim.SetBool("IsCrouch", controller.IsCrouching);

        float vx = controller.Rb ? controller.Rb.linearVelocity.x : 0f;
        float vy = controller.Rb ? controller.Rb.linearVelocity.y : 0f;
        anim.SetFloat("Speed", Mathf.Abs(vx));

        Vector2 move = controller.GetMoveInput();
        float moveSign = Mathf.Abs(move.x) > 0.01f ? Mathf.Sign(move.x) : 0f;
        anim.SetFloat("MoveX", moveSign);

        // 2) 방향 급전환 감지 → Turn
        if (Mathf.Abs(vx) > turnMinSpeed && lastMoveSign != 0f && moveSign != 0f && moveSign != lastMoveSign)
            anim.SetTrigger("Turn");

        // 3) 상태 전환 감지(리플렉션) → JumpTrigger 등
        string stateName = GetCurrentStateTypeName();
        if (!string.IsNullOrEmpty(stateName) && stateName != lastStateName)
        {
            HandleStateEnter(stateName, lastStateName);
            lastStateName = stateName;
        }

        // 4) 더블점프 휴리스틱(공중 상태에서 vy가 급상승하면 트리거)
        if (!grounded && !lastGrounded)
        {
            float deltaVy = vy - lastVy;
            if (deltaVy > doubleJumpVySpike)
                anim.SetTrigger("DoubleJumpTrigger");
        }

        // 5) 캐시 갱신
        lastGrounded = grounded;
        lastMoveSign = moveSign;
        lastVy = vy;
        
    }

    // -------- 상태 진입 시 애니 파라미터 세팅(리플렉션 기반) --------
    void HandleStateEnter(string current, string prev)
    {
        switch (current)
        {
            case "IdleState":
                anim.SetBool("IsCrouch", false);
                anim.SetFloat("Speed", 0f);
                break;

            case "MoveState":
                anim.SetBool("IsCrouch", false);
                break;

            case "CrouchState":
                anim.SetBool("IsCrouch", true);
                anim.SetFloat("Speed", 0f);
                break;

            case "AirState":
                // 지상→공중으로 넘어온 프레임에 점프 트리거
                if (prev != "AirState")
                {
                    anim.SetBool("IsCrouch", false);
                    anim.SetTrigger("JumpTrigger");
                }
                break;

            case "WallStickState":
                anim.SetBool("IsCrouch", false);
                anim.SetFloat("Speed", 0f);
                break;

            default:
                // 다른 상태가 추가되면 여기서 매핑만 보강하면 됨
                break;
        }
    }

    // -------- 초기 반영 --------
    void BootstrapInitialState()
    {
        anim.SetBool("IsGrounded", controller.IsGrounded());
        anim.SetBool("IsCrouch", controller.IsCrouching);
        anim.SetFloat("Speed", Mathf.Abs(controller.Rb ? controller.Rb.linearVelocity.x : 0f));
        anim.SetFloat("MoveX", Mathf.Sign(controller.GetMoveInput().x));

        lastGrounded = controller.IsGrounded();
        lastVy = controller.Rb ? controller.Rb.linearVelocity.y : 0f;

        lastStateName = GetCurrentStateTypeName();
        if (!string.IsNullOrEmpty(lastStateName))
            HandleStateEnter(lastStateName, null);
    }

    // -------- FSM 리플렉션: StateMachine.Current의 타입 이름 얻기 --------
    string GetCurrentStateTypeName()
    {
        if (controller == null) return null;

        // StateMachine 객체 얻기
        object sm = null;
        if (smProp != null) sm = smProp.GetValue(controller);
        else if (smField != null) sm = smField.GetValue(controller);
        if (sm == null) return null;

        // Current 같은 멤버에서 현재 상태 얻기
        object current = null;
        if (currentProp != null) current = currentProp.GetValue(sm);
        else if (currentField != null) current = currentField.GetValue(sm);

        return current != null ? current.GetType().Name : null;
    }

    void CacheFSMMembers()
    {
        var t = controller.GetType();

        // 1) PlayerController.StateMachine (Property/Field) 찾기
        smProp  = t.GetProperty("StateMachine", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        smField = t.GetField   ("StateMachine", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        var smObj = smProp != null ? smProp.GetValue(controller) : smField?.GetValue(controller);
        if (smObj == null) return;

        var smType = smObj.GetType();

        // 2) FSM 내부의 현재 상태 멤버 후보: Current / current / _current 등
        currentProp  = smType.GetProperty("Current", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?? smType.GetProperty("current", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        currentField = smType.GetField   ("Current", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?? smType.GetField   ("current", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?? smType.GetField   ("_current", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }
}
