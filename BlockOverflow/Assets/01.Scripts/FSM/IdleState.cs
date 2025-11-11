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
        if (owner.crouchHeld && owner.IsGrounded()) { Set<CrouchState>(); return; }
        if (Mathf.Abs(owner.GetMoveInput().x) > 0.01f) { Set<MoveState>(); return; }

        if (owner.ConsumeJumpPressed() && owner.TryGroundOrBufferedJump())
        { Set<AirState>(); return; }

        if (!owner.IsGrounded()) { Set<AirState>(); return; }

        if (owner.ConsumeAttackPressed()) owner.Fire();
    }
}