using UnityEngine;

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

        if (owner.ConsumeJumpPressed())
        {
            owner.DoWallJump(wallNormal);
            owner.EnterAir();
            Set<AirState>();
            return;
        }
        

        if (owner.IsWallStickExpired() || !owner.IsTouchingWall(out _))
        {
            owner.BreakWallStickUntilLand();
            owner.EnterAir();
            Set<AirState>();
            return;
        }

        if (owner.IsGrounded())
        {
            owner.ClearWallStickLockoutOnLand();
            Set<IdleState>();
        }
    }
}