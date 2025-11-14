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

        if (owner.ConsumeJumpReleased()) owner.CutJumpEarly();

        if (owner.ConsumeJumpPressed() && owner.TryAirJump()) return;

        if (owner.CanWallStickAgain() && owner.IsTouchingWall(out _))
        { Set<WallStickState>(); return; }

        if (owner.IsGrounded() && owner.Rb.linearVelocity.y <= 0.01f)
        {
            if (Mathf.Abs(owner.GetMoveInput().x) > 0.01f)
                Set<MoveState>();
            else
                Set<IdleState>();
        }
    }
}