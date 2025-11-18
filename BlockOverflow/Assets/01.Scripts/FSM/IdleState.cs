using UnityEngine;

public class IdleState : State<PlayerController>
{
    public override void OnBegin(PlayerController owner)
    {
        owner.StopImmediately();
        owner.EndCrouch();
    }

    public override void OnUpdate(PlayerController owner)
    {
        if (owner.crouchHeld) { Set<CrouchState>(); return; }
        if (Mathf.Abs(owner.MoveInput.x) > 0.01f) { Set<MoveState>(); return; }

        if (owner.JumpThisFrame && owner.TryGroundOrBufferedJump())
        { Set<AirState>(); return; }

        if (!owner.IsGrounded()) { Set<AirState>(); return; }
    }
}