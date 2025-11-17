using UnityEngine;

public class AirState : State<PlayerController>
{
    public override void OnBegin(PlayerController owner)
    {
        owner.EnterAir();
    }

    public override void OnUpdate(PlayerController owner)
    {
        owner.ApplyMovement();

        // 🔹 짧은 점프 처리 (버튼 뗐을 때 + 위로 날아가는 중일 때만)
        if (owner.ConsumeJumpReleased() && owner.Rb.linearVelocity.y > 0f)
            owner.CutJumpEarly();

        // 🔹 벽점프 (우선 처리)
        if (owner.JumpThisFrame && owner.IsTouchingWall(out Vector2 wallNormal))
        {
            owner.DoWallJump(wallNormal);
            return;
        }

        // 🔹 공중 점프 (더블 점프)
        if (owner.JumpThisFrame && owner.TryAirJump())
            return;

        // 🔹 벽 슬라이드 진입
        if (owner.CanWallStickAgain() && owner.IsTouchingWall(out _))
        {
            Set<WallStickState>();
            return;
        }

        // 🔹 착지
        if (owner.IsGrounded() && owner.Rb.linearVelocity.y <= 0.01f)
        {
            if (Mathf.Abs(owner.MoveInput.x) > 0.01f)
                Set<MoveState>();
            else
                Set<IdleState>();
        }
    }
}