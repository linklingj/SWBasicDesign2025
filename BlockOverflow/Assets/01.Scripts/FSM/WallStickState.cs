using UnityEngine;

public class WallStickState : State<PlayerController>
{
    private Vector2 cachedWallNormal;

    public override void OnBegin(PlayerController owner)
    {
        // 현재 벽의 법선 캐시
        if (!owner.IsTouchingWall(out cachedWallNormal))
        {
            Set<AirState>();
            return;
        }

        owner.BeginWallStick();
    }

    public override void OnUpdate(PlayerController owner)
    {
        // 계속 벽에 붙어있는지 확인
        if (!owner.IsTouchingWall(out cachedWallNormal))
        {
            Set<AirState>();
            return;
        }

        // 사격 가능
        if (owner.Player.Attack.WasPressedThisFrame())
        {
            owner.Fire();
        }

        // 벽점프
        if (owner.Player.Jump.WasPressedThisFrame())
        {
            owner.DoWallJump(cachedWallNormal);
            Set<AirState>();
            return;
        }

        // 3초 만료 시 떨어지고, 재부착 락아웃
        owner.TickWallStick();
        if (owner.IsWallStickExpired())
        {
            owner.BreakWallStickUntilLand();
            Set<AirState>();
            return;
        }

        // 아래로 미끄러지는 느낌 유지(필요하면 여기서 y속도 clamp)
        // owner.Rb.velocity = new Vector2(0f, Mathf.Max(owner.Rb.velocity.y, -2f));
    }

    public override void OnEnd(PlayerController owner)
    {
        // 종료 시 별도 처리 없음
    }
}