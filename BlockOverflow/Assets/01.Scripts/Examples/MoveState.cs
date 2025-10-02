using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInput;
public class MoveState : State<PlayerController>
{
    private InputAction move;   // 캐시(없어도 되지만 성능/가독성 위해)
    private InputAction jump;   // 예시: 점프 액션(Controls에 Jump가 있으니 활용)

    public override void OnBegin(PlayerController owner)
    {
        // Controls의 액션은 'owner.Player'에서만 가져온다 (단일 소유 원칙)
        move = owner.Player.Move;
        jump = owner.Player.Jump;
    }

    public override void OnUpdate(PlayerController owner)
    {
        // 이동은 항상 컨트롤러 메서드로 일원화
        owner.ApplyMovement();

        // 입력 0이면 Idle로 복귀
        if (move.ReadValue<Vector2>().sqrMagnitude < 0.0001f)
        {
            Set<IdleState>();
            return;
        }

        // 예시: 점프가 이번 프레임 눌렸다면 다른 상태로 전이 가능
        if (jump != null && jump.WasPressedThisFrame())
        {
            // Set<JumpState>(); // 점프 상태가 있다면 이렇게
            Set<IdleState>();    // 여기서는 샘플로 Idle로만 복귀
            return;
        }
        
    }

    public override void OnEnd(PlayerController owner)
    {
        Debug.Log("Exiting Move State");
    }
}