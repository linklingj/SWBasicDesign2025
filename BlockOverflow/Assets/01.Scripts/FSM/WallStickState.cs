using UnityEngine;

public class WallStickState : State<PlayerController>
{
    private Vector2 wallNormal;

    public override void OnBegin(PlayerController owner)
    {
        // 벽 노멀 캐시
        owner.IsTouchingWall(out wallNormal);

        // 벽에 붙을 때 수직 속도 0으로 (안 미끄러지게)
        var v = owner.Rb.linearVelocity;
        owner.Rb.linearVelocity = new Vector2(0f, 0f);

        // 벽에 닿은 순간 공중 점프 리필
        owner.airJumpsAvailable = owner.maxAirJumps;
    }

    public override void OnUpdate(PlayerController owner)
    {
        // 더 이상 벽이 아니면 공중 상태로
        if (!owner.IsTouchingWall(out wallNormal))
        {
            owner.EnterAir();
            Set<AirState>();
            return;
        }

        // 계속 벽에 붙어 있을 땐 Y속도 0 유지 (안 미끄러짐)
        owner.Rb.linearVelocity = new Vector2(0f, 0f);

        // 점프 누르면 → 벽 점프
        if (owner.JumpThisFrame)
        {
            owner.DoWallJump(wallNormal);
            owner.ConsumeJumpPress();
            Set<AirState>();
            return;
        }

        // 혹시 바로 아래가 땅이면 지상 상태로
        if (owner.IsGrounded())
        {
            owner.hasStartedJump = false;
            if (Mathf.Abs(owner.MoveInput.x) > 0.01f)
                Set<MoveState>();
            else
                Set<IdleState>();
        }
    }
}