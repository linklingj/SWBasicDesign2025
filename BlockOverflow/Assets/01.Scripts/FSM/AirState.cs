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

        // --------------------------------------
        // 🔹 짧은 점프 처리 (절대 첫 프레임 컷 금지!)
        // --------------------------------------
        if (owner.ConsumeJumpReleased())
        {
            // 첫 프레임에는 컷 불가
            if (Time.frameCount != owner.jumpStartFrame &&
                owner.Rb.linearVelocity.y > 0f)
            {
                owner.CutJumpEarly();
            }
        }

        // --------------------------------------
        // 🔹 벽점프 (JumpThisFrame만 사용)
        // --------------------------------------
        if (owner.JumpThisFrame && owner.IsTouchingWall(out Vector2 wallNormal))
        {
            owner.DoWallJump(wallNormal);
            return;
        }

        // --------------------------------------
        // 🔹 공중 점프 (더블 점프)
        // --------------------------------------
        if (owner.JumpThisFrame && owner.TryAirJump())
            return;

        // --------------------------------------
        // 🔹 벽 슬라이드
        // --------------------------------------
        if (owner.CanWallStickAgain() && owner.IsTouchingWall(out _))
        {
            Set<WallStickState>();
            return;
        }

        // --------------------------------------
        // 🔹 착지
        // --------------------------------------
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