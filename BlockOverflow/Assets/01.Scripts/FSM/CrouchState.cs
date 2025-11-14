using UnityEngine;

public class CrouchState : State<PlayerController>
{
    public override void OnBegin(PlayerController owner)
    {
        owner.StartCrouch();
    }

    public override void OnUpdate(PlayerController owner)
    {
        owner.ApplyMovement();

        if (!owner.crouchHeld)
        { owner.EndCrouch(); Set<IdleState>(); return; }

        if (owner.ConsumeJumpPressed() && owner.TryGroundOrBufferedJump())
        { owner.EnterAir(); Set<AirState>(); return; }

        if (!owner.IsGrounded())
        { owner.EnterAir(); Set<AirState>(); return; }
    }

    public override void OnEnd(PlayerController owner)
    {
        owner.EndCrouch();
    }
}