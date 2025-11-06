using UnityEngine;
using UnityEngine.InputSystem;

public class CrouchState : State<PlayerController>
{
    public override void OnBegin(PlayerController owner)
    {
        owner.StartCrouch();
    }

    public override void OnUpdate(PlayerController owner)
    {
        // 이동 (느리게)
        owner.ApplyMovement();

        // 숙이기 해제
        if (!owner.IsPressingDown())
        {
            owner.EndCrouch();
            Set<IdleState>();
            return;
        }

        // 점프
        if (owner.Player.Jump.WasPressedThisFrame())
        {
            if (owner.TryGroundOrBufferedJump())
            {
                owner.EnterAir();
                Set<AirState>();
                return;
            }
        }

        // 낙하
        if (!owner.IsGrounded())
        {
            owner.EnterAir();
            Set<AirState>();
            return;
        }

        // 공격
        if (owner.Player.Attack.WasPressedThisFrame())
            owner.Fire();
    }

    public override void OnEnd(PlayerController owner)
    {
        owner.EndCrouch();
    }
}