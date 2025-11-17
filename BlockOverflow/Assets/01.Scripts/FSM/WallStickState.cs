using UnityEngine;

public class WallStickState : State<PlayerController>
{
    private Vector2 wallNormal;

    public override void OnBegin(PlayerController owner)
    {
        // 처음 벽에 붙을 때 노멀 저장 + 속도/타이머 처리
        if (owner.IsTouchingWall(out wallNormal))
            owner.BeginWallStick();
    }

    public override void OnUpdate(PlayerController owner)
    {
        owner.TickWallStick();

        // 매 프레임 현재 벽 노멀 갱신 (벽 모서리에서 이동하는 경우 대비)
        if (!owner.IsTouchingWall(out wallNormal))
        {
            // 더 이상 벽이 아니면 떨어짐 처리
            owner.BreakWallStickUntilLand();
            owner.EnterAir();
            Set<AirState>();
            return;
        }

        // 🔥 벽점프 : 이번 프레임에 점프를 눌렀다면 벽 방향 반대로 튕겨내기
        if (owner.JumpThisFrame)
        {
            owner.DoWallJump(wallNormal); // ← 공중점프 말고, 노멀 기반 벽점프
            owner.EnterAir();
            Set<AirState>();
            return;
        }

        // 스틱 시간이 끝나면 그냥 떨어지기
        if (owner.IsWallStickExpired())
        {
            owner.BreakWallStickUntilLand();
            owner.EnterAir();
            Set<AirState>();
            return;
        }

        // 착지했으면 지상 상태로 복귀
        if (owner.IsGrounded())
        {
            owner.ClearWallStickLockoutOnLand();
            Set<IdleState>();
        }
    }
}