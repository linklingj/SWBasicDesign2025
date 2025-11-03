using UnityEngine;
using UnityEngine.InputSystem;

public class AirState : State<PlayerController>
{
    public override void OnBegin(PlayerController owner)
    {
        // 진입 시 공중 점프 리필
        owner.EnterAir();
    }

    public override void OnUpdate(PlayerController owner)
    {
        // ✅ 착지 먼저 판정 (1프레임이라도 땅 닿으면 지상 전환)
        if (owner.IsGrounded() && owner.Rb.linearVelocity.y <= 0.01f)
        {
            if (owner.GetMoveInput().sqrMagnitude > 0.0001f)
                Set<MoveState>();
            else
                Set<IdleState>();
            return;
        }

        // 이동 (공중 제어 포함)
        owner.ApplyMovement();

        // 점프키 짧게 떼면 상승 감쇠
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasReleasedThisFrame)
            owner.CutJumpEarly();

        // 공중 점프
        if (owner.Player.Jump.WasPressedThisFrame())
            owner.TryAirJump();

        // 벽 매달림
        if (owner.CanWallStickAgain() && owner.IsTouchingWall(out _))
        {
            Set<WallStickState>();
            return;
        }

        // 공격
        if (owner.Player.Attack.WasPressedThisFrame())
            owner.Fire();
    }
}