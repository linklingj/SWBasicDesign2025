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

        // 1) 🔥 벽 점프 최우선
        if (owner.JumpThisFrame && owner.IsTouchingWall(out Vector2 wallNormal))
        {
            owner.DoWallJump(wallNormal);
            owner.ConsumeJumpPress();
            return;
        }

        // 2) 🔥 공중 2단 점프
        if (owner.JumpThisFrame && owner.TryAirJump())
        {
            owner.ConsumeJumpPress();
            return;
        }

        // 3) (선택) 벽에 닿아 있을 때 살짝 스틱 상태로 넘기고 싶다면
        if (owner.IsTouchingWall(out _) && owner.Rb.linearVelocity.y <= 0f)
        {
            Set<WallStickState>();
            return;
        }

        // 4) 짧은 점프 컷
        if (owner.ConsumeJumpReleased())
        {
            if (Time.frameCount != owner.jumpStartFrame &&
                owner.Rb.linearVelocity.y > 0f)
            {
                owner.CutJumpEarly();
            }
        }

        // 5) 착지
        if (owner.IsGrounded() && owner.Rb.linearVelocity.y <= 0.01f)
        {
            owner.hasStartedJump = false;
            if (Mathf.Abs(owner.MoveInput.x) > 0.01f)
                Set<MoveState>();
            else
                Set<IdleState>();
        }
    }
}