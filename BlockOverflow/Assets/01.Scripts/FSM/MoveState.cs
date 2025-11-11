using UnityEngine;

public class MoveState : State<PlayerController>
{
    public override void OnBegin(PlayerController owner)
    {
        owner.EndCrouch();
    }

    public override void OnUpdate(PlayerController owner)
    {
        owner.ApplyMovement();

        if (Mathf.Abs(owner.GetMoveInput().x) < 0.01f) { Set<IdleState>(); return; }
        if (owner.crouchHeld && owner.IsGrounded()) { Set<CrouchState>(); return; }

        if (owner.ConsumeJumpPressed() && owner.TryGroundOrBufferedJump())
        { Set<AirState>(); return; }

        if (!owner.IsGrounded()) { Set<AirState>(); return; }

        if (owner.ConsumeAttackPressed()) owner.Fire();
    }
}