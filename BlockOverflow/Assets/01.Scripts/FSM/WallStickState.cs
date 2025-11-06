using UnityEngine;
using UnityEngine.InputSystem;

public class WallStickState : State<PlayerController>
{
    private Vector2 wallNormal;

    public override void OnBegin(PlayerController owner)
    {
        if (owner.IsTouchingWall(out wallNormal))
            owner.BeginWallStick();
    }

    public override void OnUpdate(PlayerController owner)
    {
        owner.TickWallStick();

        // 점프 → 벽점프
        if (owner.Player.Jump.WasPressedThisFrame())
        {
            owner.DoWallJump(wallNormal);
            owner.EnterAir();
            Set<AirState>();
            return;
        }

        // 공격 가능
        if (owner.Player.Attack.WasPressedThisFrame())
            owner.Fire();

        // 매달림 종료
        if (owner.IsWallStickExpired() || !owner.IsTouchingWall(out _))
        {
            owner.BreakWallStickUntilLand();
            owner.EnterAir();
            Set<AirState>();
            return;
        }

        // 착지
        if (owner.IsGrounded())
        {
            owner.ClearWallStickLockoutOnLand();
            Set<IdleState>();
            return;
        }
    }
}