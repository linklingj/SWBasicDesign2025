using UnityEngine;
using UnityEngine.InputSystem;

public class AirState : State<PlayerController>
{
    public override void OnBegin(PlayerController owner)
    {
        // 공중 진입 시점에 이미 EnterAir()가 호출된 상태여야 함(지상에서 넘어올 때)
        // 만약 외부에서 안 불렀다면 안전장치:
        // owner.EnterAir();
    }

    public override void OnUpdate(PlayerController owner)
    {
        // 공중에서도 좌우 이동 가능
        owner.ApplyMovement();

        // 점프키 짧게 눌렀다가 떼면 낮은 점프(가변)
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasReleasedThisFrame)
            owner.CutJumpEarly();

        // 공중 점프(1회)
        if (owner.Player.Jump.WasPressedThisFrame())
        {
            if (owner.TryAirJump())
            {
                // 추가 이펙트/사운드 등
            }
        }

        // 벽 접촉 → 벽 매달림(락아웃 아닐 때)
        if (owner.CanWallStickAgain() && owner.IsTouchingWall(out _))
        {
            Set<WallStickState>();
            return;
        }

        // 공중 사격
        if (owner.Player.Attack.WasPressedThisFrame())
        {
            owner.Fire();
        }

        // 착지 → 지상 상태로
        if (owner.IsGrounded() && owner.Rb.linearVelocity.y <= 0.01f)
        {
            // 이동 입력에 따라
            if (owner.GetMoveInput().sqrMagnitude > 0.0001f) Set<MoveState>();
            else Set<IdleState>();
        }
    }
}