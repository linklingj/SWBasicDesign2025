using UnityEngine;
using UnityEngine.InputSystem;

public class MoveState : State<PlayerController>
{
    public override void OnBegin(PlayerController owner)
    {
        owner.EndCrouch();
    }

    public override void OnUpdate(PlayerController owner)
    {
        owner.ApplyMovement();

        // 방향키 입력 없으면 대기
        if (Mathf.Abs(owner.GetMoveInput().x) < 0.01f)
        {
            Set<IdleState>();
            return;
        }

        // 숙이기
        if (owner.IsPressingDown() && owner.IsGrounded())
        {
            Set<CrouchState>();
            return;
        }

        // 점프
        if (owner.Player.Jump.WasPressedThisFrame())
        {
            if (owner.TryGroundOrBufferedJump())
            {
                
                Set<AirState>();
                return;
            }
        }

        // 낙하
        if (!owner.IsGrounded())
        {
            Set<AirState>();
            return;
        }

        // 공격
        if (owner.Player.Attack.WasPressedThisFrame())
            owner.Fire();
    }
}